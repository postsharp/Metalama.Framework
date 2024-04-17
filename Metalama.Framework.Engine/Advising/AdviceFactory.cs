// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EligibilityExtensions = Metalama.Framework.Eligibility.EligibilityExtensions;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.Advising;

internal sealed class AdviceFactory : IAdviceFactory
{
    private readonly string? _layerName;

    private readonly TemplateClassInstance? _templateInstance;
    private readonly CompilationModel _compilation;
    private readonly IDeclaration _aspectTarget;
    private readonly INamedType? _aspectTargetType;
    private readonly AdviceFactoryState _state;
    private readonly ObjectReaderFactory _objectReaderFactory;
    private readonly OtherTemplateClassProvider _otherTemplateClassProvider;

    public AdviceFactory( AdviceFactoryState state, TemplateClassInstance? templateInstance, string? layerName )
    {
        this._state = state;
        this._templateInstance = templateInstance;
        this._layerName = layerName;
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

    public AdviceFactory WithTemplateClassInstance( TemplateClassInstance templateClassInstance ) => new( this._state, templateClassInstance, this._layerName );

    public IAdviceFactory WithTemplateProvider( TemplateProvider templateProvider )
        => this.WithTemplateClassInstance(
            new TemplateClassInstance(
                templateProvider,
                this._otherTemplateClassProvider.Get( templateProvider ) ) );

    public IAdviceFactory WithTemplateProvider( ITemplateProvider templateProvider )
        => this.WithTemplateProvider( TemplateProvider.FromInstance( templateProvider ) );

    private TemplateMemberRef ValidateRequiredTemplateName( string? templateName, TemplateKind templateKind )
        => this.ValidateTemplateName( templateName, templateKind, true )!.Value;

    private TemplateMemberRef? ValidateTemplateName( string? templateName, TemplateKind templateKind, bool required = false, bool ignoreMissing = false )
    {
        if ( this._templateInstance == null )
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

        return ValidateTemplateName( this._templateInstance.TemplateClass, templateName, templateKind, required, ignoreMissing );
    }

    public static TemplateMemberRef? ValidateTemplateName(
        TemplateClass templateClass,
        string templateName,
        TemplateKind templateKind,
        bool required = false,
        bool ignoreMissing = false )
    {
        Invariant.Assert( !(required && ignoreMissing) );

        if ( templateClass.Members.TryGetValue( templateName, out var template ) )
        {
            if ( template.TemplateInfo.IsNone )
            {
                // It is possible that the aspect has a member of the required name, but the user did not use the custom attribute. In this case,
                // we want a proper error message.

                throw GeneralDiagnosticDescriptors.MemberDoesNotHaveTemplateAttribute.CreateException( (template.TemplateClass.FullName, templateName) );
            }

            if ( template.TemplateInfo.IsAbstract )
            {
                if ( !required )
                {
                    return null;
                }
                else
                {
                    throw new AssertionFailedException( "A non-abstract template was expected." );
                }
            }

            return new TemplateMemberRef( template, templateKind );
        }
        else
        {
            if ( ignoreMissing )
            {
                return null;
            }
            else
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMember.CreateException( (templateClass.ShortName, templateName) );
            }
        }
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

    private AdviceResult<T> ExecuteAdvice<T>( Advice advice )
        where T : class, IDeclaration
    {
        List<ITransformation> transformations = new();

        // Initialize the advice. It should report errors for any situation that does not depend on the target declaration.
        // These errors are reported as exceptions.
        var initializationDiagnostics = new DiagnosticBag();
        advice.Initialize( this._state.ServiceProvider, initializationDiagnostics );

        ThrowOnErrors( initializationDiagnostics );
        this._state.Diagnostics.Report( initializationDiagnostics );

        // Implement the advice. This should report errors for any situation that does depend on the target declaration.
        // These errors are reported as diagnostics.
        var result = advice.Implement(
            this._state.ServiceProvider,
            this._state.CurrentCompilation,
            t =>
            {
                this._state.SetOrders( t );
                transformations.Add( t );
            } );

        this._state.Diagnostics.Report( result.Diagnostics );

        this._state.IntrospectionListener?.AddAdviceResult( this._state.AspectInstance, advice, result, this._state.CurrentCompilation );

        switch ( result.Outcome )
        {
            case AdviceOutcome.Error:
                this._state.AspectInstance.Skip();

                break;

            case AdviceOutcome.Ignore:
                break;

            default:
                this._state.AddTransformations( transformations );

                if ( this._state.IntrospectionListener != null )
                {
                    result.Transformations = transformations.ToImmutableArray();
                }

                break;
        }

        return new AdviceResult<T>(
            result.NewDeclaration.As<IDeclaration, T>(),
            this._state.CurrentCompilation,
            result.Outcome,
            this._state.AspectBuilder.AssertNotNull(),
            advice.AdviceKind,
            result.Interfaces,
            result.InterfaceMembers );
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

    public ICompilation MutableCompilation => this._state.CurrentCompilation;

    private void CheckEligibility( IDeclaration declaration, AdviceKind adviceKind, params IDeclaration[] otherTargets )
    {
        var rule = EligibilityRuleFactory.GetAdviceEligibilityRule( adviceKind );

        if ( (rule.GetEligibility( declaration ) & EligibleScenarios.Default) == 0 )
        {
            var justification = rule.GetIneligibilityJustification( EligibleScenarios.Default, new DescribedObject<IDeclaration>( declaration ) );

            throw new InvalidOperationException(
                MetalamaStringFormatter.Format(
                    $"Cannot add an {adviceKind} advice to '{declaration}' because {justification}. Check the {nameof(EligibilityExtensions.IsAdviceEligible)}({nameof(AdviceKind)}.{adviceKind}) method." ) );
        }

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

        this.ValidateTarget( declaration, Array.Empty<IDeclaration>() );
    }

    private void ValidateTarget( IDeclaration declaration, IDeclaration[] otherTargets )
    {
        ValidateOneTarget( declaration );

        foreach ( var d in otherTargets )
        {
            ValidateOneTarget( d );
        }

        void ValidateOneTarget( IDeclaration target )
        {
            // Check that the compilation match.
            if ( !ReferenceEquals( target.Compilation, this._compilation ) && !ReferenceEquals( target.Compilation, this._state.CurrentCompilation ) )
            {
                throw new InvalidOperationException( MetalamaStringFormatter.Format( $"The target declaration is not in the current compilation." ) );
            }

            // Check that the advised target is under the aspect target.
            if ( !target.ForCompilation( this.MutableCompilation ).IsContainedIn( this._aspectTargetType ?? this._aspectTarget ) )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format(
                        $"The advised target '{target}' is not contained in the target of the aspect '{this._aspectTargetType ?? this._aspectTarget}'." ) );
            }

            // Check other targets.
            foreach ( var t in otherTargets )
            {
                ValidateOneTarget( t );
            }
        }
    }

