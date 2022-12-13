﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RoslynSpecialType = Microsoft.CodeAnalysis.SpecialType;
using SpecialType = Metalama.Framework.Code.SpecialType;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    public static class SymbolExtensions
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
                RoslynSpecialType.System_Boolean => SpecialType.Boolean,
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
                SpecialType.Boolean => RoslynSpecialType.System_Boolean,
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
        {
            if ( namedType.IsUnboundGenericType )
            {
                return true;
            }

            if ( namedType.TypeArguments.Length != namedType.TypeParameters.Length )
            {
                return false;
            }

            foreach ( var t in namedType.TypeArguments )
            {
                if ( t is ITypeParameterSymbol p )
                {
                    if ( !p.ContainingSymbol.Equals( namedType ) )
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

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
                r => r.GetSyntax() is MemberDeclarationSyntax member && member.Modifiers.Any( m => m.IsKind( kind ) ) );
        }

        public static SyntaxReference? GetPrimarySyntaxReference( this ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return null;
            }

            static SyntaxReference? GetReferenceOfShortestPath( ISymbol s, Func<SyntaxReference, bool>? filter = null )
            {
                if ( s.DeclaringSyntaxReferences.IsDefaultOrEmpty )
                {
                    return null;
                }
                else
                {
                    var references =
                        filter != null
                            ? s.DeclaringSyntaxReferences.Where( filter )
                            : s.DeclaringSyntaxReferences;

                    return references.OrderBy( x => x.SyntaxTree.FilePath.Length ).First();
                }
            }

            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: not null } methodSymbol:
                    return GetReferenceOfShortestPath( symbol ) ?? GetReferenceOfShortestPath( methodSymbol.AssociatedSymbol );

                case IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: { } partialDefinitionSymbol }:
                    return GetReferenceOfShortestPath( partialDefinitionSymbol );

                case IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: null }:
                    return GetReferenceOfShortestPath( symbol );

                default:
                    return GetReferenceOfShortestPath( symbol );
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

        public static IFieldSymbol? GetBackingField( this IPropertySymbol property )
            => (IFieldSymbol?) property.ContainingType.GetMembers( $"<{property.Name}>k__BackingField" ).SingleOrDefault();

        public static IFieldSymbol? GetBackingField( this IEventSymbol @event )

            // TODO: Currently Roslyn does not expose the event field in the symbol model and therefore we cannot find it.
            => null;

        public static ISymbol? Translate( this ISymbol? symbol, Compilation? originalCompilation, Compilation compilation )
        {
            if ( symbol == null )
            {
                return null;
            }
            else if ( originalCompilation == compilation )
            {
                return symbol;
            }
            else
            {
                return SymbolTranslator.GetInstance( compilation ).Translate( symbol );
            }
        }

        internal static SymbolId GetSymbolId( this ISymbol? symbol ) => SymbolId.Create( symbol );

        public static bool HasDefaultConstructor( this INamedTypeSymbol type )
            => type.TypeKind == TypeKind.Struct ||
               (type is { TypeKind: TypeKind.Class, IsAbstract: false } &&
                type.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        public static bool IsVisibleTo( this ISymbol symbol, Compilation compilation, ISymbol otherSymbol )
        {
            return compilation.IsSymbolAccessibleWithin(
                symbol,
                otherSymbol switch
                {
                    INamedTypeSymbol type => type,
                    _ => otherSymbol.ContainingType
                } );
        }

        public static FrameworkName? GetTargetFramework( this Compilation compilation )
        {
            var attribute = compilation.Assembly.GetAttributes().FirstOrDefault( a => a.AttributeClass?.Name == nameof(TargetFrameworkAttribute) );

            if ( attribute == null || attribute.ConstructorArguments.IsDefaultOrEmpty )
            {
                return null;
            }

            var frameworkNameString = (string?) attribute.ConstructorArguments[0].Value;

            if ( frameworkNameString == null )
            {
                return null;
            }

            return new FrameworkName( frameworkNameString );
        }

        public static bool IsCompilerGenerated( this ISymbol declaration )
        {
            return declaration.GetAttributes().Any( a => a.AttributeConstructor?.ContainingType.Name == nameof(CompilerGeneratedAttribute) );
        }

        /// <summary>
        /// Gets the kind of operator based represented by the method.
        /// </summary>
        public static OperatorKind GetOperatorKind( this IMethodSymbol method ) => SymbolHelpers.GetOperatorKindFromName( method.Name );

        public static INamedTypeSymbol GetTopmostContainingType( this INamedTypeSymbol type )
            => type.ContainingType == null ? type : type.ContainingType.GetTopmostContainingType();

        public static INamedTypeSymbol? GetClosestContainingType( this ISymbol symbol )
            => symbol switch
            {
                INamedTypeSymbol type => type,
                _ => symbol.ContainingType
            };
    }
}