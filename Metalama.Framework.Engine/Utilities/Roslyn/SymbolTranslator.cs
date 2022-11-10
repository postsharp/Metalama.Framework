﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal class SymbolTranslator
{
    private static readonly WeakCache<Compilation, SymbolTranslator> _instances = new();

    private readonly ConcurrentDictionary<ISymbol, ISymbol?> _cache = new( SymbolEqualityComparer.Default );
    private readonly Compilation _targetCompilation;
    private readonly Visitor _visitor;

    public static SymbolTranslator GetInstance( Compilation targetCompilation ) => _instances.GetOrAdd( targetCompilation, t => new SymbolTranslator( t ) );

    private SymbolTranslator( Compilation targetCompilation )
    {
        this._targetCompilation = targetCompilation;
        this._visitor = new Visitor( this );
    }

    public T? Translate<T>( T symbol )
        where T : ISymbol
        => (T?) this._cache.GetOrAdd( symbol, this.TranslateCore );

    private ISymbol? TranslateCore( ISymbol symbol ) => this._visitor.Visit( symbol );

    private class Visitor : SymbolVisitor<ISymbol>
    {
        private readonly SymbolTranslator _parent;

        public Visitor( SymbolTranslator parent )
        {
            this._parent = parent;
        }

        public override ISymbol? DefaultVisit( ISymbol symbol ) => throw new NotSupportedException( $"Cannot map a symbol of kind '{symbol.Kind}'." );

        public override ISymbol? VisitArrayType( IArrayTypeSymbol symbol )
        {
            var elementType = this._parent.Translate( symbol.ElementType );

            if ( elementType == null )
            {
                return null;
            }

            return this._parent._targetCompilation.CreateArrayTypeSymbol( elementType, symbol.Rank );
        }

        public override ISymbol? VisitDynamicType( IDynamicTypeSymbol symbol ) => this._parent._targetCompilation.DynamicType;

        private ISymbol? TranslateUniquelyNamedTypeMember( ISymbol symbol )
        {
            var namedType = this._parent.Translate( symbol.ContainingType );

            if ( namedType == null )
            {
                return namedType;
            }

            return namedType.GetMembers( symbol.Name ).SingleOrDefault( m => m.Kind == symbol.Kind );
        }

        private ISymbol? TranslateNonUniquelyNamedTypeMember( ISymbol symbol )
        {
            var namedType = this._parent.Translate( symbol.ContainingType );

            if ( namedType == null )
            {
                return namedType;
            }

            var candidates = namedType.GetMembers( symbol.Name )
                .Where( m => m.Kind == symbol.Kind && StructuralSymbolComparer.Signature.Equals( m, symbol ) )
                .ToList();

            if ( candidates.Count == 1 )
            {
                return candidates[0];
            }
            else
            {
                throw new AssertionFailedException( $"More than one symbol match '{symbol}': {string.Join( ", ", candidates.Select( x => $"'{x}'" ) )}." );
            }
        }

        public override ISymbol? VisitEvent( IEventSymbol symbol ) => this.TranslateUniquelyNamedTypeMember( symbol );

        public override ISymbol? VisitField( IFieldSymbol symbol ) => this.TranslateUniquelyNamedTypeMember( symbol );

        public override ISymbol? VisitMethod( IMethodSymbol symbol )
        {
            if ( symbol.ContainingSymbol is IMethodSymbol )
            {
                throw new NotSupportedException( "Translating a local function is not supported." );
            }

            if ( symbol.PartialImplementationPart != null )
            {
                var partialImplementation = this._parent.Translate( symbol.PartialImplementationPart );

                return partialImplementation?.PartialDefinitionPart;
            }
            else
            {
                var translated = this.TranslateNonUniquelyNamedTypeMember( symbol );

                if ( translated == null )
                {
                    return null;
                }

                if ( symbol.PartialDefinitionPart != null )
                {
                    return ((IMethodSymbol) translated).PartialImplementationPart;
                }
                else
                {
                    return translated;
                }
            }
        }

        public override ISymbol? VisitNamedType( INamedTypeSymbol symbol )
        {
            if ( symbol.IsGenericType && !symbol.IsGenericTypeDefinition() )
            {
                var constructedFrom = this._parent.Translate( symbol.ConstructedFrom );

                if ( constructedFrom == null )
                {
                    return null;
                }

                var typeArguments = new ITypeSymbol[symbol.TypeArguments.Length];

                for ( var i = 0; i < typeArguments.Length; i++ )
                {
                    var typeArgument = this._parent.Translate( symbol.TypeArguments[i] );

                    if ( typeArgument == null )
                    {
                        return null;
                    }

                    typeArguments[i] = typeArgument;
                }

                return constructedFrom.Construct( typeArguments );
            }
            else
            {
                ImmutableArray<INamedTypeSymbol> types;

                if ( symbol.ContainingType != null )
                {
                    var containingType = this._parent.Translate( symbol.ContainingType );

                    if ( containingType == null )
                    {
                        return null;
                    }

                    types = containingType.GetTypeMembers( symbol.Name, symbol.Arity );
                }
                else if ( symbol.ContainingNamespace != null )
                {
                    var ns = this._parent.Translate( symbol.ContainingNamespace );

                    if ( ns == null )
                    {
                        return null;
                    }

                    types = ns.GetTypeMembers( symbol.Name, symbol.Arity );
                }
                else
                {
                    throw new AssertionFailedException( $"Unexpected containing declaration of type '{symbol}'." );
                }

                if ( types.IsDefaultOrEmpty )
                {
                    return null;
                }

                if ( types.Length > 1 )
                {
                    throw new AssertionFailedException( $"More than one type named '{symbol.Name}' in '{symbol.ContainingSymbol}'." );
                }

                return types[0];
            }
        }

        public override ISymbol? VisitNamespace( INamespaceSymbol symbol )
        {
            if ( symbol.IsGlobalNamespace )
            {
                switch ( symbol.NamespaceKind )
                {
                    case NamespaceKind.Assembly:
                        var assembly = this._parent.Translate( symbol.ContainingAssembly );

                        if ( assembly == null )
                        {
                            return null;
                        }

                        return assembly.GlobalNamespace;

                    case NamespaceKind.Module:
                        var module = this._parent.Translate( symbol.ContainingModule );

                        if ( module == null )
                        {
                            return null;
                        }

                        return module.GlobalNamespace;

                    case NamespaceKind.Compilation:
                        return this._parent._targetCompilation.GlobalNamespace;

                    default:
                        throw new AssertionFailedException( $"Unexpected NamespaceKind {symbol.NamespaceKind} for '{symbol}'." );
                }
            }
            else
            {
                var ns = this._parent.Translate( symbol.ContainingNamespace );

                if ( ns == null )
                {
                    return null;
                }

                return ns.GetNamespaceMembers().SingleOrDefault( x => x.Name == symbol.Name );
            }
        }

        public override ISymbol? VisitParameter( IParameterSymbol symbol )
        {
            var containingSymbol = this._parent.Translate( symbol.ContainingSymbol );

            ImmutableArray<IParameterSymbol> parameters;

            switch ( containingSymbol )
            {
                case null:
                    return null;

                case IMethodSymbol method:
                    parameters = method.Parameters;

                    break;

                case IPropertySymbol property:
                    parameters = property.Parameters;

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected parent of parameter: {containingSymbol.Kind}." );
            }

            if ( parameters.Length <= symbol.Ordinal )
            {
                return null;
            }

            return parameters[symbol.Ordinal];
        }

        public override ISymbol? VisitPointerType( IPointerTypeSymbol symbol )
        {
            var pointedAtType = this._parent.Translate( symbol.PointedAtType );

            if ( pointedAtType == null )
            {
                return null;
            }

            return this._parent._targetCompilation.CreatePointerTypeSymbol( pointedAtType );
        }

        public override ISymbol? VisitProperty( IPropertySymbol symbol ) => this.TranslateNonUniquelyNamedTypeMember( symbol );

        public override ISymbol? VisitTypeParameter( ITypeParameterSymbol symbol )
        {
            var containingSymbol = this._parent.Translate( symbol.ContainingSymbol );

            ImmutableArray<ITypeParameterSymbol> parameters;

            switch ( containingSymbol )
            {
                case null:
                    return null;

                case IMethodSymbol method:
                    parameters = method.TypeParameters;

                    break;

                case INamedTypeSymbol namedType:
                    parameters = namedType.TypeParameters;

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected parent of type parameter: {containingSymbol.Kind}." );
            }

            if ( parameters.Length <= symbol.Ordinal )
            {
                return null;
            }

            return parameters[symbol.Ordinal];
        }

        public override ISymbol? VisitAssembly( IAssemblySymbol symbol )
        {
            return this._parent._targetCompilation.GetAssembly( symbol.Identity );
        }

        public override ISymbol? VisitModule( IModuleSymbol symbol )
        {
            var assembly = this._parent.Translate( symbol.ContainingAssembly );

            if ( assembly == null )
            {
                return null;
            }

            return assembly.Modules.SingleOrDefault( m => m.Name == symbol.Name );
        }
    }
}