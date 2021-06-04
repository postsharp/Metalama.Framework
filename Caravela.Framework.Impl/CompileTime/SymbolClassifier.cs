// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// The main implementation of <see cref="ISymbolClassifier"/>.
    /// </summary>
    internal sealed class SymbolClassifier : ISymbolClassifier
    {
        /// <summary>
        /// List of well-known types, for which the scope is overriden (i.e. this list takes precedence over any other rule).
        /// 'MembersOnly' means that the rule applies to the members of the type, but not to the type itself.
        /// </summary>
        private static readonly Dictionary<string, (TemplatingScope Scope, bool MembersOnly)> _wellKnownRunTimeTypes =
            new (Type Type, TemplatingScope Scope, bool MembersOnly)[]
            {
                (typeof(Console), TemplatingScope.RunTimeOnly, false),
                (typeof(Process), TemplatingScope.RunTimeOnly, false),
                (typeof(Thread), TemplatingScope.RunTimeOnly, false),
                (typeof(AppDomain), TemplatingScope.RunTimeOnly, false),
                (typeof(MemberInfo), TemplatingScope.RunTimeOnly, true),
                (typeof(ParameterInfo), TemplatingScope.RunTimeOnly, true)
            }.ToDictionary( t => t.Type.FullName, t => (t.Scope, t.MembersOnly) );

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _compileTimeAttribute;
        private readonly INamedTypeSymbol _compileTimeOnlyAttribute;
        private readonly INamedTypeSymbol _templateAttribute;
        private readonly Dictionary<ISymbol, TemplatingScope?> _cacheFromAttributes = new( SymbolEqualityComparer.Default );
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SymbolClassifier( Compilation compilation, IServiceProvider serviceProvider )
        {
            this._compilation = compilation;
            this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ).AssertNotNull();
            this._compileTimeOnlyAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeOnlyAttribute).FullName ).AssertNotNull();
            this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName ).AssertNotNull();
            this._referenceAssemblyLocator = serviceProvider.GetService<ReferenceAssemblyLocator>();
        }

        public bool IsTemplate( ISymbol symbol )
        {
            // Look for a [Template] attribute on the symbol.
            if ( symbol.GetAttributes().Any( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._templateAttribute ) ) )
            {
                return true;
            }

            switch ( symbol )
            {
                case IMethodSymbol { OverriddenMethod: { } overriddenMethod }:
                    // Look at the overriden method.
                    return this.IsTemplate( overriddenMethod! );

                case IPropertySymbol { OverriddenProperty: { } overriddenProperty }:
                    // Look at the overridden property.
                    return this.IsTemplate( overriddenProperty! );

                default:
                    return false;
            }
        }

        private TemplatingScope? GetAttributeScope( AttributeData attribute )
        {
            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeOnlyAttribute ) )
            {
                return TemplatingScope.CompileTimeOnly;
            }

            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeAttribute ) )
            {
                return TemplatingScope.Both;
            }

            return null;
        }

        private TemplatingScope? GetAssemblyScope( IAssemblySymbol? assembly )
        {
            if ( assembly == null )
            {
                return null;
            }

            if ( assembly.Name == "System.Private.CoreLib" || this._referenceAssemblyLocator.SystemAssemblyNames.Contains( assembly.Name ) )
            {
                // .NET Standard, Roslyn, ...
                return TemplatingScope.Both;
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

        public TemplatingScope GetTemplatingScope( ISymbol symbol )
        {
            // From well-known types.
            if ( TryGetWellKnownScope( symbol, false, out var scopeFromWellKnown ) )
            {
                return scopeFromWellKnown;
            }

            // From assembly.
            var scopeFromAssembly = this.GetAssemblyScope( symbol.ContainingAssembly );

            if ( scopeFromAssembly != null )
            {
                return scopeFromAssembly.Value;
            }

            return this.GetScopeFromAttributes( symbol ) ?? TemplatingScope.RunTimeOnly;
        }

        private TemplatingScope? GetScopeFromAttributes( ISymbol symbol )
        {
            TemplatingScope? AddToCache( TemplatingScope? scope )
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
            _ = AddToCache( TemplatingScope.Both );

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
                    return AddToCache( TemplatingScope.RunTimeOnly );

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
                    return AddToCache( TemplatingScope.Both );
            }

            return AddToCache( null );
        }

        internal static bool TryGetWellKnownScope( ISymbol symbol, bool isMember, out TemplatingScope scope )
        {
            scope = TemplatingScope.Unknown;

            switch ( symbol )
            {
                case IErrorTypeSymbol:
                    return false;

                case INamedTypeSymbol namedType:
                    if ( namedType.GetReflectionName() is { } name &&
                         _wellKnownRunTimeTypes.TryGetValue( name, out var config ) &&
                         (!config.MembersOnly || isMember) )
                    {
                        scope = config.Scope;

                        return true;
                    }
                    else if ( namedType.BaseType != null )
                    {
                        return TryGetWellKnownScope( namedType.BaseType, isMember, out scope );
                    }
                    else
                    {
                        return false;
                    }

                case { ContainingType: { } namedType }:
                    return TryGetWellKnownScope( namedType, true, out scope );

                default:
                    return false;
            }
        }
    }
}