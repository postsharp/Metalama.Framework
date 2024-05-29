﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AdviceImpl.Attributes;
using Metalama.Framework.Engine.AdviceImpl.Contracts;
using Metalama.Framework.Engine.AdviceImpl.Initialization;
using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EligibilityExtensions = Metalama.Framework.Eligibility.EligibilityExtensions;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.Advising;

// ReSharper disable once PossibleInterfaceMemberAmbiguity
internal sealed partial class AdviceFactory<T> : IAdviser<T>, IAdviceFactoryImpl
    where T : IDeclaration
{
    private readonly AdviceFactoryState _state;
    private readonly TemplateClassInstance? _templateClassInstance;
    private readonly string? _layerName;
    private readonly INamedType? _explicitlyImplementedInterfaceType;

    private readonly ObjectReaderFactory _objectReaderFactory;
    private readonly OtherTemplateClassProvider _otherTemplateClassProvider;

    private readonly CompilationModel _compilation;
    private readonly IDeclaration _aspectTarget;
    private readonly INamedType? _aspectTargetType;

    public T Target { get; }

    public AdviceFactory( T target, AdviceFactoryState state, TemplateClassInstance? templateClassInstance, string? layerName, INamedType? explicitlyImplementedInterfaceType )
    {
        this.Target = target;
        this._state = state;
        this._templateClassInstance = templateClassInstance;
        this._layerName = layerName;
        this._explicitlyImplementedInterfaceType = explicitlyImplementedInterfaceType;

        this._objectReaderFactory = state.ServiceProvider.GetRequiredService<ObjectReaderFactory>();
        this._otherTemplateClassProvider = state.ServiceProvider.GetRequiredService<OtherTemplateClassProvider>();

        // The AdviceFactory is now always working on the initial compilation.
        // In the future, AdviceFactory could work on a compilation snapshot, however we have no use case for this feature yet.
        this._compilation = state.InitialCompilation;
        this._aspectTarget = state.AspectInstance.TargetDeclaration.GetTarget( this.MutableCompilation );
        this._aspectTargetType = this._aspectTarget.GetClosestNamedType();
    }

    private IObjectReader GetObjectReader( object? tags ) => this._objectReaderFactory.GetReader( tags );

    private DisposeAction WithNonUserCode() => this._state.ExecutionContext.WithoutDependencyCollection();

    private AdviceFactory<T> WithTemplateClassInstance( TemplateClassInstance templateClassInstance )
        => new( this.Target, this._state, templateClassInstance, this._layerName, this._explicitlyImplementedInterfaceType );

    IAdviceFactoryImpl IAdviceFactoryImpl.WithTemplateClassInstance( TemplateClassInstance templateClassInstance )
        => this.WithTemplateClassInstance( templateClassInstance );

    public IAdviceFactory WithTemplateProvider( TemplateProvider templateProvider )
        => this.WithTemplateClassInstance(
            new TemplateClassInstance(
                templateProvider,
                this._otherTemplateClassProvider.Get( templateProvider ) ) );

    public IAdviceFactory WithTemplateProvider( ITemplateProvider templateProvider )
        => this.WithTemplateProvider( TemplateProvider.FromInstance( templateProvider ) );

    public IAdviceFactoryImpl WithExplicitInterfaceImplementation( INamedType explicitlyImplementedInterfaceType )
        => new AdviceFactory<T>( this.Target, this._state, this._templateClassInstance, this._layerName, explicitlyImplementedInterfaceType );

    private TemplateMemberRef ValidateRequiredTemplateName( string? templateName, TemplateKind templateKind )
        => this.ValidateTemplateName( templateName, templateKind, true )!.Value;

    private TemplateMemberRef? ValidateTemplateName( string? templateName, TemplateKind templateKind, bool required = false, bool ignoreMissing = false )
    {
        if ( this._templateClassInstance == null )
        {
            throw new AssertionFailedException( "The template instance cannot be null." );
        }

        if ( templateName == null )
        {
            if ( required )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(templateName),
                    $"A required template name was not provided for the template kind {templateKind}." );
            }
            else
            {
                return null;
            }
        }

        return TemplateNameValidator.ValidateTemplateName( this._templateClassInstance.TemplateClass, templateName, templateKind, required, ignoreMissing );
    }

    private TemplateMemberRef SelectMethodTemplate( IMethod targetMethod, in MethodTemplateSelector templateSelector )
    {
        var defaultTemplate = this.ValidateRequiredTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default );
        var asyncTemplate = this.ValidateTemplateName( templateSelector.AsyncTemplate, TemplateKind.Async );

        var enumerableTemplate = this.ValidateTemplateName( templateSelector.EnumerableTemplate, TemplateKind.IEnumerable );
        var enumeratorTemplate = this.ValidateTemplateName( templateSelector.EnumeratorTemplate, TemplateKind.IEnumerator );

        var asyncEnumerableTemplate = this.ValidateTemplateName( templateSelector.AsyncEnumerableTemplate, TemplateKind.IAsyncEnumerable, ignoreMissing: true );
        var asyncEnumeratorTemplate = this.ValidateTemplateName( templateSelector.AsyncEnumeratorTemplate, TemplateKind.IAsyncEnumerator, ignoreMissing: true );

        var interpretedKind = TemplateKind.Default;

        var selectedTemplate = defaultTemplate;

        var asyncInfo = targetMethod.GetAsyncInfoImpl();
        var iteratorInfo = targetMethod.GetIteratorInfoImpl();

        // See if we have an async template, which actually does not need to have an async implementation, does not not need to 
        // be applied only on methods with async implementations. However, if the template has an async implementation, the
        // target awaitable type must be compatible with an async implementation, i.e. it must have a method builder.

        if ( asyncInfo.IsAsync == true || (templateSelector.UseAsyncTemplateForAnyAwaitable && (asyncInfo is { IsAwaitable: true, HasMethodBuilder: true } ||
                                                                                                iteratorInfo.EnumerableKind is EnumerableKind.IAsyncEnumerable
                                                                                                    or
                                                                                                    EnumerableKind.IAsyncEnumerator)) )
        {
            interpretedKind = TemplateKind.Async;

            if ( asyncTemplate.HasValue )
            {
                selectedTemplate = asyncTemplate.Value;

                // We don't return because the result can still be overwritten by async iterators.
            }
        }

        var useIteratorTemplate = iteratorInfo.IsIteratorMethod == true
                                  || (templateSelector.UseEnumerableTemplateForAnyEnumerable && iteratorInfo.EnumerableKind != EnumerableKind.None);

        switch ( iteratorInfo.EnumerableKind )
        {
            case EnumerableKind.None:
                break;

            case EnumerableKind.UntypedIEnumerable:
            case EnumerableKind.IEnumerable:
                if ( useIteratorTemplate && enumerableTemplate.HasValue )
                {
                    return enumerableTemplate.Value;
                }
                else
                {
                    interpretedKind = TemplateKind.IEnumerable;
                }

                break;

            case EnumerableKind.UntypedIEnumerator:
            case EnumerableKind.IEnumerator:
                if ( useIteratorTemplate && enumeratorTemplate.HasValue )
                {
                    return enumeratorTemplate.Value;
                }
                else
                {
                    interpretedKind = TemplateKind.IEnumerator;
                }

                break;

            case EnumerableKind.IAsyncEnumerable:
                if ( useIteratorTemplate && asyncEnumerableTemplate.HasValue )
                {
                    return asyncEnumerableTemplate.Value;
                }
                else
                {
                    interpretedKind = TemplateKind.IAsyncEnumerable;
                }

                break;

            case EnumerableKind.IAsyncEnumerator:
                if ( useIteratorTemplate && asyncEnumeratorTemplate.HasValue )
                {
                    return asyncEnumeratorTemplate.Value;
                }
                else
                {
                    interpretedKind = TemplateKind.IAsyncEnumerator;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return selectedTemplate.InterpretedAs( interpretedKind );
    }

    private TemplateMemberRef? SelectGetterTemplate(
        IFieldOrPropertyOrIndexer targetFieldOrProperty,
        in GetterTemplateSelector templateSelector,
        bool required )
    {
        var getter = targetFieldOrProperty.GetMethod ?? throw new InvalidOperationException();

        var defaultTemplate = this.ValidateTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default, required );
        var enumerableTemplate = this.ValidateTemplateName( templateSelector.EnumerableTemplate, TemplateKind.IEnumerable );
        var enumeratorTemplate = this.ValidateTemplateName( templateSelector.EnumeratorTemplate, TemplateKind.IEnumerator );

        var selectedTemplate = defaultTemplate;

        if ( !templateSelector.HasOnlyDefaultTemplate )
        {
            var iteratorInfo = getter.GetIteratorInfoImpl();

            if ( enumerableTemplate.HasValue && iteratorInfo.IsIteratorMethod == true )
            {
                selectedTemplate = enumerableTemplate;
            }

            if ( enumeratorTemplate.HasValue && iteratorInfo.EnumerableKind is EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator )
            {
                return enumeratorTemplate;
            }
        }

        return selectedTemplate;
    }

    IAdviser<TNewDeclaration> IAdviser<T>.WithTarget<TNewDeclaration>( TNewDeclaration target ) => this.WithDeclaration( target );

    public AdviceFactory<TNewTarget> WithDeclaration<TNewTarget>( TNewTarget target )
        where TNewTarget : IDeclaration
    {
        this.ValidateTarget( target );

        return new AdviceFactory<TNewTarget>( target, this._state, this._templateClassInstance, this._layerName, this._explicitlyImplementedInterfaceType );
    }

    public ICompilation MutableCompilation => this._state.CurrentCompilation;

    private void Validate( IDeclaration declaration, AdviceKind adviceKind, params IDeclaration[] otherTargets )
    {
        var rule = EligibilityRuleFactory.GetAdviceEligibilityRule( adviceKind );

        if ( (rule.GetEligibility( declaration ) & EligibleScenarios.Default) == 0 )
        {
            var justification = rule.GetIneligibilityJustification( EligibleScenarios.Default, new DescribedObject<IDeclaration>( declaration ) );

            throw new InvalidOperationException(
                MetalamaStringFormatter.Format(
                    $"Cannot add an {adviceKind} advice to '{declaration}' because {justification}. Check the {nameof(EligibilityExtensions.IsAdviceEligible)}({nameof(AdviceKind)}.{adviceKind}) method." ) );
        }

        this.ValidateExplicitInterfaceImplementation( adviceKind );

        this.ValidateTarget( declaration, otherTargets );
    }

    private void CheckContractEligibility( IDeclaration declaration, ContractDirection contractDirection )
    {
        var rule = EligibilityRuleFactory.GetContractAdviceEligibilityRule( contractDirection );

        if ( (rule.GetEligibility( declaration ) & EligibleScenarios.Default) == 0 )
        {
            var justification = rule.GetIneligibilityJustification( EligibleScenarios.Default, new DescribedObject<IDeclaration>( declaration ) );

            throw new InvalidOperationException(
                MetalamaStringFormatter.Format(
                    $"Cannot add an {AdviceKind.AddContract} advice of direction {contractDirection} to '{declaration}' because {justification}. Check the {nameof(EligibilityExtensions.IsContractAdviceEligible)}({nameof(ContractDirection)}.{contractDirection}) method." ) );
        }

        this.ValidateTarget( declaration );
    }

    private void ValidateTarget( IDeclaration declaration, IDeclaration[]? otherTargets = null )
    {
        ValidateOneTarget( declaration );

        if ( otherTargets != null )
        {
            foreach ( var d in otherTargets )
            {
                ValidateOneTarget( d );
            }
        }

        void ValidateOneTarget( IDeclaration target )
        {
            // Check that the compilation match.
            if ( !ReferenceEquals( target.Compilation, this._compilation ) && !ReferenceEquals( target.Compilation, this._state.CurrentCompilation ) )
            {
                throw new InvalidOperationException( "The target declaration is not in the current compilation." );
            }

            // Check that the advised target is under the aspect target.
            if ( !target.ForCompilation( this.MutableCompilation ).IsContainedIn( this._aspectTargetType ?? this._aspectTarget ) )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format(
                        $"The advised target '{target}' is not contained in the target of the aspect '{this._aspectTargetType ?? this._aspectTarget}'." ) );
            }
        }
    }

    private void ValidateExplicitInterfaceImplementation( AdviceKind adviceKind )
    {
        if ( this._explicitlyImplementedInterfaceType != null
             && adviceKind is not (AdviceKind.IntroduceMethod or AdviceKind.IntroduceEvent or AdviceKind.IntroduceOperator or AdviceKind.IntroduceProperty
                 or AdviceKind.IntroduceIndexer) )
        {
            throw new InvalidOperationException( $"The {adviceKind} advice cannot be applied when explicitly implementing an interface." );
        }
    }

    private Advice.AdviceConstructorParameters<TDeclaration> GetAdviceConstructorParameters<TDeclaration>( TDeclaration target )
        where TDeclaration : IDeclaration
    {
        if ( this._templateClassInstance == null )
        {
            throw new InvalidOperationException( "The template class instance cannot be null." );
        }

        return new( this._state.AspectInstance, this._templateClassInstance, target, this._compilation, this._layerName );
    }

    public IOverrideAdviceResult<IMethod> Override(
        IMethod targetMethod,
        in MethodTemplateSelector templateSelector,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetMethod, AdviceKind.OverrideMethod );

            switch ( targetMethod.MethodKind )
            {
                case MethodKind.EventAdd:
                    {
                        var @event = (IEvent) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateRequiredTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( @event.AddMethod, this.GetObjectReader( args ) );

                        return new OverrideEventAdvice(
                                this.GetAdviceConstructorParameters( @event ),
                                addTemplate: template,
                                removeTemplate: null,
                                this.GetObjectReader( tags ) )
                            .Execute( this._state )
                            .GetAccessor( e => e.AddMethod );
                    }

                case MethodKind.EventRemove:
                    {
                        var @event = (IEvent) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateRequiredTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( @event.AddMethod, this.GetObjectReader( args ) );

                        return new OverrideEventAdvice(
                                this.GetAdviceConstructorParameters( @event ),
                                addTemplate: null,
                                removeTemplate: template,
                                this.GetObjectReader( tags ) )
                            .Execute( this._state )
                            .GetAccessor( e => e.RemoveMethod );
                    }

                case MethodKind.PropertyGet:
                    {
                        var propertyOrIndexer = (IPropertyOrIndexer) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.SelectGetterTemplate( propertyOrIndexer, templateSelector.AsGetterTemplateSelector(), true )
                            ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) );

                        switch ( propertyOrIndexer )
                        {
                            case IProperty property:
                                return new OverrideFieldOrPropertyAdvice(
                                        this.GetAdviceConstructorParameters<IFieldOrProperty>( property ),
                                        getTemplate: template,
                                        setTemplate: null,
                                        this.GetObjectReader( tags ) )
                                    .Execute( this._state )
                                    .GetAccessor( p => p.GetMethod );

                            case IIndexer indexer:
                                return new OverrideIndexerAdvice(
                                        this.GetAdviceConstructorParameters( indexer ),
                                        getTemplate: template,
                                        setTemplate: null,
                                        this.GetObjectReader( tags ) )
                                    .Execute( this._state )
                                    .GetAccessor( p => p.GetMethod );

                            default:
                                throw new AssertionFailedException( $"Unexpected declaration {propertyOrIndexer.DeclarationKind}." );
                        }
                    }

                case MethodKind.PropertySet:
                    {
                        var propertyOrIndexer = (IPropertyOrIndexer) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default, true )
                            ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) );

                        switch ( propertyOrIndexer )
                        {
                            case IProperty property:
                                return new OverrideFieldOrPropertyAdvice(
                                        this.GetAdviceConstructorParameters<IFieldOrProperty>( property ),
                                        getTemplate: null,
                                        setTemplate: template,
                                        this.GetObjectReader( tags ) )
                                    .Execute( this._state )
                                    .GetAccessor( p => p.SetMethod );

                            case IIndexer indexer:
                                return new OverrideIndexerAdvice(
                                        this.GetAdviceConstructorParameters( indexer ),
                                        getTemplate: null,
                                        setTemplate: template,
                                        this.GetObjectReader( tags ) )
                                    .Execute( this._state )
                                    .GetAccessor( p => p.SetMethod );

                            default:
                                throw new AssertionFailedException( $"Unexpected declaration {propertyOrIndexer.DeclarationKind}." );
                        }
                    }

                default:
                    {
                        var template = this.SelectMethodTemplate( targetMethod, templateSelector )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) )
                            .AssertNotNull();

                        return new OverrideMethodAdvice(
                                this.GetAdviceConstructorParameters( targetMethod ),
                                template,
                                this.GetObjectReader( tags ) )
                            .Execute( this._state );
                    }
            }
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceMethod(
        INamedType targetType,
        string defaultTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IMethodBuilder>? buildMethod = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceMethod );

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                !.Value
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceMethodAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                scope,
                whenExists,
                buildMethod,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceFinalizer(
        INamedType targetType,
        string defaultTemplate,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceFinalizer );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceFinalizerAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceUnaryOperator(
        INamedType targetType,
        string defaultTemplate,
        IType inputType,
        IType resultType,
        OperatorKind kind,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IMethodBuilder>? buildAction = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( kind.GetCategory() != OperatorCategory.Unary )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format( $"Cannot add an IntroduceUnaryOperator advice with kind {kind} as it is not an unary operator." ) );
            }

            this.Validate( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceOperatorAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                kind,
                leftOperandType: inputType,
                rightOperandType: null,
                resultType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceBinaryOperator(
        INamedType targetType,
        string defaultTemplate,
        IType leftType,
        IType rightType,
        IType resultType,
        OperatorKind kind,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IMethodBuilder>? buildAction = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( kind.GetCategory() != OperatorCategory.Binary )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format( $"Cannot add an IntroduceBinaryOperator advice with {kind} as it is not a binary operator." ) );
            }

            this.Validate( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceOperatorAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                kind,
                leftType,
                rightType,
                resultType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceConversionOperator(
        INamedType targetType,
        string defaultTemplate,
        IType fromType,
        IType toType,
        bool isImplicit = false,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IMethodBuilder>? buildAction = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var operatorKind = isImplicit ? OperatorKind.ImplicitConversion : OperatorKind.ExplicitConversion;

            var advice = new IntroduceOperatorAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                operatorKind,
                leftOperandType: fromType,
                rightOperandType: null,
                toType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IOverrideAdviceResult<IConstructor> Override(
        IConstructor targetConstructor,
        string template,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetConstructor, AdviceKind.OverrideConstructor );

            var boundTemplate =
                this.ValidateTemplateName( template, TemplateKind.Default, true )
                    ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetConstructor, this.GetObjectReader( args ) );

            var advice = new OverrideConstructorAdvice(
                this.GetAdviceConstructorParameters( targetConstructor ),
                boundTemplate.AssertNotNull(),
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IConstructor> IntroduceConstructor(
        INamedType targetType,
        string defaultTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IConstructorBuilder>? buildAction = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceConstructor );

            var template =
                this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                    .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            return new IntroduceConstructorAdvice(
                    this.GetAdviceConstructorParameters( targetType ),
                    template.PartialForIntroduction( this.GetObjectReader( args ) ),
                    scope,
                    whenExists,
                    buildAction,
                    this.GetObjectReader( tags ) )
                .Execute( this._state );
        }
    }

    public IOverrideAdviceResult<IProperty> Override(
        IFieldOrProperty targetFieldOrProperty,
        string template,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetFieldOrProperty, AdviceKind.OverrideFieldOrPropertyOrIndexer );

            // Set template represents both set and init accessors.
            var propertyTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
                .GetTemplateMember<IProperty>( this._compilation, this._state.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();

            var getTemplate =
                targetFieldOrProperty.GetMethod != null
                    ? accessorTemplates.Get?.ForOverride( targetFieldOrProperty.GetMethod )
                    : null;

            var setTemplate =
                targetFieldOrProperty.SetMethod != null
                    ? accessorTemplates.Set?.ForOverride( targetFieldOrProperty.SetMethod )
                    : null;

            var advice = new OverrideFieldOrPropertyAdvice(
                this.GetAdviceConstructorParameters( targetFieldOrProperty ),
                getTemplate,
                setTemplate,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IOverrideAdviceResult<IPropertyOrIndexer> OverrideAccessors(
        IFieldOrPropertyOrIndexer targetFieldOrPropertyOrIndexer,
        in GetterTemplateSelector getTemplateSelector,
        string? setTemplate = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateClassInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.Validate( targetFieldOrPropertyOrIndexer, AdviceKind.OverrideFieldOrPropertyOrIndexer );

            // Set template represents both set and init accessors.
            var boundGetTemplate = targetFieldOrPropertyOrIndexer.GetMethod != null
                ? this.SelectGetterTemplate( targetFieldOrPropertyOrIndexer, getTemplateSelector, setTemplate == null )
                    ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetFieldOrPropertyOrIndexer.GetMethod, this.GetObjectReader( args ) )
                : null;

            var boundSetTemplate = targetFieldOrPropertyOrIndexer.SetMethod != null
                ? this.ValidateTemplateName( setTemplate, TemplateKind.Default, getTemplateSelector.IsNull )
                    ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetFieldOrPropertyOrIndexer.SetMethod, this.GetObjectReader( args ) )
                : null;

            if ( boundGetTemplate == null && boundSetTemplate == null )
            {
                throw new InvalidOperationException( "There is no accessor to override." );
            }

            switch ( targetFieldOrPropertyOrIndexer )
            {
                case IFieldOrProperty targetFieldOrProperty:
                    {
                        var advice = new OverrideFieldOrPropertyAdvice(
                            this.GetAdviceConstructorParameters( targetFieldOrProperty ),
                            boundGetTemplate,
                            boundSetTemplate,
                            this.GetObjectReader( tags ) );

                        return advice.Execute( this._state );
                    }

                case IIndexer targetIndexer:
                    {
                        var advice = new OverrideIndexerAdvice(
                            this.GetAdviceConstructorParameters( targetIndexer ),
                            boundGetTemplate,
                            boundSetTemplate,
                            this.GetObjectReader( tags ) );

                        return advice.Execute( this._state );
                    }

                default:
                    throw new AssertionFailedException( $"{targetFieldOrPropertyOrIndexer.GetType().Name} is not expected here." );
            }
        }
    }

    public IOverrideAdviceResult<IProperty> OverrideAccessors(
        IFieldOrProperty targetFieldOrProperty,
        in GetterTemplateSelector getTemplate = default,
        string? setTemplate = null,
        object? args = null,
        object? tags = null )
        => (IOverrideAdviceResult<IProperty>) this.OverrideAccessors( (IFieldOrPropertyOrIndexer) targetFieldOrProperty, getTemplate, setTemplate, args, tags );

    public IOverrideAdviceResult<IIndexer> OverrideAccessors(
        IIndexer targetIndexer,
        in GetterTemplateSelector getTemplate = default,
        string? setTemplate = null,
        object? args = null,
        object? tags = null )
        => (IOverrideAdviceResult<IIndexer>) this.OverrideAccessors( (IFieldOrPropertyOrIndexer) targetIndexer, getTemplate, setTemplate, args, tags );

    public IIntroductionAdviceResult<IField> IntroduceField(
        INamedType targetType,
        string templateName,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IFieldBuilder>? buildField = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceField );

            var template = this.ValidateRequiredTemplateName( templateName, TemplateKind.Default )
                .GetTemplateMember<IField>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceFieldAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                null,
                template,
                scope,
                whenExists,
                buildField,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IField> IntroduceField(
        INamedType targetType,
        string fieldName,
        IType fieldType,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IFieldBuilder>? buildField = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceField );

            var advice = new IntroduceFieldAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                fieldName,
                default,
                scope,
                whenExists,
                builder =>
                {
                    builder.Type = fieldType;
                    buildField?.Invoke( builder );
                },
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IField> IntroduceField(
        INamedType targetType,
        string fieldName,
        Type fieldType,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IFieldBuilder>? buildField = null,
        object? tags = null )
        => this.IntroduceField(
            targetType,
            fieldName,
            this._compilation.Factory.GetTypeByReflectionType( fieldType ),
            scope,
            whenExists,
            buildField,
            tags );

    public IIntroductionAdviceResult<IProperty> IntroduceAutomaticProperty(
        INamedType targetType,
        string propertyName,
        IType propertyType,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IPropertyBuilder>? buildProperty = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceProperty );

            var advice = new IntroducePropertyAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                propertyName,
                propertyType,
                default,
                default,
                default,
                scope,
                whenExists,
                buildProperty,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IProperty> IntroduceAutomaticProperty(
        INamedType targetType,
        string propertyName,
        Type propertyType,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IPropertyBuilder>? buildProperty = null,
        object? tags = null )
        => this.IntroduceAutomaticProperty(
            targetType,
            propertyName,
            this._compilation.Factory.GetTypeByReflectionType( propertyType ),
            scope,
            whenExists,
            buildProperty,
            tags );

    public IIntroductionAdviceResult<IProperty> IntroduceProperty(
        INamedType targetType,
        string defaultTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IPropertyBuilder>? buildProperty = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceProperty );

            var propertyTemplate = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IProperty>( this._compilation, this._state.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();

            var advice = new IntroducePropertyAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                explicitName: null,
                explicitType: null,
                propertyTemplate,
                accessorTemplates.Get?.PartialForIntroduction(),
                accessorTemplates.Set?.PartialForIntroduction(),
                scope,
                whenExists,
                buildProperty,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IProperty> IntroduceProperty(
        INamedType targetType,
        string name,
        string? getTemplate,
        string? setTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IPropertyBuilder>? buildProperty = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( getTemplate == null && setTemplate == null )
            {
                throw new ArgumentNullException( nameof(getTemplate), "Either getTemplate or setTemplate must be provided." );
            }

            this.Validate( targetType, AdviceKind.IntroduceProperty );

            var boundGetTemplate = this.ValidateTemplateName( getTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundSetTemplate = this.ValidateTemplateName( setTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroducePropertyAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                name,
                explicitType: null,
                propertyTemplate: null,
                boundGetTemplate?.PartialForIntroduction( parameterReaders ),
                boundSetTemplate?.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildProperty,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IIndexer> IntroduceIndexer(
        INamedType targetType,
        IType indexType,
        string? getTemplate,
        string? setTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IIndexerBuilder>? buildIndexer = null,
        object? args = null,
        object? tags = null )
        => this.IntroduceIndexer(
            targetType,
            new[] { (indexType, "index") },
            getTemplate,
            setTemplate,
            scope,
            whenExists,
            buildIndexer,
            args,
            tags );

    public IIntroductionAdviceResult<IIndexer> IntroduceIndexer(
        INamedType targetType,
        Type indexType,
        string? getTemplate,
        string? setTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IIndexerBuilder>? buildIndexer = null,
        object? args = null,
        object? tags = null )
        => this.IntroduceIndexer(
            targetType,
            new[] { (this._compilation.Factory.GetTypeByReflectionType( indexType ), "index") },
            getTemplate,
            setTemplate,
            scope,
            whenExists,
            buildIndexer,
            args,
            tags );

    public IIntroductionAdviceResult<IIndexer> IntroduceIndexer(
        INamedType targetType,
        IReadOnlyList<(Type Type, string Name)> indices,
        string? getTemplate,
        string? setTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IIndexerBuilder>? buildIndexer = null,
        object? args = null,
        object? tags = null )
        => this.IntroduceIndexer(
            targetType,
            indices.SelectAsImmutableArray( x => (this._compilation.Factory.GetTypeByReflectionType( x.Type ), x.Name) ),
            getTemplate,
            setTemplate,
            scope,
            whenExists,
            buildIndexer,
            args,
            tags );

    public IIntroductionAdviceResult<IIndexer> IntroduceIndexer(
        INamedType targetType,
        IReadOnlyList<(IType Type, string Name)> indices,
        string? getTemplate,
        string? setTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IIndexerBuilder>? buildIndexer = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateClassInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( getTemplate == null && setTemplate == null )
            {
                throw new ArgumentNullException( nameof(getTemplate), "Either getTemplate or setTemplate must be provided." );
            }

            this.Validate( targetType, AdviceKind.IntroduceIndexer );

            var boundGetTemplate = this.ValidateTemplateName( getTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundSetTemplate = this.ValidateTemplateName( setTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroduceIndexerAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                indices,
                boundGetTemplate?.PartialForIntroduction( parameterReaders ),
                boundSetTemplate?.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildIndexer,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IOverrideAdviceResult<IEvent> OverrideAccessors(
        IEvent targetEvent,
        string? addTemplate,
        string? removeTemplate,
        string? invokeTemplate,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( invokeTemplate != null )
            {
                throw GeneralDiagnosticDescriptors.UnsupportedFeature.CreateException( $"Invoker overrides." );
            }

            if ( invokeTemplate != null )
            {
                throw new NotImplementedException( "Support for overriding event raisers is not yet implemented." );
            }

            this.Validate( targetEvent, AdviceKind.OverrideEvent );

            var boundAddTemplate =
                this.ValidateRequiredTemplateName( addTemplate, TemplateKind.Default )
                    .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetEvent.AddMethod, this.GetObjectReader( args ) );

            var boundRemoveTemplate =
                this.ValidateRequiredTemplateName( removeTemplate, TemplateKind.Default )
                    .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetEvent.RemoveMethod, this.GetObjectReader( args ) );

            var advice = new OverrideEventAdvice(
                this.GetAdviceConstructorParameters( targetEvent ),
                boundAddTemplate,
                boundRemoveTemplate,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IEvent> IntroduceEvent(
        INamedType targetType,
        string defaultTemplate,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IEventBuilder>? buildEvent = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceEvent );

            var eventTemplate = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IEvent>( this._compilation, this._state.ServiceProvider );

            var (add, remove) = eventTemplate.GetAccessorTemplates();

            var advice = new IntroduceEventAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                explicitName: null,
                eventTemplate,
                add?.PartialForIntroduction(),
                remove?.PartialForIntroduction(),
                scope,
                whenExists,
                buildEvent,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IEvent> IntroduceEvent(
        INamedType targetType,
        string name,
        string addTemplate,
        string removeTemplate,
        string? invokeTemplate = null,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IEventBuilder>? buildEvent = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.IntroduceEvent );

            var boundAddTemplate = this.ValidateRequiredTemplateName( addTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundRemoveTemplate = this.ValidateRequiredTemplateName( removeTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroduceEventAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                name,
                eventTemplate: null,
                boundAddTemplate.PartialForIntroduction( parameterReaders ),
                boundRemoveTemplate.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildEvent,
                this.GetObjectReader( tags ),
                this._explicitlyImplementedInterfaceType );

            return advice.Execute( this._state );
        }
    }

    public IImplementInterfaceAdviceResult ImplementInterface(
        INamedType targetType,
        INamedType interfaceType,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.ImplementInterface );

            var advice = new ImplementInterfaceAdvice(
                this.GetAdviceConstructorParameters( targetType ),
                interfaceType,
                whenExists,
                this.GetObjectReader( tags ),
                this );

            return advice.Execute( this._state );
        }
    }

    public IImplementInterfaceAdviceResult ImplementInterface(
        INamedType targetType,
        Type interfaceType,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        object? tags = null )
        => this.ImplementInterface(
            targetType,
            (INamedType) targetType.GetCompilationModel().Factory.GetTypeByReflectionType( interfaceType ),
            whenExists,
            tags );

    public IAddInitializerAdviceResult AddInitializer(
        INamedType targetType,
        string template,
        InitializerKind kind,
        object? tags = null,
        object? args = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.AddInitializer );

            var boundTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new TemplateBasedInitializeAdvice(
                this.GetAdviceConstructorParameters<IMemberOrNamedType>( targetType ),
                boundTemplate.ForInitializer( this.GetObjectReader( args ) ),
                kind,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IAddInitializerAdviceResult AddInitializer(
        INamedType targetType,
        IStatement statement,
        InitializerKind kind )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetType, AdviceKind.AddInitializer );

            var advice = new SyntaxBasedInitializeAdvice(
                this.GetAdviceConstructorParameters<IMemberOrNamedType>( targetType ),
                statement,
                kind );

            return advice.Execute( this._state );
        }
    }

    public IAddInitializerAdviceResult AddInitializer( IConstructor targetConstructor, string template, object? tags = null, object? args = null )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetConstructor, AdviceKind.AddInitializer );

            var boundTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new TemplateBasedInitializeAdvice(
                this.GetAdviceConstructorParameters<IMemberOrNamedType>( targetConstructor ),
                boundTemplate.ForInitializer( this.GetObjectReader( args ) ),
                InitializerKind.BeforeInstanceConstructor,
                this.GetObjectReader( tags ) );

            return advice.Execute( this._state );
        }
    }

    public IAddInitializerAdviceResult AddInitializer( IConstructor targetConstructor, IStatement statement )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( targetConstructor, AdviceKind.AddInitializer );

            var advice = new SyntaxBasedInitializeAdvice(
                this.GetAdviceConstructorParameters<IMemberOrNamedType>( targetConstructor ),
                statement,
                InitializerKind.BeforeInstanceConstructor );

            return advice.Execute( this._state );
        }
    }

    public IAddContractAdviceResult<IParameter> AddContract(
        IParameter targetParameter,
        string template,
        ContractDirection kind = ContractDirection.Input,
        object? tags = null,
        object? args = null )
    {
        using ( this.WithNonUserCode() )
        {
            switch ( kind )
            {
                case ContractDirection.Output when targetParameter.RefKind is not (RefKind.Ref or RefKind.Out) && !targetParameter.IsReturnParameter:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        MetalamaStringFormatter.Format(
                            $"Cannot add an output contract to the parameter '{targetParameter}' because it is neither 'ref' nor 'out'." ) );

                case ContractDirection.Input when targetParameter.RefKind is RefKind.Out:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        MetalamaStringFormatter.Format( $"Cannot add an input contract to the out parameter '{targetParameter}' " ) );

                case ContractDirection.Input when targetParameter.IsReturnParameter:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        MetalamaStringFormatter.Format( $"Cannot add an input contract to the return parameter '{targetParameter}' " ) );
            }

            if ( !this.TryPrepareContract( targetParameter, template, ref kind, out var boundTemplate ) )
            {
                return AddContractAdviceResult<IParameter>.Ignored;
            }

            var advice = new ParameterContractAdvice(
                this.GetAdviceConstructorParameters( targetParameter ),
                boundTemplate,
                kind,
                this.GetObjectReader( tags ),
                this.GetObjectReader( args ) );

            return advice.Execute( this._state );
        }
    }

    public IAddContractAdviceResult<IFieldOrPropertyOrIndexer> AddContract(
        IFieldOrPropertyOrIndexer targetMember,
        string template,
        ContractDirection direction = ContractDirection.Default,
        object? tags = null,
        object? args = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( !this.TryPrepareContract( targetMember, template, ref direction, out var boundTemplate ) )
            {
                return AddContractAdviceResult<IFieldOrPropertyOrIndexer>.Ignored;
            }

            var advice = new FieldOrPropertyOrIndexerContractAdvice(
                this.GetAdviceConstructorParameters( targetMember ),
                boundTemplate,
                direction,
                this.GetObjectReader( tags ),
                this.GetObjectReader( args ) );

            return advice.Execute( this._state );
        }
    }

    private bool TryPrepareContract<TContract>(
        TContract targetDeclaration,
        string templateName,
        ref ContractDirection direction,
        [NotNullWhen( true )] out TemplateMember<IMethod>? boundTemplate )
        where TContract : class, IDeclaration
    {
        if ( this._templateClassInstance == null )
        {
            throw new InvalidOperationException();
        }

        if ( direction == ContractDirection.None )
        {
            boundTemplate = null;

            return false;
        }

        this.CheckContractEligibility( targetDeclaration, direction );

        direction = ContractAspectHelper.GetEffectiveDirection( direction, targetDeclaration );

        boundTemplate = this.ValidateRequiredTemplateName( templateName, TemplateKind.Default )
            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

        return true;
    }

    public IIntroductionAdviceResult<IAttribute> IntroduceAttribute(
        IDeclaration targetDeclaration,
        IAttributeData attribute,
        OverrideStrategy whenExists = OverrideStrategy.Default )
    {
        this.ValidateExplicitInterfaceImplementation( AdviceKind.IntroduceAttribute );

        return new AddAttributeAdvice(
            this.GetAdviceConstructorParameters( targetDeclaration ),
            attribute,
            whenExists ).Execute( this._state );
    }

    public IRemoveAttributesAdviceResult RemoveAttributes( IDeclaration targetDeclaration, INamedType attributeType )
    {
        using ( this.WithNonUserCode() )
        {
            this.ValidateExplicitInterfaceImplementation( AdviceKind.RemoveAttributes );

            return new RemoveAttributesAdvice(
                this.GetAdviceConstructorParameters( targetDeclaration ),
                attributeType ).Execute( this._state );
        }
    }

    public IRemoveAttributesAdviceResult RemoveAttributes( IDeclaration targetDeclaration, Type attributeType )
        => this.RemoveAttributes( targetDeclaration, (INamedType) this._compilation.Factory.GetTypeByReflectionType( attributeType ) );

    public IIntroductionAdviceResult<IParameter> IntroduceParameter(
        IConstructor constructor,
        string parameterName,
        IType parameterType,
        TypedConstant defaultValue,
        Func<IParameter, IConstructor, PullAction>? pullAction = null,
        ImmutableArray<AttributeConstruction> attributes = default )
    {
        using ( this.WithNonUserCode() )
        {
            this.Validate( constructor, AdviceKind.IntroduceParameter );

            var advice = new IntroduceConstructorParameterAdvice(
                this.GetAdviceConstructorParameters( constructor ),
                parameterName,
                parameterType,
                attributes.IsDefaultOrEmpty ? null : builder => builder.AddAttributes( attributes ),
                pullAction,
                defaultValue );

            return advice.Execute( this._state );
        }
    }

    public IIntroductionAdviceResult<IParameter> IntroduceParameter(
        IConstructor constructor,
        string parameterName,
        Type parameterType,
        TypedConstant defaultValue,
        Func<IParameter, IConstructor, PullAction>? pullAction = null,
        ImmutableArray<AttributeConstruction> attributes = default )
        => this.IntroduceParameter(
            constructor,
            parameterName,
            this._compilation.Factory.GetTypeByReflectionType( parameterType ),
            defaultValue,
            pullAction,
            attributes );

    public IClassIntroductionAdviceResult IntroduceClass(
        INamespaceOrNamedType targetNamespaceOrType,
        string name,
        TypeKind typeKind,
        Action<INamedTypeBuilder>? buildType = null )
    {
        if ( typeKind is not TypeKind.Class )
        {
            throw new NotImplementedException( "Introducing other kinds of types than classes is not implemented." );
        }

        using ( this.WithNonUserCode() )
        {
            this.ValidateExplicitInterfaceImplementation( AdviceKind.IntroduceType );

            return AsAdviser(
                this,
                new IntroduceNamedTypeAdvice(
                        this.GetAdviceConstructorParameters( targetNamespaceOrType ),
                        name,
                        buildType )
                    .Execute( this._state ) );
        }
    }

    public INamespaceIntroductionAdviceResult IntroduceNamespace(
        INamespace targetNamespace,
        string name )
    {
        // TODO: Dependency on template class instance should not be required.
        if ( this._templateClassInstance == null )
        {
            throw new InvalidOperationException();
        }

        using ( this.WithNonUserCode() )
        {
            return
                AsAdviser(
                    this,
                    new IntroduceNamespaceAdvice( this.GetAdviceConstructorParameters( targetNamespace ), name )
                        .Execute( this._state ) );
        }
    }

    public void AddAnnotation<TDeclaration>( TDeclaration declaration, IAnnotation<TDeclaration> annotation, bool export = false )
        where TDeclaration : class, IDeclaration
    {
        using ( this.WithNonUserCode() )
        {
            this.ValidateExplicitInterfaceImplementation( AdviceKind.AddAnnotation );

            if ( this._templateClassInstance == null )
            {
                throw new InvalidOperationException();
            }

            var advice = new AddAnnotationAdvice(
                new(
                    this._state.AspectInstance,
                    this._templateClassInstance,
                    declaration,
                    this._compilation,
                    LayerName: null ),
                new AnnotationInstance( annotation, export, declaration.ToTypedRef<IDeclaration>() ) );

            advice.Execute( this._state );
        }
    }

    private static IClassIntroductionAdviceResult AsAdviser( AdviceFactory<T> adviceFactory, IIntroductionAdviceResult<INamedType> result )
        => new ClassIntroductionAdviceResult( adviceFactory, result );

    private static INamespaceIntroductionAdviceResult AsAdviser( AdviceFactory<T> adviceFactory, IIntroductionAdviceResult<INamespace> result )
        => new NamespaceIntroductionAdviceResult( adviceFactory, result );
}