    public IOverrideAdviceResult<IMethod> Override(
        IMethod targetMethod,
        in MethodTemplateSelector templateSelector,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetMethod, AdviceKind.OverrideMethod );

            Advice advice;

            switch ( targetMethod.MethodKind )
            {
                case MethodKind.EventAdd:
                    {
                        var @event = (IEvent) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateRequiredTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( @event.AddMethod, this.GetObjectReader( args ) );

                        advice = new OverrideEventAdvice(
                            this._state.AspectInstance,
                            this._templateInstance,
                            @event,
                            this._state.CurrentCompilation,
                            template,
                            null,
                            this._layerName,
                            this.GetObjectReader( tags ) );
                    }

                    break;

                case MethodKind.EventRemove:
                    {
                        var @event = (IEvent) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateRequiredTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( @event.AddMethod, this.GetObjectReader( args ) );

                        advice = new OverrideEventAdvice(
                            this._state.AspectInstance,
                            this._templateInstance,
                            @event,
                            this._state.CurrentCompilation,
                            null,
                            template,
                            this._layerName,
                            this.GetObjectReader( tags ) );
                    }

                    break;

                case MethodKind.PropertyGet:
                    {
                        var propertyOrIndexer = (IPropertyOrIndexer) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.SelectGetterTemplate( propertyOrIndexer, templateSelector.AsGetterTemplateSelector(), true )
                            ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) );

