// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CompileTime
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
        private static readonly ImmutableDictionary<string, (string Namespace, TemplatingScope Scope, bool MembersOnly)> _wellKnownTypes =
            new (Type ReflectionType, TemplatingScope Scope, bool MembersOnly)[]
            {
                (typeof(Console), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GC), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GCCollectionMode), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GCNotificationStatus), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeArgumentHandle), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeFieldHandle), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeMethodHandle), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeTypeHandle), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(STAThreadAttribute), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(AppDomain), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(MemberInfo), Scope: TemplatingScope.RunTimeOnly, true),
                (typeof(ParameterInfo), Scope: TemplatingScope.RunTimeOnly, true)
            }.ToImmutableDictionary( t => t.ReflectionType.Name, t => (t.ReflectionType.Namespace, t.Scope, t.MembersOnly) );

        private static readonly ImmutableDictionary<string, (TemplatingScope Scope, bool IncludeDescendants)> _wellKnownNamespaces =
            new (string Namespace, TemplatingScope Scope, bool IncludeDescendants)[]
            {
                ("System", TemplatingScope.Both, false),
                ("System.Reflection", TemplatingScope.Both, true),
                ("System.Text", TemplatingScope.Both, true),
                ("System.Collections", TemplatingScope.Both, true),
                ("System.Linq", TemplatingScope.Both, true),
                ("Microsoft.CodeAnalysis", TemplatingScope.RunTimeOnly, true)
            }.ToImmutableDictionary( t => t.Namespace, t => (t.Scope, t.IncludeDescendants), StringComparer.Ordinal );

        private readonly Compilation? _compilation;
        private readonly INamedTypeSymbol? _compileTimeAttribute;
        private readonly INamedTypeSymbol? _compileTimeOnlyAttribute;
        private readonly INamedTypeSymbol? _templateAttribute;
        private readonly INamedTypeSymbol? _ignoreUnlessOverriddenAttribute;
        private readonly ConcurrentDictionary<ISymbol, TemplatingScope?> _cacheScopeFromAttributes = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplatingScope> _cacheResultingScope = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplateInfo> _cacheInheritedTemplateInfo = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplateInfo> _cacheNonInheritedTemplateInfo = new( SymbolEqualityComparer.Default );
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolClassifier"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="compilation">The compilation, or null if the compilation has no reference to Metalama.</param>
        public SymbolClassifier( IServiceProvider serviceProvider, Compilation? compilation )
        {
            if ( compilation != null )
            {
                this._compilation = compilation;
                this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeAttribute).FullName ).AssertNotNull();
                this._compileTimeOnlyAttribute = this._compilation.GetTypeByMetadataName( typeof(CompileTimeOnlyAttribute).FullName ).AssertNotNull();
                this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName ).AssertNotNull();
                this._ignoreUnlessOverriddenAttribute = this._compilation.GetTypeByMetadataName( typeof(AbstractAttribute).FullName ).AssertNotNull();
            }

            this._referenceAssemblyLocator = serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();
        }

        public TemplateInfo GetTemplateInfo( ISymbol symbol ) => this.GetTemplateInfo( symbol, false );

        private TemplateInfo GetTemplateInfo( ISymbol symbol, bool isInherited )
            => isInherited
                ? this._cacheInheritedTemplateInfo.GetOrAdd( symbol, s => this.GetTemplateInfoCore( s, true ) )
                : this._cacheNonInheritedTemplateInfo.GetOrAdd( symbol, s => this.GetTemplateInfoCore( s, false ) );

        private TemplateInfo GetTemplateInfoCore( ISymbol symbol, bool isInherited )
        {
            if ( this._templateAttribute == null )
            {
                return TemplateInfo.None;
            }

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
                                          || symbol.GetAttributes().Any( a => this.IsAttributeOfType( a, this._ignoreUnlessOverriddenAttribute! ) )) )
                    {
                        return templateInfo.AsAbstract();
                    }
                    else
                    {
                        return templateInfo;
                    }
                }
            }

            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: { } associatedSymbol }:
                    return this.GetTemplateInfo( associatedSymbol );

                case IMethodSymbol { OverriddenMethod: { } overriddenMethod }:
                    // Look at the overriden method.
                    return this.GetTemplateInfo( overriddenMethod, true );

                case IPropertySymbol { OverriddenProperty: { } overriddenProperty }:
                    // Look at the overridden property.
                    return this.GetTemplateInfo( overriddenProperty, true );

                default:
                    return TemplateInfo.None;
            }
        }

        private bool IsAttributeOfType( AttributeData a, ITypeSymbol type ) => this._compilation!.HasImplicitConversion( a.AttributeClass, type );

        private static TemplateInfo GetTemplateInfo( AttributeData templateAttribute )
        {
            switch ( templateAttribute.AttributeClass?.Name )
            {
                case nameof(IntroduceAttribute):
                    return new TemplateInfo( TemplateAttributeType.Introduction, templateAttribute );

                case nameof(InterfaceMemberAttribute):
                    return new TemplateInfo( TemplateAttributeType.InterfaceMember, templateAttribute );

                default:
                    return new TemplateInfo( TemplateAttributeType.Template, templateAttribute );
            }
        }

        private TemplatingScope? GetTemplatingScope( AttributeData attribute )
        {
            if ( this._compilation == null )
            {
                return null;
            }

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
            if ( !this._cacheResultingScope.TryGetValue( symbol, out var scope ) )
            {
                scope = this.GetTemplatingScopeCore( symbol );
                this._cacheResultingScope.TryAdd( symbol, scope );
            }

            return scope;
        }

        private TemplatingScope GetTemplatingScopeCore( ISymbol symbol )
        {
            var scope = this.GetTemplatingScopeCore( symbol, 0 );

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

        private TemplatingScope GetTemplatingScopeCore( ISymbol symbol, int recursion )
        {
            if ( symbol is ITypeParameterSymbol )
            {
                // All generic parameters are now run-time only.
                return TemplatingScope.RunTimeOnly;
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
                    throw new AssertionFailedException( "Generic templates or aspects are not supported." );

                case IErrorTypeSymbol:
                    // We treat all error symbols as run-time only, by convention.
                    return TemplatingScope.RunTimeOnly;

                case IArrayTypeSymbol array:
                    {
                        var arrayScope = this.GetTemplatingScopeCore( array.ElementType, recursion + 1 );

                        if ( arrayScope == TemplatingScope.Dynamic )
                        {
                            return TemplatingScope.Invalid;
                        }
                        else
                        {
                            return arrayScope;
                        }
                    }

                case IPointerTypeSymbol pointer:
                    return this.GetTemplatingScopeCore( pointer.PointedAtType, recursion + 1 );

                case INamedTypeSymbol { IsGenericType: true } namedType
                    when !namedType.IsGenericTypeDefinition() &&
                         !SymbolEqualityComparer.Default.Equals( namedType, namedType.OriginalDefinition ):
                    {
                        List<TemplatingScope> scopes = new( namedType.TypeArguments.Length + 1 );
                        var declarationScope = this.GetTemplatingScopeCore( namedType.OriginalDefinition, recursion + 1 );
                        scopes.Add( declarationScope );
                        scopes.AddRange( namedType.TypeArguments.Select( arg => this.GetTemplatingScopeCore( arg, recursion + 1 ) ) );

                        var compileTimeOnlyCount = 0;
                        var runtimeCount = 0;

                        foreach ( var scope in scopes )
                        {
                            switch ( scope )
                            {
                                case TemplatingScope.Dynamic:
                                    // Only a few well-known types can have dynamic generic arguments, other are unsupported.
                                    switch ( namedType.Name )
                                    {
                                        case nameof(Task<object>):
                                        case nameof(ValueTask<object>):
                                        case nameof(IEnumerable<object>):
                                        case nameof(IEnumerator<object>):
                                        case nameof(IAsyncEnumerable<object>):
                                        case nameof(IAsyncEnumerator<object>):
                                            return TemplatingScope.Dynamic;

                                        default:
                                            return TemplatingScope.Invalid;
                                    }

                                case TemplatingScope.RunTimeOnly:
                                    runtimeCount++;

                                    break;

                                case TemplatingScope.CompileTimeOnly:
                                    compileTimeOnlyCount++;

                                    break;

                                case TemplatingScope.Both:
                                    break;

                                default:
                                    throw new AssertionFailedException( $"Unexpected scope: {scope}." );
                            }
                        }

                        switch ( runtimeCount )
                        {
                            case > 0 when compileTimeOnlyCount > 0:
                                return TemplatingScope.Conflict;

                            case > 0:
                                return TemplatingScope.RunTimeOnly;

                            default:
                                {
                                    if ( compileTimeOnlyCount > 0 )
                                    {
                                        return TemplatingScope.CompileTimeOnly;
                                    }
                                    else
                                    {
                                        return TemplatingScope.Both;
                                    }
                                }
                        }
                    }
            }

            // From well-known types.
            if ( this.TryGetWellKnownScope( symbol, false, out var scopeFromWellKnown ) )
            {
                return scopeFromWellKnown;
            }

            // From assembly.
            var scopeFromAssembly = this.GetAssemblyScope( symbol.ContainingAssembly );

            if ( scopeFromAssembly != null )
            {
                return scopeFromAssembly.Value;
            }

            // From attributes.
            var scopeFromAttributes = this.GetScopeFromAttributes( symbol ) ?? TemplatingScope.RunTimeOnly;

            if ( scopeFromAttributes != TemplatingScope.Both )
            {
                return scopeFromAttributes;
            }

            // From signature.
            return this.GetScopeFromSignature( symbol, recursion + 1 );
        }

        private TemplatingScope GetScopeFromSignature( ISymbol symbol, int recursion )
        {
            var signatureScope = TemplatingScope.Both;

            void CombineScope( ITypeSymbol type )
            {
                var typeScope = this.GetTemplatingScopeCore( type, recursion + 1 );

                if ( typeScope != TemplatingScope.Both )
                {
                    signatureScope = signatureScope == TemplatingScope.Both ? typeScope : TemplatingScope.Conflict;
                }
            }

            switch ( symbol )
            {
                case IMethodSymbol method:
                    CombineScope( method.ReturnType );

                    foreach ( var parameter in method.Parameters )
                    {
                        CombineScope( parameter.Type );
                    }

                    return signatureScope;

                case IPropertySymbol property:
                    CombineScope( property.Type );

                    foreach ( var parameter in property.Parameters )
                    {
                        CombineScope( parameter.Type );
                    }

                    return signatureScope;

                case IFieldSymbol field:
                    return this.GetTemplatingScopeCore( field.Type, recursion + 1 );

                case IEventSymbol @event:
                    return this.GetTemplatingScopeCore( @event.Type, recursion + 1 );

                default:
                    return TemplatingScope.Both;
            }
        }

        private TemplatingScope? GetScopeFromAttributes( ISymbol symbol )
        {
            TemplatingScope? AddToCache( TemplatingScope? scope )
            {
                this._cacheScopeFromAttributes[symbol] = scope;

                return scope;
            }

            // From cache.
            if ( this._cacheScopeFromAttributes.TryGetValue( symbol, out var scopeFromCache ) )
            {
                return scopeFromCache;
            }

            // Add the symbol being processed to the cache temporarily to avoid an infinite recursion.
            _ = AddToCache( null );

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
                var scopeFromOverriddenMethod = this.GetScopeFromAttributes( overriddenMethod );

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

        private bool TryGetWellKnownScope( ISymbol symbol, bool isMember, out TemplatingScope scope )
        {
            scope = TemplatingScope.Unknown;

            switch ( symbol )
            {
                case IErrorTypeSymbol:
                    // Coverage: ignore
                    return false;

                case INamedTypeSymbol namedType:
                    // Check well-known types and ancestors.
                    for ( var t = namedType; t != null && t.SpecialType != SpecialType.System_Object; t = t.BaseType )
                    {
                        if ( t.MetadataName is { } name &&
                             _wellKnownTypes.TryGetValue( name, out var config ) &&
                             config.Namespace == namedType.ContainingNamespace.ToDisplayString() &&
                             (!config.MembersOnly || isMember) )
                        {
                            scope = config.Scope;

                            return true;
                        }
                    }

                    // Check well-known namespaces.
                    if ( this._referenceAssemblyLocator.IsSystemAssemblyName( namedType.ContainingAssembly.Name ) )
                    {
                        // Some namespaces inside system assemblies have a well-known scope.
                        for ( var ns = namedType.ContainingNamespace; ns != null; ns = ns.ContainingNamespace )
                        {
                            var nsString = ns.ToDisplayString();

                            if ( _wellKnownNamespaces.TryGetValue( nsString, out var wellKnownNamespace ) )
                            {
                                if ( wellKnownNamespace.IncludeDescendants || ns.Equals( namedType.ContainingNamespace ) )
                                {
                                    scope = wellKnownNamespace.Scope;

                                    return true;
                                }
                            }
                        }

                        // The default scope in system assemblies is run-time-only.
                        scope = TemplatingScope.RunTimeOnly;

                        return true;
                    }

                    return false;

                case { ContainingType: { } namedType }:
                    return this.TryGetWellKnownScope( namedType, true, out scope );

                default:
                    return false;
            }
        }
    }
}