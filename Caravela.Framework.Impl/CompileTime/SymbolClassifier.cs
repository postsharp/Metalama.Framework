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
using System.Threading.Tasks;

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
        private static readonly Dictionary<string, (string Namespace, TemplatingScope Scope, bool MembersOnly)> _wellKnownRunTimeTypes =
            new (Type ReflectionType, TemplatingScope Scope, bool MembersOnly)[]
            {
                (typeof(Console), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Process), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Thread), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(AppDomain), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(MemberInfo), Scope: TemplatingScope.RunTimeOnly, true),
                (typeof(ParameterInfo), Scope: TemplatingScope.RunTimeOnly, true),
                (typeof(Task), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Task<>), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(ValueTask), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(ValueTask<>), Scope: TemplatingScope.RunTimeOnly, false)
            }.ToDictionary( t => t.ReflectionType.Name, t => (t.ReflectionType.Namespace, t.Scope, t.MembersOnly) );

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _compileTimeAttribute;
        private readonly INamedTypeSymbol _compileTimeOnlyAttribute;
        private readonly INamedTypeSymbol _templateAttribute;
        private readonly INamedTypeSymbol _ignoreUnlessOverriddenAttribute;
        private readonly INamedTypeSymbol _interfaceMemberAttribute;
        private readonly Dictionary<ISymbol, TemplatingScope?> _cacheFromAttributes = new( SymbolEqualityComparer.Default );
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        public SymbolClassifier( Compilation compilation, IServiceProvider serviceProvider )
        {
            this._compilation = compilation;
            this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ).AssertNotNull();
            this._compileTimeOnlyAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeOnlyAttribute).FullName ).AssertNotNull();
            this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName ).AssertNotNull();
            this._interfaceMemberAttribute = this._compilation.GetTypeByMetadataName( typeof(InterfaceMemberAttribute).FullName ).AssertNotNull();
            this._ignoreUnlessOverriddenAttribute = this._compilation.GetTypeByMetadataName( typeof(AbstractAttribute).FullName ).AssertNotNull();
            this._referenceAssemblyLocator = serviceProvider.GetService<ReferenceAssemblyLocator>();
        }

        public TemplateInfo GetTemplateInfo( ISymbol symbol ) => this.GetTemplateInfo( symbol, false );

        private TemplateInfo GetTemplateInfo( ISymbol symbol, bool isInherited )
        {
            // Look for a [Template] attribute on the symbol.
            var templateAttribute = symbol
                .GetAttributes()
                .FirstOrDefault( a => this.IsAttributeOfType( a, this._templateAttribute ) );

            if ( templateAttribute != null )
            {
                var templateInfo = GetTemplateInfo( templateAttribute );

                if ( !templateInfo.IsNone )
                {
                    // Ignore any abstract member.
                    if ( !isInherited && (symbol.IsAbstract
                                          || symbol.GetAttributes().Any( a => this.IsAttributeOfType( a, this._ignoreUnlessOverriddenAttribute ) )) )
                    {
                        return templateInfo.AsAbstract();
                    }
                    else
                    {
                        return templateInfo;
                    }
                }
            }

            // Look for a [InterfaceMember] attribute on the symbol.
            if ( symbol.GetAttributes().Any( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._interfaceMemberAttribute ) ) )
            {
                return new TemplateInfo( TemplateAttributeType.InterfaceMember );
            }

            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: {} associatedSymbol }:
                    return this.GetTemplateInfo( associatedSymbol );
                
                case IMethodSymbol { OverriddenMethod: { } overriddenMethod }:
                    // Look at the overriden method.
                    return this.GetTemplateInfo( overriddenMethod!, true );

                case IPropertySymbol { OverriddenProperty: { } overriddenProperty }:
                    // Look at the overridden property.
                    return this.GetTemplateInfo( overriddenProperty!, true );

                default:
                    return default;
            }
        }

        private bool IsAttributeOfType( AttributeData a, ITypeSymbol type ) => this._compilation.HasImplicitConversion( a.AttributeClass, type );

        private static TemplateInfo GetTemplateInfo( AttributeData templateAttribute )
        {
            switch ( templateAttribute.AttributeClass?.Name )
            {
                case nameof(IntroduceAttribute):
                    return new TemplateInfo( TemplateAttributeType.Introduction );

                case nameof(InterfaceMemberAttribute):
                    return new TemplateInfo( TemplateAttributeType.InterfaceMember );

                default:
                    return new TemplateInfo( TemplateAttributeType.Template );
            }
        }

        public TemplatingScope? GetTemplatingScope( AttributeData attribute )
        {
            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeOnlyAttribute ) )
            {
                return TemplatingScope.CompileTimeOnly;
            }
            else if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeAttribute ) )
            {
                return TemplatingScope.Both;
            }
            else
            {
                return null;
            }
        }

        private TemplatingScope? GetAssemblyScope( IAssemblySymbol? assembly )
        {
            if ( assembly == null )
            {
                return null;
            }

            if ( this._referenceAssemblyLocator.IsSystemAssemblyName( assembly.Name ) )
            {
                // .NET Standard, Roslyn, ...
                return TemplatingScope.Both;
            }

            var scopeFromAttributes = assembly.GetAttributes()
                .Concat( assembly.Modules.First().GetAttributes() )
                .Select( this.GetTemplatingScope )
                .FirstOrDefault( s => s != null );

            if ( scopeFromAttributes != null )
            {
                return scopeFromAttributes.Value;
            }

            return null;
        }

        public TemplatingScope GetTemplatingScope( ISymbol symbol )
        {
            var scope = this.GetTemplatingScope( symbol, 0 );

            if ( scope == TemplatingScope.CompileTimeOnly )
            {
                // If the member returns a run-time only type, it is CompileTimeDynamic.
                var returnType = symbol switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    IMethodSymbol method when !method.GetReturnTypeAttributes()
                        .Any( a => this.GetTemplatingScope( a ).GetValueOrDefault() == TemplatingScope.CompileTimeOnly ) => method
                        .ReturnType,
                    _ => null
                };

                if ( returnType != null && returnType.SpecialType != SpecialType.System_Void )
                {
                    switch ( this.GetTemplatingScope( returnType ) )
                    {
                        case TemplatingScope.RunTimeOnly:
                            return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;

                        case TemplatingScope.Both:
                            return TemplatingScope.CompileTimeOnlyReturningBoth;
                    }
                }
            }

            return scope;
        }

        private TemplatingScope GetTemplatingScope( ISymbol symbol, int recursion )
        {
            if ( symbol is ITypeParameterSymbol )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "Type parameters are not supported." );
            }

            if ( recursion > 32 )
            {
                throw new AssertionFailedException();
            }

            switch ( symbol )
            {
                case IDynamicTypeSymbol:
                    return TemplatingScope.Dynamic;

                case ITypeParameterSymbol:
                    return TemplatingScope.Both;

                case IErrorTypeSymbol:
                    return TemplatingScope.Unknown;

                case IArrayTypeSymbol array:
                    return this.GetTemplatingScope( array.ElementType, recursion + 1 );

                case IPointerTypeSymbol pointer:
                    return this.GetTemplatingScope( pointer.PointedAtType, recursion + 1 );

                case INamedTypeSymbol { IsGenericType: true } namedType
                    when !namedType.IsGenericTypeDefinition() &&
                         !SymbolEqualityComparer.Default.Equals( namedType, namedType.OriginalDefinition ):
                    {
                        List<TemplatingScope> scopes = new( namedType.TypeArguments.Length + 1 );
                        var declarationScope = this.GetTemplatingScope( namedType.OriginalDefinition, recursion + 1 );
                        scopes.Add( declarationScope );
                        scopes.AddRange( namedType.TypeArguments.Select( arg => this.GetTemplatingScope( arg, recursion + 1 ) ) );

                        var compileTimeOnlyCount = 0;
                        var runtimeCount = 0;

                        foreach ( var scope in scopes )
                        {
                            switch ( scope )
                            {
                                case TemplatingScope.Dynamic:
                                    return TemplatingScope.Dynamic;

                                case TemplatingScope.RunTimeOnly:
                                    runtimeCount++;

                                    break;

                                case TemplatingScope.CompileTimeOnly:
                                    compileTimeOnlyCount++;

                                    break;

                                case TemplatingScope.Both:
                                    break;

                                case TemplatingScope.Unknown:
                                    return TemplatingScope.Unknown;

                                default:
                                    throw new AssertionFailedException( $"Unexpected scope: {scope}." );
                            }
                        }

                        if ( runtimeCount > 0 && compileTimeOnlyCount > 0 )
                        {
                            return TemplatingScope.Conflict;
                        }
                        else if ( runtimeCount > 0 )
                        {
                            return TemplatingScope.RunTimeOnly;
                        }
                        else if ( compileTimeOnlyCount > 0 )
                        {
                            return TemplatingScope.CompileTimeOnly;
                        }
                        else
                        {
                            return TemplatingScope.Both;
                        }
                    }
            }

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
                .Select( this.GetTemplatingScope )
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
                    if ( namedType.MetadataName is { } name &&
                         _wellKnownRunTimeTypes.TryGetValue( name, out var config ) &&
                         config.Namespace == namedType.ContainingNamespace.ToDisplayString() &&
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