                        switch ( propertyOrIndexer )
                        {
                            case IProperty property:
                                advice = new OverrideFieldOrPropertyAdvice(
                                    this._state.AspectInstance,
                                    this._templateInstance,
                                    property,
                                    this._state.CurrentCompilation,
                                    template,
                                    null,
                                    this._layerName,
                                    this.GetObjectReader( tags ) );

                                break;

                            case IIndexer indexer:
                                advice = new OverrideIndexerAdvice(
                                    this._state.AspectInstance,
                                    this._templateInstance,
                                    indexer,
                                    this._state.CurrentCompilation,
                                    template,
                                    null,
                                    this._layerName,
                                    this.GetObjectReader( tags ) );

                                break;

                            default:
                                throw new AssertionFailedException( $"Unexpected declaration {propertyOrIndexer.DeclarationKind}." );
                        }
                    }

                    break;

                case MethodKind.PropertySet:
                    {
                        var propertyOrIndexer = (IPropertyOrIndexer) targetMethod.ContainingDeclaration.AssertNotNull();

                        var template = this.ValidateTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default, true )
                            ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) );

                        switch ( propertyOrIndexer )
                        {
                            case IProperty property:
                                advice = new OverrideFieldOrPropertyAdvice(
                                    this._state.AspectInstance,
                                    this._templateInstance,
                                    property,
                                    this._state.CurrentCompilation,
                                    null,
                                    template,
                                    this._layerName,
                                    this.GetObjectReader( tags ) );

                                break;

                            case IIndexer indexer:
                                advice = new OverrideIndexerAdvice(
                                    this._state.AspectInstance,
                                    this._templateInstance,
                                    indexer,
                                    this._state.CurrentCompilation,
                                    null,
                                    template,
                                    this._layerName,
                                    this.GetObjectReader( tags ) );

                                break;

                            default:
                                throw new AssertionFailedException( $"Unexpected declaration {propertyOrIndexer.DeclarationKind}." );
                        }
                    }

                    break;

                default:
                    {
                        var template = this.SelectMethodTemplate( targetMethod, templateSelector )
                            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                            .ForOverride( targetMethod, this.GetObjectReader( args ) )
                            .AssertNotNull();

                        advice = new OverrideMethodAdvice(
                            this._state.AspectInstance,
                            this._templateInstance,
                            targetMethod,
                            this._state.CurrentCompilation,
                            template,
                            this._layerName,
                            this.GetObjectReader( tags ) );

                        break;
                    }
            }

            return this.ExecuteAdvice<IMethod>( advice );
        }
    }

    public IIntroductionAdviceResult<INamedType> IntroduceType( INamespace targetNamespace, string typeName, Action<INamedTypeBuilder>? buildType = null )
    {
        if ( this._templateInstance == null )
        {
            throw new InvalidOperationException();
        }

        using ( this.WithNonUserCode() )
        {
            var advice = new IntroduceTypeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetNamespace,
                typeName,
                this._state.CurrentCompilation,
                buildType,
                this._layerName );

            return this.ExecuteAdvice<INamedType>( advice );
        }
    }

    public IIntroductionAdviceResult<INamedType> IntroduceType( INamedType targetType, string typeName, Action<INamedTypeBuilder>? buildType = null )
    {
        if ( this._templateInstance == null )
        {
            throw new InvalidOperationException();
        }

        using ( this.WithNonUserCode() )
        {
            var advice = new IntroduceTypeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                typeName,
                this._state.CurrentCompilation,
                buildType,
                this._layerName );

            return this.ExecuteAdvice<INamedType>( advice );
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
        if ( this._templateInstance == null )
        {
            throw new InvalidOperationException();
        }

        using ( this.WithNonUserCode() )
        {
            this.CheckEligibility( targetType, AdviceKind.IntroduceMethod );

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                !.Value
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceMethodAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                scope,
                whenExists,
                buildMethod,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
        }
    }

    public IIntroductionAdviceResult<IMethod> IntroduceFinalizer(
        INamedType targetType,
        string defaultTemplate,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        object? args = null,
        object? tags = null )
    {
        if ( this._templateInstance == null )
        {
            throw new InvalidOperationException();
        }

        using ( this.WithNonUserCode() )
        {
            this.CheckEligibility( targetType, AdviceKind.IntroduceFinalizer );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceFinalizerAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( kind.GetCategory() != OperatorCategory.Unary )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format( $"Cannot add an IntroduceUnaryOperator advice with {kind} as it is not an unary operator." ) );
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceOperatorAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                kind,
                inputType,
                null,
                resultType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( kind.GetCategory() != OperatorCategory.Binary )
            {
                throw new InvalidOperationException(
                    MetalamaStringFormatter.Format( $"Cannot add an IntroduceBinaryOperator advice with {kind} as it is not a binary operator." ) );
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceOperatorAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                kind,
                leftType,
                rightType,
                resultType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceOperator );

            var template = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var operatorKind = isImplicit ? OperatorKind.ImplicitConversion : OperatorKind.ExplicitConversion;

            var advice = new IntroduceOperatorAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                operatorKind,
                fromType,
                null,
                toType,
                template.PartialForIntroduction( this.GetObjectReader( args ) ),
                whenExists,
                buildAction,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetConstructor, AdviceKind.OverrideConstructor );

            var boundTemplate =
                this.ValidateTemplateName( template, TemplateKind.Default, true )
                    ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetConstructor, this.GetObjectReader( args ) );

            var advice = new OverrideConstructorAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetConstructor,
                this._compilation,
                boundTemplate.AssertNotNull(),
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IConstructor>( advice );
        }
    }

    public IOverrideAdviceResult<IProperty> Override(
        IFieldOrProperty targetFieldOrProperty,
        string template,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetFieldOrProperty, AdviceKind.OverrideFieldOrPropertyOrIndexer );

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
                this._state.AspectInstance,
                this._templateInstance,
                targetFieldOrProperty,
                this._state.CurrentCompilation,
                getTemplate,
                setTemplate,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
        }
    }

    public IOverrideAdviceResult<IProperty> OverrideAccessors(
        IFieldOrPropertyOrIndexer targetFieldOrPropertyOrIndexer,
        in GetterTemplateSelector getTemplateSelector,
        string? setTemplate = null,
        object? args = null,
        object? tags = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetFieldOrPropertyOrIndexer, AdviceKind.OverrideFieldOrPropertyOrIndexer );

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
                            this._state.AspectInstance,
                            this._templateInstance,
                            targetFieldOrProperty,
                            this._state.CurrentCompilation,
                            boundGetTemplate,
                            boundSetTemplate,
                            this._layerName,
                            this.GetObjectReader( tags ) );

                        return this.ExecuteAdvice<IProperty>( advice );
                    }

                case IIndexer targetIndexer:
                    {
                        var advice = new OverrideIndexerAdvice(
                            this._state.AspectInstance,
                            this._templateInstance,
                            targetIndexer,
                            this._state.CurrentCompilation,
                            boundGetTemplate,
                            boundSetTemplate,
                            this._layerName,
                            this.GetObjectReader( tags ) );

                        return this.ExecuteAdvice<IProperty>( advice );
                    }

                default:
                    throw new AssertionFailedException( $"{targetFieldOrPropertyOrIndexer.GetType().Name} is not expected here." );
            }
        }
    }

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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceField );

            var template = this.ValidateRequiredTemplateName( templateName, TemplateKind.Default )
                .GetTemplateMember<IField>( this._compilation, this._state.ServiceProvider );

            var advice = new IntroduceFieldAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                null,
                template,
                scope,
                whenExists,
                buildField,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IField>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceField );

            var advice = new IntroduceFieldAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                fieldName,
                default,
                scope,
                whenExists,
                builder =>
                {
                    builder.Type = fieldType;
                    buildField?.Invoke( builder );
                },
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IField>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceProperty );

            var advice = new IntroducePropertyAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                propertyName,
                propertyType,
                default,
                default,
                default,
                scope,
                whenExists,
                buildProperty,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceProperty );

            var propertyTemplate = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IProperty>( this._compilation, this._state.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();

            var advice = new IntroducePropertyAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                null,
                null,
                propertyTemplate,
                accessorTemplates.Get?.PartialForIntroduction(),
                accessorTemplates.Set?.PartialForIntroduction(),
                scope,
                whenExists,
                buildProperty,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( getTemplate == null && setTemplate == null )
            {
                throw new ArgumentNullException( nameof(getTemplate), "Either getTemplate or setTemplate must be provided." );
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceProperty );

            var boundGetTemplate = this.ValidateTemplateName( getTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundSetTemplate = this.ValidateTemplateName( setTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroducePropertyAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                name,
                null,
                default,
                boundGetTemplate?.PartialForIntroduction( parameterReaders ),
                boundSetTemplate?.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildProperty,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( getTemplate == null && setTemplate == null )
            {
                throw new ArgumentNullException( nameof(getTemplate), "Either getTemplate or setTemplate must be provided." );
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceIndexer );

            var boundGetTemplate = this.ValidateTemplateName( getTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundSetTemplate = this.ValidateTemplateName( setTemplate, TemplateKind.Default )
                ?.GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroduceIndexerAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                indices,
                boundGetTemplate?.PartialForIntroduction( parameterReaders ),
                boundSetTemplate?.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildIndexer,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IIndexer>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( invokeTemplate != null )
            {
                throw GeneralDiagnosticDescriptors.UnsupportedFeature.CreateException( $"Invoker overrides." );
            }

            if ( invokeTemplate != null )
            {
                throw new NotImplementedException( "Support for overriding event raisers is not yet implemented." );
            }

            this.CheckEligibility( targetEvent, AdviceKind.OverrideEvent );

            var boundAddTemplate =
                this.ValidateRequiredTemplateName( addTemplate, TemplateKind.Default )
                    .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetEvent.AddMethod, this.GetObjectReader( args ) );

            var boundRemoveTemplate =
                this.ValidateRequiredTemplateName( removeTemplate, TemplateKind.Default )
                    .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider )
                    .ForOverride( targetEvent.RemoveMethod, this.GetObjectReader( args ) );

            var advice = new OverrideEventAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetEvent,
                this._state.CurrentCompilation,
                boundAddTemplate,
                boundRemoveTemplate,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IEvent>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceEvent );

            var eventTemplate = this.ValidateRequiredTemplateName( defaultTemplate, TemplateKind.Default )
                .GetTemplateMember<IEvent>( this._compilation, this._state.ServiceProvider );

            var accessorTemplates = eventTemplate.GetAccessorTemplates();

            var advice = new IntroduceEventAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                null,
                eventTemplate,
                accessorTemplates.Add?.PartialForIntroduction(),
                accessorTemplates.Remove?.PartialForIntroduction(),
                scope,
                whenExists,
                buildEvent,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IEvent>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.IntroduceEvent );

            var boundAddTemplate = this.ValidateRequiredTemplateName( addTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var boundRemoveTemplate = this.ValidateRequiredTemplateName( removeTemplate, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var parameterReaders = this.GetObjectReader( args );

            var advice = new IntroduceEventAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                name,
                default,
                boundAddTemplate.PartialForIntroduction( parameterReaders ),
                boundRemoveTemplate.PartialForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildEvent,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IEvent>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.ImplementInterface );

            var advice = new ImplementInterfaceAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                interfaceType,
                whenExists,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<INamedType>( advice );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.AddInitializer );

            var boundTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new TemplateBasedInitializeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                boundTemplate.ForInitializer( this.GetObjectReader( args ) ),
                kind,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<INamedType>( advice );
        }
    }

    public IAddInitializerAdviceResult AddInitializer(
        INamedType targetType,
        IStatement statement,
        InitializerKind kind )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetType, AdviceKind.AddInitializer );

            var advice = new SyntaxBasedInitializeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetType,
                this._state.CurrentCompilation,
                statement,
                kind,
                this._layerName );

            return this.ExecuteAdvice<INamedType>( advice );
        }
    }

    public IAddInitializerAdviceResult AddInitializer( IConstructor targetConstructor, string template, object? tags = null, object? args = null )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetConstructor, AdviceKind.AddInitializer );

            var boundTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
                .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

            var advice = new TemplateBasedInitializeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetConstructor,
                this._state.CurrentCompilation,
                boundTemplate.ForInitializer( this.GetObjectReader( args ) ),
                InitializerKind.BeforeInstanceConstructor,
                this._layerName,
                this.GetObjectReader( tags ) );

            return this.ExecuteAdvice<IConstructor>( advice );
        }
    }

    public IAddInitializerAdviceResult AddInitializer( IConstructor targetConstructor, IStatement statement )
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( targetConstructor, AdviceKind.AddInitializer );

            var advice = new SyntaxBasedInitializeAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                targetConstructor,
                this._state.CurrentCompilation,
                statement,
                InitializerKind.BeforeInstanceConstructor,
                this._layerName );

            return this.ExecuteAdvice<IConstructor>( advice );
        }
    }

    private static void ThrowOnErrors( DiagnosticBag diagnosticBag )
    {
        if ( diagnosticBag.HasError() )
        {
            throw new DiagnosticException(
                "Errors have occured while creating advice.",
                diagnosticBag.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
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

            return this.AddContractImpl<IParameter>( targetParameter, template, kind, tags, args );
        }
    }

    public IIntroductionAdviceResult<IPropertyOrIndexer> AddContract(
        IFieldOrPropertyOrIndexer targetMember,
        string template,
        ContractDirection kind = ContractDirection.Default,
        object? tags = null,
        object? args = null )
        => this.AddContractImpl<IPropertyOrIndexer>( targetMember, template, kind, tags, args );

    private AdviceResult<T> AddContractImpl<T>(
        IDeclaration targetDeclaration,
        string template,
        ContractDirection direction,
        object? tags,
        object? args )
        where T : class, IDeclaration
    {
        if ( this._templateInstance == null )
        {
            throw new InvalidOperationException();
        }

        if ( direction == ContractDirection.None )
        {
            return new AdviceResult<T>(
                null,
                this._state.CurrentCompilation,
                AdviceOutcome.Ignore,
                this._state.AspectBuilder.AssertNotNull(),
                AdviceKind.AddContract,
                Array.Empty<IInterfaceImplementationResult>(),
                Array.Empty<IInterfaceMemberImplementationResult>() );
        }

        this.CheckContractEligibility( targetDeclaration, direction );

        direction = ContractAspectHelper.GetEffectiveDirection( direction, targetDeclaration );

        var boundTemplate = this.ValidateRequiredTemplateName( template, TemplateKind.Default )
            .GetTemplateMember<IMethod>( this._compilation, this._state.ServiceProvider );

        var advice = new ContractAdvice(
            this._state.AspectInstance,
            this._templateInstance,
            targetDeclaration,
            this._compilation,
            boundTemplate,
            direction,
            this._layerName,
            this.GetObjectReader( tags ),
            this.GetObjectReader( args ) );

        var result = this.ExecuteAdvice<T>( advice );

        return result;
    }

    public IIntroductionAdviceResult<IAttribute> IntroduceAttribute(
        IDeclaration targetDeclaration,
        IAttributeData attribute,
        OverrideStrategy whenExists = OverrideStrategy.Default )
        => this.ExecuteAdvice<IAttribute>(
            new AddAttributeAdvice(
                this._state.AspectInstance,
                this._templateInstance!,
                targetDeclaration,
                this._compilation,
                attribute,
                whenExists,
                this._layerName ) );

    public IRemoveAttributesAdviceResult RemoveAttributes( IDeclaration targetDeclaration, INamedType attributeType )
    {
        using ( this.WithNonUserCode() )
        {
            return this.ExecuteAdvice<IDeclaration>(
                new RemoveAttributesAdvice(
                    this._state.AspectInstance,
                    this._templateInstance!,
                    targetDeclaration,
                    this._state.CurrentCompilation,
                    attributeType,
                    this._layerName ) );
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
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            this.CheckEligibility( constructor, AdviceKind.IntroduceParameter );

            var advice = new IntroduceConstructorParameterAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                constructor,
                this._state.CurrentCompilation,
                this._layerName,
                parameterName,
                parameterType,
                attributes.IsDefaultOrEmpty ? null : builder => builder.AddAttributes( attributes ),
                pullAction,
                defaultValue );

            return this.ExecuteAdvice<IParameter>( advice );
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

    public void AddAnnotation<TDeclaration>( TDeclaration declaration, IAnnotation<TDeclaration> annotation, bool export = false )
        where TDeclaration : class, IDeclaration
    {
        using ( this.WithNonUserCode() )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var advice = new AddAnnotationAdvice(
                this._state.AspectInstance,
                this._templateInstance,
                declaration,
                this._state.CurrentCompilation,
                new AnnotationInstance( annotation, export, declaration.ToTypedRef<IDeclaration>() ) );

            this.ExecuteAdvice<IDeclaration>( advice );
        }
    }
}