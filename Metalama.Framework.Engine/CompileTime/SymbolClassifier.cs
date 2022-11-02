// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
                // We don't want users to interact with a few classes so we mark then RunTimeOnly
                (typeof(Console), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GC), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GCCollectionMode), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(GCNotificationStatus), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(STAThreadAttribute), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(AppDomain), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Process), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Thread), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(ExecutionContext), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(SynchronizationContext), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Environment), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeEnvironment), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(RuntimeInformation), Scope: TemplatingScope.RunTimeOnly, false),
                (typeof(Marshal), Scope: TemplatingScope.RunTimeOnly, false)
            }.ToImmutableDictionary(
                t => t.ReflectionType.Name.AssertNotNull(),
                t => (t.ReflectionType.Namespace.AssertNotNull(), t.Scope, t.MembersOnly) );

        private readonly Compilation? _compilation;
        private readonly INamedTypeSymbol? _templateAttribute;
        private readonly INamedTypeSymbol? _declarativeAdviceAttribute;
        private readonly ConcurrentDictionary<ISymbol, TemplatingScope?> _cacheScopeFromAttributes = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplatingScope> _cacheDefaultScope = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplatingScope> _cacheOtherScope = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplateInfo> _cacheInheritedTemplateInfo = new( SymbolEqualityComparer.Default );
        private readonly ConcurrentDictionary<ISymbol, TemplateInfo> _cacheNonInheritedTemplateInfo = new( SymbolEqualityComparer.Default );
        private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;
        private readonly AttributeDeserializer _attributeDeserializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolClassifier"/> class.
        /// </summary>
        /// <param name="referenceAssemblyLocator"></param>
        /// <param name="compilation">The compilation, or null if the compilation has no reference to Metalama.</param>
        public SymbolClassifier( IServiceProvider serviceProvider, Compilation? compilation, AttributeDeserializer attributeDeserializer )
        {
            this._referenceAssemblyLocator = serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();
            this._attributeDeserializer = attributeDeserializer;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "SymbolClassifier" );

            if ( compilation != null )
            {
                this._compilation = compilation;
                this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName.AssertNotNull() ).AssertNotNull();

                this._declarativeAdviceAttribute = this._compilation.GetTypeByMetadataName( typeof(DeclarativeAdviceAttribute).FullName.AssertNotNull() )
                    .AssertNotNull();
            }
        }

        public TemplateInfo GetTemplateInfo( ISymbol symbol ) => this.GetTemplateInfo( symbol, false );

        private TemplateInfo GetTemplateInfo( ISymbol symbol, bool isInherited )
            => isInherited
                ? this._cacheInheritedTemplateInfo.GetOrAdd( symbol, s => this.GetTemplateInfoCore( s, true ) )
                : this._cacheNonInheritedTemplateInfo.GetOrAdd( symbol, s => this.GetTemplateInfoCore( s, false ) );

        private TemplateInfo GetTemplateInfoCore( ISymbol symbol, bool isInherited )
        {
            if ( this._templateAttribute == null || this._declarativeAdviceAttribute == null )
            {
                return TemplateInfo.None;
            }

            // Look for a [Template] attribute on the symbol.
            var templateAttribute = symbol
                .GetAttributes()
                .FirstOrDefault( a => this.IsAttributeOfType( a, this._templateAttribute ) || this.IsAttributeOfType( a, this._declarativeAdviceAttribute ) );

            if ( templateAttribute != null )
            {
                var templateInfo = this.GetTemplateInfo( symbol, templateAttribute );

                if ( !templateInfo.IsNone )
                {
                    // Ignore any abstract member.
                    if ( !isInherited && (symbol.IsAbstract
                                          || templateAttribute.NamedArguments.Any( a => a.Key == nameof(TemplateAttribute.IsEmpty) && (bool) a.Value.Value )) )
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

        private TemplateInfo GetTemplateInfo( ISymbol declaringSymbol, AttributeData attributeData )
        {
            if ( !this._attributeDeserializer.TryCreateAttribute( attributeData, NullDiagnosticAdder.Instance, out var attributeInstance ) )
            {
                // This happens when the attribute class is defined in user code.
                // In this case, we have to instantiate the attribute later, after we have the compile-time assembly for the user code.

                // It also happens in case of mismatch between the current Metalama version and the Metalama version to which the project is
                // linked, which should not happen in theory.

                this._logger.Warning?.Log( $"Could not instantiate an attribute of type '{attributeData.AttributeClass}'." );
            }

            var memberId = SymbolId.Create( declaringSymbol );

            switch ( attributeData.AttributeClass?.Name )
            {
                case nameof(TemplateAttribute):
                case "TestTemplateAttribute":
                    return new TemplateInfo( memberId, TemplateAttributeType.Template, (IAdviceAttribute?) attributeInstance );

                case nameof(InterfaceMemberAttribute):
                    return new TemplateInfo( memberId, TemplateAttributeType.InterfaceMember, (IAdviceAttribute?) attributeInstance );

                default:
                    return new TemplateInfo( memberId, TemplateAttributeType.DeclarativeAdvice, (IAdviceAttribute?) attributeInstance );
            }
        }

        private static TemplatingScope? GetTemplatingScope( AttributeData attribute )
            => attribute.AttributeClass?.Name switch
            {
                nameof(CompileTimeAttribute) => TemplatingScope.CompileTimeOnly,
                nameof(CompileTimeReturningRunTimeAttribute) => TemplatingScope.CompileTimeOnlyReturningRuntimeOnly,
                nameof(TemplateAttribute) => TemplatingScope.CompileTimeOnly,
                nameof(RunTimeOrCompileTimeAttribute) => TemplatingScope.RunTimeOrCompileTime,
                nameof(IntroduceAttribute) => TemplatingScope.RunTimeOnly,
                nameof(InterfaceMemberAttribute) => TemplatingScope.RunTimeOnly,
                _ => null
            };

        private static TemplatingScope? GetAssemblyScope( IAssemblySymbol? assembly )
        {
            if ( assembly == null )
            {
                return null;
            }

            var scopeFromAttributes = Enumerable.Concat( assembly.GetAttributes(), assembly.Modules.First().GetAttributes() )
                .Select( GetTemplatingScope )
                .FirstOrDefault( s => s != null );

            if ( scopeFromAttributes != null )
            {
                return scopeFromAttributes.Value;
            }

            return null;
        }

        public TemplatingScope GetTemplatingScope( ISymbol symbol )
        {
            return this.GetTemplatingScopeCore( symbol, GetTemplatingScopeOptions.Default, ImmutableLinkedList<ISymbol>.Empty );
        }

        private TemplatingScope GetTemplatingScopeCore( ISymbol symbol, GetTemplatingScopeOptions options, ImmutableLinkedList<ISymbol> symbolsBeingProcessed )
        {
            CheckRecursion( symbolsBeingProcessed );

            if ( symbol.Kind == SymbolKind.Namespace )
            {
                return TemplatingScope.RunTimeOrCompileTime;
            }

            // Cache lookup.
            var cache = options == GetTemplatingScopeOptions.Default ? this._cacheDefaultScope : this._cacheOtherScope;

            if ( cache.TryGetValue( symbol, out var scope ) )
            {
                return scope;
            }

            scope = GetRawScope();

            // Fix compile-time-only symbols according to their expression type.
            if ( scope == TemplatingScope.CompileTimeOnly )
            {
                var expressionType = symbol.GetExpressionType();

                if ( expressionType != null )
                {
                    switch ( this.GetTemplatingScope( expressionType ) )
                    {
                        case TemplatingScope.RunTimeOnly:
                            return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;

                        case TemplatingScope.RunTimeOrCompileTime:
                            return TemplatingScope.CompileTimeOnlyReturningBoth;
                    }
                }
                else if ( symbol is ITypeParameterSymbol { DeclaringMethod: { } declaringMethod } && !this.GetTemplateInfo( declaringMethod ).IsNone )
                {
                    // Compile-time template parameters always represent run-time types.
                    return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;
                }
            }

            // Add to cache.

            cache.TryAdd( symbol, scope );

            return scope;

            TemplatingScope GetRawScope()
            {
                switch ( symbol )
                {
                    case IDynamicTypeSymbol:
                        return TemplatingScope.Dynamic;

                    case ITypeParameterSymbol typeParameterSymbol:
                        var scopeFromAttribute = this.GetScopeFromAttributes( typeParameterSymbol, symbolsBeingProcessed );

                        if ( scopeFromAttribute != null )
                        {
                            return scopeFromAttribute.Value;
                        }
                        else if ( typeParameterSymbol.ContainingSymbol.Kind == SymbolKind.Method
                                  && !this.GetTemplateInfo( typeParameterSymbol.ContainingSymbol ).IsNone )
                        {
                            // Template parameters are run-time by default.
                            return TemplatingScope.RunTimeOnly;
                        }
                        else if ( (options & GetTemplatingScopeOptions.TypeParametersAreNeutral) != 0 )
                        {
                            // Do not try to go to the containing symbol if we are called from the method level because this would
                            // create an infinite recursion.

                            return TemplatingScope.RunTimeOrCompileTime;
                        }
                        else
                        {
                            var declaringScope = this.GetTemplatingScopeCore( typeParameterSymbol.ContainingSymbol, options, symbolsBeingProcessed );

                            return declaringScope;
                        }

                    case IErrorTypeSymbol:
                        // We treat all error symbols as run-time only, by convention.
                        return TemplatingScope.RunTimeOnly;

                    case IArrayTypeSymbol array:
                        {
                            var elementScope = this.GetTemplatingScopeCore( array.ElementType, options, symbolsBeingProcessed );

                            if ( elementScope is TemplatingScope.Dynamic )
                            {
                                return TemplatingScope.Invalid;
                            }
                            else
                            {
                                return elementScope.GetExpressionValueScope();
                            }
                        }

                    case IPointerTypeSymbol pointer:
                        return this.GetTemplatingScopeCore( pointer.PointedAtType, options, symbolsBeingProcessed );

                    case INamedTypeSymbol { IsGenericType: true } namedType when !namedType.IsGenericTypeDefinition():
                        {
                            List<TemplatingScope> scopes = new( namedType.TypeArguments.Length + 1 );
                            var declarationScope = this.GetTemplatingScopeCore( namedType.OriginalDefinition, options, symbolsBeingProcessed );
                            scopes.Add( declarationScope );
                            scopes.AddRange( namedType.TypeArguments.Select( arg => this.GetTemplatingScopeCore( arg, options, symbolsBeingProcessed ) ) );

                            var compileTimeOnlyCount = 0;
                            var runtimeCount = 0;

                            foreach ( var typeArgumentScope in scopes )
                            {
                                switch ( typeArgumentScope )
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
                                    case TemplatingScope.CompileTimeOnlyReturningRuntimeOnly:
                                        runtimeCount++;

                                        break;

                                    case TemplatingScope.CompileTimeOnly:
                                        compileTimeOnlyCount++;

                                        break;

                                    case TemplatingScope.RunTimeOrCompileTime:
                                        break;

                                    default:
                                        throw new AssertionFailedException( $"Unexpected scope: {typeArgumentScope}." );
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
                                            return TemplatingScope.RunTimeOrCompileTime;
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
                var scopeFromAssembly = GetAssemblyScope( symbol.ContainingAssembly );

                if ( scopeFromAssembly != null )
                {
                    return scopeFromAssembly.Value;
                }

                // From attributes.
                var scopeFromAttributes = this.GetScopeFromAttributes( symbol, symbolsBeingProcessed ) ?? TemplatingScope.RunTimeOnly;

                if ( scopeFromAttributes != TemplatingScope.RunTimeOrCompileTime )
                {
                    // With generic declarations, we must validate type arguments.
                    var typeArguments = symbol switch
                    {
                        INamedTypeSymbol namedType => namedType.TypeArguments,
                        IMethodSymbol method => method.TypeArguments,
                        _ => default
                    };

                    if ( !typeArguments.IsDefaultOrEmpty )
                    {
                        foreach ( var typeArgument in typeArguments )
                        {
                            if ( typeArgument.Kind == SymbolKind.TypeParameter )
                            {
                                continue;
                            }

                            var typeArgumentScope = this.GetTemplatingScopeCore( typeArgument, options, symbolsBeingProcessed.Insert( symbol ) )
                                .GetExpressionValueScope();

                            if ( typeArgumentScope != TemplatingScope.RunTimeOrCompileTime && typeArgumentScope != scopeFromAttributes )
                            {
                                return TemplatingScope.Conflict;
                            }
                        }
                    }

                    return scopeFromAttributes;
                }

                // From signature.
                return this.GetScopeFromSignature( symbol, options, symbolsBeingProcessed );
            }
        }

        private static void CheckRecursion( ImmutableLinkedList<ISymbol> symbolsBeingProcessed )
        {
            if ( symbolsBeingProcessed.Count > 32 )
            {
                var symbols = string.Join( ", ", symbolsBeingProcessed.Distinct( SymbolEqualityComparer.Default ).Select( x => $"'{x}'" ) );

                throw new AssertionFailedException( $"Infinite recursion detected involving the following symbols: {symbols}" );
            }
        }

        private void CombineScope(
            ITypeSymbol type,
            GetTemplatingScopeOptions options,
            ImmutableLinkedList<ISymbol> symbolsBeingProcessed,
            ref TemplatingScope combinedScope )
        {
            var typeScope = this.GetTemplatingScopeCore( type, options, symbolsBeingProcessed );

            if ( typeScope != combinedScope )
            {
                combinedScope = (typeScope, combinedScope) switch
                {
                    (TemplatingScope.Conflict, _) => TemplatingScope.Conflict,
                    (TemplatingScope.Invalid, _) => TemplatingScope.Invalid,
                    (TemplatingScope.CompileTimeOnlyReturningRuntimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.RunTimeOnly,
                    (TemplatingScope.CompileTimeOnlyReturningRuntimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOnly,
                    (_, TemplatingScope.RunTimeOrCompileTime) => typeScope,
                    (TemplatingScope.RunTimeOrCompileTime, _) => combinedScope,
                    (TemplatingScope.RunTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.Conflict,
                    (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.Conflict,
                    _ => throw new AssertionFailedException( $"Invalid combination: ({typeScope}, {combinedScope})" )
                };
            }
        }

        private TemplatingScope GetScopeFromSignature( ISymbol symbol, GetTemplatingScopeOptions options, ImmutableLinkedList<ISymbol> symbolsBeingProcessed )
        {
            var signatureScope = TemplatingScope.RunTimeOrCompileTime;
            var signatureMemberOptions = options | GetTemplatingScopeOptions.TypeParametersAreNeutral;

            switch ( symbol )
            {
                case IMethodSymbol method:
                    this.CombineScope( method.ReturnType, signatureMemberOptions, symbolsBeingProcessed, ref signatureScope );

                    foreach ( var parameter in method.Parameters )
                    {
                        this.CombineScope( parameter.Type, signatureMemberOptions, symbolsBeingProcessed, ref signatureScope );
                    }

                    return signatureScope;

                case IPropertySymbol property:
                    this.CombineScope( property.Type, signatureMemberOptions, symbolsBeingProcessed, ref signatureScope );

                    foreach ( var parameter in property.Parameters )
                    {
                        this.CombineScope( parameter.Type, signatureMemberOptions, symbolsBeingProcessed, ref signatureScope );
                    }

                    return signatureScope;

                case IFieldSymbol field:
                    return this.GetTemplatingScopeCore( field.Type, signatureMemberOptions, symbolsBeingProcessed );

                case IEventSymbol @event:
                    return this.GetTemplatingScopeCore( @event.Type, signatureMemberOptions, symbolsBeingProcessed );

                case IParameterSymbol parameter:
                    {
                        var parameterTypeScope = this.GetTemplatingScopeCore( parameter.Type, signatureMemberOptions, symbolsBeingProcessed );

                        // The type can be a compile-time type parameter, but it does not make the parameter compile-time.
                        return parameterTypeScope.GetExpressionValueScope();
                    }

                default:
                    return TemplatingScope.RunTimeOrCompileTime;
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private TemplatingScope? GetScopeFromAttributes( ISymbol symbol, ImmutableLinkedList<ISymbol> symbolsBeingProcessed )
        {
            CheckRecursion( symbolsBeingProcessed );

            // Get from cache.
            if ( this._cacheScopeFromAttributes.TryGetValue( symbol, out var scopeFromCache ) )
            {
                return scopeFromCache;
            }

            // Compute.
            var scope = Compute();

            // Add to cache.
            this._cacheScopeFromAttributes[symbol] = scope;

            return scope;

            TemplatingScope? Compute()
            {
                // Check if we have a cyclic reference. This happens for instance in `class C : IEquatable<C>`. When evaluating IEquatable<C>,
                // C is non determined.
                if ( symbolsBeingProcessed.Contains( symbol, SymbolEqualityComparer.Default ) )
                {
                    return null;
                }

                // From attributes.
                var scopeFromAttributes = symbol
                    .GetAttributes()
                    .Select( GetTemplatingScope )
                    .FirstOrDefault( s => s != null );

                if ( scopeFromAttributes != null )
                {
                    return scopeFromAttributes.Value;
                }

                var symbolsBeingProcessPlusCurrent = symbolsBeingProcessed.Insert( symbol );

                // From overridden method.
                if ( symbol is IMethodSymbol { OverriddenMethod: { } overriddenMethod } )
                {
                    var scopeFromOverriddenMethod = this.GetScopeFromAttributes( overriddenMethod, symbolsBeingProcessPlusCurrent );

                    if ( scopeFromOverriddenMethod != null )
                    {
                        return scopeFromOverriddenMethod;
                    }
                }

                // From declaring type.
                if ( symbol.ContainingType != null )
                {
                    var scopeFromContainingType = this.GetScopeFromAttributes( symbol.ContainingType, symbolsBeingProcessPlusCurrent );

                    if ( scopeFromContainingType != null )
                    {
                        return scopeFromContainingType;
                    }
                }

                switch ( symbol )
                {
                    case INamedTypeSymbol { IsAnonymousType: true } anonymousType:
                        var combinedScope = TemplatingScope.RunTimeOrCompileTime;

                        foreach ( var member in anonymousType.GetMembers() )
                        {
                            if ( member is IPropertySymbol property )
                            {
                                this.CombineScope( property.Type, GetTemplatingScopeOptions.Default, symbolsBeingProcessed, ref combinedScope );
                            }
                        }

                        return combinedScope;

                    case INamedTypeSymbol namedType:
                        {
                            // Note: Type with [CompileTime] on a base type or an interface should be considered compile-time,
                            // even if it has a generic argument from an external assembly (which makes it run-time). So generic arguments should come last.

                            // From base type.
                            if ( namedType.BaseType != null )
                            {
                                var scopeFromBaseType = this.GetScopeFromAttributes( namedType.BaseType, symbolsBeingProcessPlusCurrent );

                                if ( scopeFromBaseType != null )
                                {
                                    return scopeFromBaseType;
                                }
                            }

                            // From implemented interfaces.
                            foreach ( var @interface in namedType.AllInterfaces )
                            {
                                var scopeFromInterface = this.GetScopeFromAttributes( @interface, symbolsBeingProcessPlusCurrent );

                                if ( scopeFromInterface != null )
                                {
                                    return scopeFromInterface;
                                }
                            }

                            // From generic arguments.
                            if ( !namedType.IsGenericTypeDefinition() )
                            {
                                foreach ( var genericArgument in namedType.TypeArguments )
                                {
                                    var scopeFromGenericArgument = this.GetScopeFromAttributes( genericArgument, symbolsBeingProcessPlusCurrent );

                                    if ( scopeFromGenericArgument != null )
                                    {
                                        return scopeFromGenericArgument;
                                    }
                                }
                            }

                            break;
                        }

                    case INamespaceSymbol:
                        // Namespace can be either run-time, build-time or both. We don't do more now but we may have to do it based on assemblies defining the namespace.
                        return TemplatingScope.RunTimeOrCompileTime;
                }

                return null;
            }
        }

        private bool TryGetWellKnownScope( ISymbol symbol, bool isMember, out TemplatingScope scope )
        {
            scope = TemplatingScope.RunTimeOrCompileTime;

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
                             config.Namespace == namedType.ContainingNamespace.GetFullName() &&
                             (!config.MembersOnly || isMember) )
                        {
                            scope = config.Scope;

                            return true;
                        }
                    }

                    // Check system types.
                    if ( this._referenceAssemblyLocator.IsSystemType( namedType ) )
                    {
                        scope = TemplatingScope.RunTimeOrCompileTime;

                        return true;
                    }

                    return false;

                case { ContainingType: { } namedType }:
                    return this.TryGetWellKnownScope( namedType, true, out scope );

                default:
                    return false;
            }
        }

        [Flags]
        private enum GetTemplatingScopeOptions
        {
            Default = 0,
            TypeParametersAreNeutral = 1
        }
    }
}