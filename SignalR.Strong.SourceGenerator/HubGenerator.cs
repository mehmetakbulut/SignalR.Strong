﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Strong.SourceGenerator
{
    [Generator]
    public class HubGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
            
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var body = new StringBuilder(@"// <auto-generated />
// Generated by SignalR.Strong.SourceGenerator
#nullable enable
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    /// <summary>
    /// Source-generated extension methods for HubConnection
    /// </summary>
    public static partial class HubConnectionExtensionsGenerated
    {
        /// <summary>
        /// Get a strongly-typed hub
        /// </summary>
        public static THub AsGeneratedHub<THub>(this HubConnection conn)
        {
            return conn.makeNewHub<THub>();
        }
    }
}

// <auto-generated />
namespace SignalR.Strong.SourceGenerated
{");

            // Collect all the symbols for hub interfaces based on type argument to AsGeneratedHub<T> calls
            var hubSymbols = new Dictionary<string, ITypeSymbol>();
            var syntaxTrees = context.Compilation.SyntaxTrees;
            
            foreach (SyntaxTree tree in syntaxTrees)
            {
                var nodes = tree.GetRoot().DescendantNodes();
                var model = context.Compilation.GetSemanticModel(tree);

                var invocations = nodes.OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax syntax
                        || syntax.Name is not GenericNameSyntax call) continue;
                    
                    // TODO: Check if receiver/1st-parameter symbol is HubConnection
                    
                    if (!call.ToString().StartsWith("AsGeneratedHub") || call.Arity != 1 ||
                        call.TypeArgumentList.Arguments[0] is not IdentifierNameSyntax typeArg) continue;
                    
                    if (model.GetSymbolInfo(typeArg).Symbol is not ITypeSymbol {IsAbstract: true} symbol) continue;
                    
                    hubSymbols[symbol.Name] = symbol;
                }
            }
            
            // Generate proxy classes for all the hub interfaces detected previously
            foreach (var hubSymbol in hubSymbols.Values)
            {
                var typeName = $"Generated{hubSymbol.Name}";
                body.AppendLine($@"
    /// <summary>
    /// Source-generated proxy class for {hubSymbol.Name}
    /// </summary>
    public sealed class {typeName} : {hubSymbol.ToString()}
    {{
        private readonly Microsoft.AspNetCore.SignalR.Client.HubConnection conn;

        /// <summary>
        /// Do not construct this type manually
        /// </summary>
        public {typeName}(Microsoft.AspNetCore.SignalR.Client.HubConnection connection)
        {{
            this.conn = connection;
        }}");
                var members = hubSymbol
                    .GetAllInterfaceMethods(true)
                    .Where(member => member.Kind == SymbolKind.Method)
                    .Select(member => member);
                
                foreach (var member in members)
                {
                    var signature = new StringBuilder($"public {member.ReturnType.ToString()} {member.Name}(");
                    var first = true;

                    var callArgs = new StringBuilder("");

                    foreach (var parameter in member.Parameters)
                    {
                        if (!first)
                        {
                            signature.Append(", ");
                        }

                        first = false;
                        signature.Append($"{parameter.Type.ToString()} {parameter.Name}");

                        callArgs.Append($", {parameter.Name}");
                    }

                    signature.Append(")");

                    var precall = "";
                    var postcall = "";
                    
                    if (member.ReturnType.ToString() != "void")
                    {
                        precall = "return ";
                    }
                    
                    var call = $"{precall}this.conn.{getSpecificCall(member)}(\"{member.Name}\"{callArgs.ToString()}){postcall};";

                    body.AppendLine($@"
        /// <summary>
        /// Source-generated proxy method for {hubSymbol.Name}.{member.Name}
        /// </summary>
        {signature}
        {{
            {call}
        }}");
                }

                body.AppendLine(@"
    }");
            }
            
            body.AppendLine(@"
}");

            // Generate a method that will dispatch AsGeneratedHub<T> calls to appropriate proxy constructors
            body.AppendLine(@"
namespace SignalR.Strong
{
    /// <summary>
    /// Source-generated extension methods for HubConnection
    /// </summary>
    public static partial class HubConnectionExtensionsGenerated
    {
        private static THub makeNewHub<THub>(this HubConnection conn)
        {");
            foreach (var hubSymbol in hubSymbols.Values)
            {
                var typeName = $"SignalR.Strong.SourceGenerated.Generated{hubSymbol.Name}";
                body.AppendLine($@"
            if(typeof(THub) == typeof({hubSymbol})) return (THub) ({hubSymbol}) new {typeName}(conn);");
            }

            body.AppendLine(@"
            throw new System.ArgumentException();
        }
    }
}");
            
            context.AddSource(nameof(HubGenerator) + ".g.cs",
                SourceText.From(body.ToString(), Encoding.UTF8));
        }

        private string getSpecificCall(IMethodSymbol member)
        {
            if (member.ReturnType is INamedTypeSymbol { Arity: 1, Name: "Task"} a
                && a.TypeArguments[0] is INamedTypeSymbol { Arity: 1, Name: "ChannelReader"} b)
            {
                return $"StreamAsChannelAsync<{b.TypeArguments[0]}>";
            }

            if (member.ReturnType is INamedTypeSymbol { Arity: 1, Name: "IAsyncEnumerable"} c)
            {
                return $"StreamAsync<{c.TypeArguments[0]}>";
            }

            if (!member.ReturnType.ToString().Contains("System.Threading.Tasks.Task"))
            {
                return "SendAsync";
            }
            
            if (member.Parameters.Any(symbol =>
                symbol.Type.ToString().Contains("ChannelReader<")
                || symbol.Type.ToString().Contains("IAsyncEnumerable<")))
            {
                return "SendAsync";
            }
            
            if (member.ReturnType is INamedTypeSymbol {Arity: 1, Name: "Task"} d)
            {
                return $"InvokeAsync<{d.TypeArguments[0]}>";
            }

            if (member.ReturnType is INamedTypeSymbol {Arity: 0, Name: "Task"})
            {
                return "InvokeAsync";
            }

            throw new InvalidOperationException();
        }
    }
}