// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using RoslynSpecialType = Microsoft.CodeAnalysis.SpecialType;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class SymbolExtensions
    {
        // Coverage: ignore
        public static SpecialType ToOurSpecialType( this RoslynSpecialType type )
            => type switch
            {
                RoslynSpecialType.System_Byte => SpecialType.Byte,
                RoslynSpecialType.System_SByte => SpecialType.SByte,
                RoslynSpecialType.System_Int16 => SpecialType.Int16,
                RoslynSpecialType.System_Int32 => SpecialType.Int32,
                RoslynSpecialType.System_Int64 => SpecialType.Int64,
                RoslynSpecialType.System_UInt16 => SpecialType.UInt16,
                RoslynSpecialType.System_UInt32 => SpecialType.UInt32,
                RoslynSpecialType.System_UInt64 => SpecialType.UInt64,
                RoslynSpecialType.System_String => SpecialType.String,
                RoslynSpecialType.System_Decimal => SpecialType.Decimal,
                RoslynSpecialType.System_Single => SpecialType.Single,
                RoslynSpecialType.System_Double => SpecialType.Double,
                RoslynSpecialType.System_Object => SpecialType.Object,
                RoslynSpecialType.System_Void => SpecialType.Void,
                RoslynSpecialType.System_Collections_IEnumerable => SpecialType.IEnumerable,
                RoslynSpecialType.System_Collections_IEnumerator => SpecialType.IEnumerator,
                RoslynSpecialType.System_Collections_Generic_IEnumerable_T => SpecialType.IEnumerable_T,
                RoslynSpecialType.System_Collections_Generic_IEnumerator_T => SpecialType.IEnumerator_T,
                _ => SpecialType.None
            };

        public static RoslynSpecialType ToRoslynSpecialType( this SpecialType type )
            => type switch
            {
                SpecialType.Byte => RoslynSpecialType.System_Byte,
                SpecialType.SByte => RoslynSpecialType.System_SByte,
                SpecialType.Int16 => RoslynSpecialType.System_Int16,
                SpecialType.Int32 => RoslynSpecialType.System_Int32,
                SpecialType.Int64 => RoslynSpecialType.System_Int64,
                SpecialType.UInt16 => RoslynSpecialType.System_UInt16,
                SpecialType.UInt32 => RoslynSpecialType.System_UInt32,
                SpecialType.UInt64 => RoslynSpecialType.System_UInt64,
                SpecialType.String => RoslynSpecialType.System_String,
                SpecialType.Decimal => RoslynSpecialType.System_Decimal,
                SpecialType.Single => RoslynSpecialType.System_Single,
                SpecialType.Double => RoslynSpecialType.System_Double,
                SpecialType.Object => RoslynSpecialType.System_Object,
                SpecialType.Void => RoslynSpecialType.System_Void,
                SpecialType.IEnumerable => RoslynSpecialType.System_Collections_IEnumerable,
                SpecialType.IEnumerator => RoslynSpecialType.System_Collections_IEnumerator,
                SpecialType.IEnumerable_T => RoslynSpecialType.System_Collections_Generic_IEnumerable_T,
                SpecialType.IEnumerator_T => RoslynSpecialType.System_Collections_Generic_IEnumerator_T,

                // Note that we have special types that Roslyn does not have.
                _ => RoslynSpecialType.None
            };

        public static bool IsGenericTypeDefinition( this INamedTypeSymbol namedType )
            => namedType.IsUnboundGenericType || namedType.TypeArguments.Any( a => a is ITypeParameterSymbol );

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

            if ( type.BaseType != null )
            {
                return member.IsMemberOf( type.BaseType );
            }

            return false;
        }

        public static bool Is( this ITypeSymbol left, ITypeSymbol right )
        {
            if ( left is IErrorTypeSymbol )
            {
                return false;
            }

            if ( SymbolEqualityComparer.Default.Equals( left, right ) )
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

        public static bool IsAccessor( this IMethodSymbol method )
            => method.MethodKind switch
            {
                MethodKind.PropertyGet => true,
                MethodKind.PropertySet => true,
                MethodKind.EventAdd => true,
                MethodKind.EventRemove => true,
                MethodKind.EventRaise => true,
                _ => false
            };

        public static bool HasModifier( this ISymbol symbol, SyntaxKind kind )
        {
            if ( symbol.DeclaringSyntaxReferences.IsEmpty )
            {
                throw new ArgumentOutOfRangeException();
            }

            return symbol.DeclaringSyntaxReferences.Any(
                r => r.GetSyntax() is MemberDeclarationSyntax member && member.Modifiers.Any( m => m.Kind() == kind ) );
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

        public static SyntaxNode? GetPrimaryDeclaration( this ISymbol symbol ) => symbol.GetPrimarySyntaxReference()?.GetSyntax();

        public static bool IsInterfaceMemberImplementation( this ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.ExplicitInterfaceImplementations.Any(),
                IPropertySymbol propertySymbol => propertySymbol.ExplicitInterfaceImplementations.Any(),
                IEventSymbol eventSymbol => eventSymbol.ExplicitInterfaceImplementations.Any(),
                _ => false
            };
    }
}