using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.DelegateAllocation

namespace SignalR.Strong.SourceGenerator
{
    public static class TypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> GetAllInterfaces(this ITypeSymbol typeSymbol, bool includeTop = false)
            => Enumerable
                .Repeat(typeSymbol, includeTop ? 1 : 0)
                .Concat(typeSymbol.AllInterfaces)
                .Where(symbol => symbol.TypeKind == TypeKind.Interface);

        public static IEnumerable<IMethodSymbol> GetAllInterfaceMethods(this ITypeSymbol typeSymbol, bool includeTop = false)
        {
            return typeSymbol
                .GetAllInterfaces(includeTop)
                .SelectMany(x => x.GetMembers())
                .Where(x => x.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .GroupBy(x => x.Name)
                .Preview(g =>
                {
                    var byArity = g.GroupBy(x => x.Arity);
                    if (byArity.Count() > 1)
                        throw new NotSupportedException(
                            $"Not Supported Overloaded Methods with the same name \"{g.Key}\""
                        );
                })
                .Select(g =>
                {
                    return g.Aggregate((a, b) =>
                    {
                        if (a.Equals(b, SymbolEqualityComparer.Default))
                            throw new NotSupportedException(
                                $"Not Supported Overloaded Methods with the same name \"{g.Key}\" and different signatures"
                            );
                        return a;
                    });
                });
        }

        private static IEnumerable<T> Preview<T>(this IEnumerable<T> items, Action<T> onItem)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (onItem == null) throw new ArgumentNullException(nameof(onItem));
            
            return items.Select(item =>
            {
                onItem(item);
                return item;
            });
        }
    }
}