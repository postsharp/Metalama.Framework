// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal static class SymbolExtensions
    {
        public static bool AnyBaseType( this INamedTypeSymbol type, Predicate<INamedTypeSymbol> predicate )
        {
            for ( var t = type; t != null; t = t.BaseType )
            {
                if ( predicate( t ) )
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<INamedTypeSymbol> GetTypes( this IAssemblySymbol assembly ) => assembly.GlobalNamespace.GetTypes();

        private static IEnumerable<INamedTypeSymbol> GetTypes( this INamespaceSymbol ns )
        {
            foreach ( var type in ns.GetTypeMembers() )
            {
                yield return type;
            }

            foreach ( var namespaceMember in ns.GetNamespaceMembers() )
            {
                foreach ( var type in namespaceMember.GetTypes() )
                {
                    yield return type;
                }
            }
        }

        public static ITypeSymbol? GetTypeByReflectionType( this Compilation compilation, Type type )
            => ReflectionMapper.GetInstance( compilation ).GetTypeSymbol( type );

        public static bool IsMemberOf( this ISymbol member, INamedTypeSymbol type )
        {
            if ( member.ContainingType == null )
            {
                return false;
            }

            if ( SymbolEqualityComparer.Default.Equals( member.ContainingType, type ) )
            {
                return true;
            }

            if ( type.BaseType != null && member.IsMemberOf( type.BaseType ) )
            {
                return true;
            }

            return false;
        }

        public static bool Is( this ITypeSymbol left, Type right )
        {
            if ( right.IsGenericType )
            {
                throw new ArgumentOutOfRangeException( nameof(right), "This method does not work with generic types." );
            }

            if ( left is IErrorTypeSymbol )
            {
                return false;
            }

            var rightName = right.FullName;

            if ( left.GetReflectionName() == rightName )
            {
                return true;
            }
            else if ( left.BaseType != null && left.BaseType.Is( right ) )
            {
                return true;
            }
            else
            {
                foreach ( var i in left.Interfaces )
                {
                    if ( i.Is( right ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // TODO: Partial methods etc.

        public static SyntaxReference? GetPrimarySyntaxReference( this ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()
                           ?? methodSymbol.AssociatedSymbol!.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault();

                default:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault();
            }
        }

        public static SyntaxNode? GetPrimaryDeclaration( this ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()?.GetSyntax()
                           ?? methodSymbol.AssociatedSymbol!.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()?.GetSyntax();

                default:
                    return symbol.DeclaringSyntaxReferences.OrderBy( x => x.SyntaxTree.FilePath.Length ).FirstOrDefault()?.GetSyntax();
            }
        }
    }
}