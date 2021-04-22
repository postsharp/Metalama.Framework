// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class SymbolClassifier : ISymbolClassifier
    {
        private static readonly object _addSync = new();
        private static readonly ConditionalWeakTable<Compilation, ISymbolClassifier> _instances = new();

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _compileTimeAttribute;
        private readonly INamedTypeSymbol _compileTimeOnlyAttribute;
        private readonly INamedTypeSymbol _templateAttribute;
        private readonly Dictionary<ISymbol, SymbolDeclarationScope?> _cacheFromAttributes = new( SymbolEqualityComparer.Default );

        private SymbolClassifier( Compilation compilation )
        {
            this._compilation = compilation;
            this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ).AssertNotNull();
            this._compileTimeOnlyAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeOnlyAttribute).FullName ).AssertNotNull();
            this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName ).AssertNotNull();
        }

        public static ISymbolClassifier GetInstance( Compilation compilation )
        {
            if ( !_instances.TryGetValue( compilation, out var value ) )
            {
                lock ( _addSync )
                {
                    if ( !_instances.TryGetValue( compilation, out value ) )
                    {
                        var hasCaravelaReference = compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ) != null;
                        value = hasCaravelaReference ? new SymbolClassifier( compilation ) : NoCaravelaReferenceClassifier.Instance;
                        _instances.Add( compilation, value );
                    }
                }
            }

            return value;
        }

        public bool IsTemplate( ISymbol symbol )
        {
            // Look for a [Template] attribute on the symbol.
            if ( symbol.GetAttributes().Any( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._templateAttribute ) ) )
            {
                return true;
            }

            // Look at the overriden method.
            if ( symbol is IMethodSymbol { OverriddenMethod: { } overriddenMethod } )
            {
                return this.IsTemplate( overriddenMethod! );
            }

            // Look at the overridden property.
            if ( symbol is IPropertySymbol { OverriddenProperty: { } overriddenProperty } )
            {
                return this.IsTemplate( overriddenProperty! );
            }

            return false;
        }

        protected virtual SymbolDeclarationScope? GetAttributeScope( AttributeData attribute )
        {
            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeOnlyAttribute ) )
            {
                return SymbolDeclarationScope.CompileTimeOnly;
            }

            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeAttribute ) )
            {
                return SymbolDeclarationScope.Default;
            }

            return null;
        }

        protected virtual SymbolDeclarationScope? GetAssemblyScope( IAssemblySymbol? assembly )
        {
            if ( assembly == null )
            {
                return null;
            }

            // TODO: be more strict with .NET Standard.
            if ( IsStandardLibrary( assembly ) )
            {
                return SymbolDeclarationScope.Default;
            }

            var scopeFromAttributes = assembly.GetAttributes()
                .Concat( assembly.Modules.First().GetAttributes() )
                .Select( this.GetAttributeScope )
                .FirstOrDefault( s => s != null );

            if ( scopeFromAttributes != null )
            {
                return scopeFromAttributes.Value;
            }

            return null;
        }

        private static bool IsStandardLibrary( IAssemblySymbol assembly )
            => assembly.Name.StartsWith( "System", StringComparison.OrdinalIgnoreCase )
               || assembly.Name.Equals( "netstandard", StringComparison.OrdinalIgnoreCase );

        public SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol )
        {
            var scopeFromAssembly = this.GetAssemblyScope( symbol.ContainingAssembly );

            if ( scopeFromAssembly != null )
            {
                return scopeFromAssembly.Value;
            }

            return this.GetScopeFromAttributes( symbol ) ?? SymbolDeclarationScope.RunTimeOnly;
        }

        private SymbolDeclarationScope? GetScopeFromAttributes( ISymbol symbol )
        {
            SymbolDeclarationScope? AddToCache( SymbolDeclarationScope? scope )
            {
                this._cacheFromAttributes[symbol] = scope;

                return scope;
            }

            // From cache.
            if ( this._cacheFromAttributes.TryGetValue( symbol, out var scopeFromCache ) )
            {
                return scopeFromCache;
            }

            // Add the symbol being processed to the cache temporarily to avoid an infinite recursion.
            _ = AddToCache( SymbolDeclarationScope.Default );

            // From attributes.
            var scopeFromAttributes = symbol
                .GetAttributes()
                .Select( this.GetAttributeScope )
                .FirstOrDefault( s => s != null );

            if ( scopeFromAttributes != null )
            {
                return AddToCache( scopeFromAttributes.Value );
            }

            // From overridden method.
            if ( symbol is IMethodSymbol { OverriddenMethod: { } overriddenMethod } )
            {
                var scopeFromOverriddenMethod = this.GetScopeFromAttributes( overriddenMethod! );

                if ( scopeFromOverriddenMethod != null )
                {
                    return AddToCache( scopeFromOverriddenMethod );
                }
            }

            // From declaring type.
            if ( symbol.ContainingType != null )
            {
                var scopeFromContainingType = this.GetScopeFromAttributes( symbol.ContainingType );

                if ( scopeFromContainingType != null )
                {
                    return AddToCache( scopeFromContainingType );
                }
            }

            switch ( symbol )
            {
                case ITypeSymbol type when type.Name == "dynamic":
                    return AddToCache( SymbolDeclarationScope.RunTimeOnly );

                case ITypeSymbol type:
                    {
                        if ( symbol is INamedTypeSymbol namedType )
                        {
                            // Note: Type with [CompileTime] on a base type or an interface should be considered compile-time,
                            // even if it has a generic argument from an external assembly (which makes it run-time). So generic arguments should come last.

                            // From base type.
                            if ( type.BaseType != null )
                            {
                                var scopeFromBaseType = this.GetScopeFromAttributes( type.BaseType );

                                if ( scopeFromBaseType != null )
                                {
                                    return AddToCache( scopeFromBaseType );
                                }
                            }

                            // From interfaces.
                            foreach ( var @interface in type.AllInterfaces )
                            {
                                var scopeFromInterface = this.GetScopeFromAttributes( @interface );

                                if ( scopeFromInterface != null )
                                {
                                    return AddToCache( scopeFromInterface );
                                }
                            }

                            // From generic arguments.
                            foreach ( var genericArgument in namedType.TypeArguments )
                            {
                                var scopeFromGenericArgument = this.GetScopeFromAttributes( genericArgument );

                                if ( scopeFromGenericArgument != null )
                                {
                                    return AddToCache( scopeFromGenericArgument );
                                }
                            }
                        }

                        break;
                    }

                case INamespaceSymbol:
                    // Namespace can be either run-time, build-time or both. We don't do more now but we may have to do it based on assemblies defining the namespace.
                    return AddToCache( SymbolDeclarationScope.Default );
            }

            return AddToCache( null );
        }

        private class NoCaravelaReferenceClassifier : ISymbolClassifier
        {
            public static readonly NoCaravelaReferenceClassifier Instance = new();

            public bool IsTemplate( ISymbol symbol ) => false;

            public SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol )
                => IsStandardLibrary( symbol.ContainingAssembly ) ? SymbolDeclarationScope.Default : SymbolDeclarationScope.RunTimeOnly;
        }
    }
}