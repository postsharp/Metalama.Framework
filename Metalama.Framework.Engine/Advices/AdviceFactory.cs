// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.DependencyInjection;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using RefKind = Metalama.Framework.Code.RefKind;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.Advices
{
    /// <summary>
    /// Represents the result of a method of <see cref="IAdviceFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of declaration returned by the advice method.</typeparam>
    internal class AdviceResult<T> : IIntroductionAdviceResult<T>
        where T : class, ICompilationElement
    {
        private readonly IRef<T> _declaration;
        private readonly ICompilation _compilation;

        /// <summary>
        /// Gets the declaration created or transformed by the advice method. For introduction advice methods, this is the introduced declaration when a new
        /// declaration is introduced, or the existing declaration when a declaration of the same name and signature already exists. For advice that modify a field,
        /// this is the property that now represents the field.
        /// </summary>
        public T Declaration
            => this.Outcome != AdviceOutcome.Error
                ? this._declaration.GetTarget( this._compilation )
                : throw new InvalidOperationException( "Cannot get the resulting declaration when the outcome is Error." );

        public AdviceOutcome Outcome { get; }

        internal AdviceResult( IRef<T> declaration, ICompilation compilation, AdviceOutcome outcome )
        {
            this._declaration = declaration;
            this._compilation = compilation;
            this.Outcome = outcome;
        }
    }

    [Obfuscation( Exclude = true )] // Not obfuscated to have a decent call stack in case of user exception.
    internal class AdviceFactory : IAdviceFactory
    {
        private readonly string? _layerName;

        private readonly TemplateClassInstance? _templateInstance;

        public AdviceFactoryState State { get; }

        public AdviceFactory( AdviceFactoryState state, TemplateClassInstance? templateInstance, string? layerName )
        {
            this.State = state;
            this._templateInstance = templateInstance;
            this._layerName = layerName;
        }

        public AdviceFactory WithTemplateClassInstance( TemplateClassInstance templateClassInstance )
            => new( this.State, templateClassInstance, this._layerName );

        public IAdviceFactory WithTemplateProvider( ITemplateProvider templateProvider )
        {
            return this.WithTemplateClassInstance(
                new TemplateClassInstance( templateProvider, this.State.PipelineConfiguration.OtherTemplateClasses[templateProvider.GetType().FullName] ) );
        }

        private TemplateMemberRef ValidateTemplateName( string? templateName, TemplateKind templateKind, bool required = false )
        {
            if ( this._templateInstance == null )
            {
                throw new AssertionFailedException();
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
                    return default;
                }
            }
            else if ( this._templateInstance.TemplateClass.Members.TryGetValue( templateName, out var template ) )
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
                        return default;
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
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMember.CreateException(
                    (this._templateInstance.TemplateClass.ShortName, templateName) );
            }
        }

        private TemplateMemberRef SelectTemplate( IMethod targetMethod, in MethodTemplateSelector templateSelector )
        {
            var defaultTemplate = this.ValidateTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default, true );
            var asyncTemplate = this.ValidateTemplateName( templateSelector.AsyncTemplate, TemplateKind.Async );

            var enumerableTemplate = this.ValidateTemplateName( templateSelector.EnumerableTemplate, TemplateKind.IEnumerable );
            var enumeratorTemplate = this.ValidateTemplateName( templateSelector.EnumeratorTemplate, TemplateKind.IEnumerator );
            var asyncEnumerableTemplate = this.ValidateTemplateName( templateSelector.AsyncEnumerableTemplate, TemplateKind.IAsyncEnumerable );

            var asyncEnumeratorTemplate = this.ValidateTemplateName(
                templateSelector.AsyncEnumeratorTemplate,
                TemplateKind.IAsyncEnumerator );

            var interpretedKind = TemplateKind.Default;

            var selectedTemplate = defaultTemplate;

            var asyncInfo = targetMethod.GetAsyncInfoImpl();
            var iteratorInfo = targetMethod.GetIteratorInfoImpl();

            // See if we have an async template, which actually does not need to have an async implementation, does not not need to 
            // be applied only on methods with async implementations. However, if the template has an async implementation, the
            // target awaitable type must be compatible with an async implementation, i.e. it must have a method builder.

            if ( asyncInfo.IsAsync || (templateSelector.UseAsyncTemplateForAnyAwaitable && ((asyncInfo.IsAwaitable && asyncInfo.HasMethodBuilder) ||
                                                                                            iteratorInfo.EnumerableKind is EnumerableKind.IAsyncEnumerable or
                                                                                                EnumerableKind.IAsyncEnumerator)) )
            {
                interpretedKind = TemplateKind.Async;

                if ( !asyncTemplate.IsNull )
                {
                    selectedTemplate = asyncTemplate;

                    // We don't return because the result can still be overwritten by async iterators.
                }
            }

            var useIteratorTemplate = iteratorInfo.IsIterator
                                      || (templateSelector.UseEnumerableTemplateForAnyEnumerable && iteratorInfo.EnumerableKind != EnumerableKind.None);

            switch ( iteratorInfo.EnumerableKind )
            {
                case EnumerableKind.None:
                    break;

                case EnumerableKind.UntypedIEnumerable:
                case EnumerableKind.IEnumerable:
                    if ( useIteratorTemplate && !enumerableTemplate.IsNull )
                    {
                        return enumerableTemplate;
                    }
                    else
                    {
                        interpretedKind = TemplateKind.IEnumerable;
                    }

                    break;

                case EnumerableKind.UntypedIEnumerator:
                case EnumerableKind.IEnumerator:
                    if ( useIteratorTemplate && !enumeratorTemplate.IsNull )
                    {
                        return enumeratorTemplate;
                    }
                    else
                    {
                        interpretedKind = TemplateKind.IEnumerator;
                    }

                    break;

                case EnumerableKind.IAsyncEnumerable:
                    if ( useIteratorTemplate && !asyncEnumerableTemplate.IsNull )
                    {
                        return asyncEnumerableTemplate;
                    }
                    else
                    {
                        interpretedKind = TemplateKind.IAsyncEnumerable;
                    }

                    break;

                case EnumerableKind.IAsyncEnumerator:
                    if ( useIteratorTemplate && !asyncEnumeratorTemplate.IsNull )
                    {
                        return asyncEnumeratorTemplate;
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
            where T : class, ICompilationElement
        {
            List<ITransformation> transformations = new();

            // Initialize the advice. It should report errors for any situation that does not depend on the target declaration.
            // These errors are reported as exceptions.
            var initializationDiagnostics = new DiagnosticList();
            advice.Initialize( this.State.ServiceProvider, initializationDiagnostics );

            ThrowOnErrors( initializationDiagnostics );
            this.State.Diagnostics.Report( initializationDiagnostics );

            // Implement the aspect. This should report errors for any situation that does depend on the target declaration.
            // These errors are reported as diagnostics.
            var result = advice.Implement(
                this.State.ServiceProvider,
                this.State.CurrentCompilation,
                t =>
                {
                    t.OrderWithinAspectInstance = this.State.GetTransformationOrder();
                    transformations.Add( t );
                } );

            this.State.Diagnostics.Report( result.Diagnostics );

            this.State.IntrospectionListener?.AddAdviceResult( this.State.AspectInstance, advice, result, this.State.CurrentCompilation );

            switch ( result.Outcome )
            {
                case AdviceOutcome.Error:
                    this.State.SkipAspect();

                    break;

                case AdviceOutcome.Ignored:
                    break;

                default:
                    this.State.AddTransformations( transformations );

                    if ( this.State.IntrospectionListener != null )
                    {
                        result.Transformations = transformations.ToImmutableArray();
                    }

                    break;
            }

            return new AdviceResult<T>( result.NewDeclaration.As<T>(), this.State.CurrentCompilation, result.Outcome );
        }

        private CompilationModel ValidateCompilation( IDeclaration target, IDeclaration? target2 = null ) 
        {
            if ( target.Compilation != this.State.InitialCompilation && target.Compilation != this.State.CurrentCompilation )
            {
                throw new InvalidOperationException( "The target declaration is not in the current compilation." );
            }

            if ( target2 != null && target2.Compilation != target.Compilation )
            {
                throw new InvalidOperationException( "Compilation mismatch." );
            }

            return target.GetCompilationModel();
        }

        private TemplateMemberRef SelectTemplate( IFieldOrPropertyOrIndexer targetFieldOrProperty, in GetterTemplateSelector templateSelector, bool required )
        {
            var getter = targetFieldOrProperty.GetMethod;

            if ( getter == null )
            {
                return default;
            }

            var defaultTemplate = this.ValidateTemplateName( templateSelector.DefaultTemplate, TemplateKind.Default, required );
            var enumerableTemplate = this.ValidateTemplateName( templateSelector.EnumerableTemplate, TemplateKind.IEnumerable );
            var enumeratorTemplate = this.ValidateTemplateName( templateSelector.EnumeratorTemplate, TemplateKind.IEnumerator );

            var selectedTemplate = defaultTemplate;

            if ( !templateSelector.HasOnlyDefaultTemplate )
            {
                var iteratorInfo = getter.GetIteratorInfoImpl();

                if ( !enumerableTemplate.IsNull && iteratorInfo.IsIterator )
                {
                    selectedTemplate = enumerableTemplate;
                }

                if ( !enumeratorTemplate.IsNull && iteratorInfo.EnumerableKind is EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator )
                {
                    return enumeratorTemplate;
                }
            }

            return selectedTemplate;
        }

        public ICompilation Compilation => this.State.CurrentCompilation;

        public void Override( IMethod targetMethod, in MethodTemplateSelector templateSelector, object? args = null, object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetMethod.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an OverrideMethod advice to '{targetMethod}' because it is an abstract." ) );
            }

            var compilation = this.ValidateCompilation( targetMethod );

            var template = this.SelectTemplate( targetMethod, templateSelector )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider )
                .ForOverride( targetMethod, ObjectReader.GetReader( args ) );

            var advice = new OverrideMethodAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetMethod,
                template,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            this.ExecuteAdvice<IMethod>( advice );
        }

        public IIntroductionAdviceResult<IMethod> IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IMethodBuilder>? buildAction = null,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetType.TypeKind == TypeKind.Interface )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an IntroduceMethod advice to '{targetType}' because it is an interface." ) );
            }

            var compilation = this.ValidateCompilation( targetType );

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var advice = new IntroduceMethodAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                template.ForIntroduction( ObjectReader.GetReader( args ) ),
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            return this.ExecuteAdvice<IMethod>( advice );
        }

        public IAdviceResult<IProperty> Override(
            IFieldOrProperty targetFieldOrProperty,
            string defaultTemplate,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetFieldOrProperty.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an OverrideFieldOrProperty advice to '{targetFieldOrProperty}' because it is an abstract." ) );
            }

            var compilation = this.ValidateCompilation( targetFieldOrProperty );

            // Set template represents both set and init accessors.
            var propertyTemplate = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IProperty>( compilation, this.State.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();
            var getTemplate = accessorTemplates.Get;
            var setTemplate = accessorTemplates.Set;

            var advice = new OverrideFieldOrPropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetFieldOrProperty,
                propertyTemplate,
                getTemplate.ForIntroduction(),
                setTemplate.ForIntroduction(),
                this._layerName,
                ObjectReader.GetReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
        }

        public IAdviceResult<IProperty> OverrideAccessors(
            IFieldOrProperty targetFieldOrProperty,
            in GetterTemplateSelector getTemplateSelector,
            string? setTemplate = null,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetFieldOrProperty.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format(
                        $"Cannot add an OverrideFieldOrPropertyAccessors advice to '{targetFieldOrProperty}' because it is an abstract." ) );
            }

            var compilation = this.ValidateCompilation( targetFieldOrProperty );

            // Set template represents both set and init accessors.
            var getTemplateRef = this.SelectTemplate( targetFieldOrProperty, getTemplateSelector, setTemplate == null )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider )
                .ForOverride( targetFieldOrProperty.GetMethod, ObjectReader.GetReader( args ) );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, getTemplateSelector.IsNull )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider )
                .ForOverride( targetFieldOrProperty.SetMethod, ObjectReader.GetReader( args ) );

            if ( getTemplateRef.IsNull && setTemplateRef.IsNull )
            {
                throw new InvalidOperationException( "There is no accessor to override." );
            }

            var advice = new OverrideFieldOrPropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetFieldOrProperty,
                default,
                getTemplateRef,
                setTemplateRef,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            return this.ExecuteAdvice<IProperty>( advice );
        }

        public IIntroductionAdviceResult<IField> IntroduceField(
            INamedType targetType,
            string templateName,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IFieldBuilder>? buildAction = null,
            object? tags = null,
            IPullStrategy? pullStrategy = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var compilation = this.ValidateCompilation( targetType );

            var template = this.ValidateTemplateName( templateName, TemplateKind.Default, true )
                .GetTemplateMember<IField>( compilation, this.State.ServiceProvider );

            var advice = new IntroduceFieldAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                null,
                template,
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                pullStrategy );

            return this.ExecuteAdvice<IField>( advice );
        }

        public IIntroductionAdviceResult<IField> IntroduceField(
            INamedType targetType,
            string fieldName,
            IType fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IFieldBuilder>? buildAction = null,
            object? tags = null,
            IPullStrategy? pullStrategy = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var advice = new IntroduceFieldAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                fieldName,
                default,
                scope,
                whenExists,
                builder =>
                {
                    builder.Type = fieldType;
                    buildAction?.Invoke( builder );
                },
                this._layerName,
                ObjectReader.GetReader( tags ),
                pullStrategy );

            return this.ExecuteAdvice<IField>( advice );
        }

        public IIntroductionAdviceResult<IField> IntroduceField(
            INamedType targetType,
            string fieldName,
            Type fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IFieldBuilder>? buildAction = null,
            object? tags = null,
            IPullStrategy? pullStrategy = null )
            => this.IntroduceField(
                targetType,
                fieldName,
                this.State.InitialCompilation.Factory.GetTypeByReflectionType( fieldType ),
                scope,
                whenExists,
                buildAction,
                tags,
                pullStrategy );

        public IIntroductionAdviceResult<IProperty> IntroduceAutomaticProperty(
            INamedType targetType,
            string propertyName,
            IType propertyType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IPropertyBuilder>? buildAction = null,
            object? tags = null,
            IPullStrategy? pullStrategy = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var advice = new IntroducePropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                propertyName,
                propertyType,
                default,
                default,
                default,
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                pullStrategy );

            return this.ExecuteAdvice<IProperty>( advice );
        }

        public IIntroductionAdviceResult<IProperty> IntroduceAutomaticProperty(
            INamedType targetType,
            string propertyName,
            Type propertyType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IPropertyBuilder>? buildAction = null,
            object? tags = null,
            IPullStrategy? pullStrategy = null )
            => this.IntroduceAutomaticProperty(
                targetType,
                propertyName,
                this.State.InitialCompilation.Factory.GetTypeByReflectionType( propertyType ),
                scope,
                whenExists,
                buildAction,
                tags );

        public IIntroductionAdviceResult<IProperty> IntroduceProperty(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IPropertyBuilder>? buildAction = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetType.TypeKind == TypeKind.Interface )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an IntroduceMethod advice to '{targetType}' because it is an interface." ) );
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var propertyTemplate = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IProperty>( compilation, this.State.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();

            var advice = new IntroducePropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                null,
                null,
                propertyTemplate,
                accessorTemplates.Get.ForIntroduction(),
                accessorTemplates.Set.ForIntroduction(),
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                null );

            return this.ExecuteAdvice<IProperty>( advice );
        }

        public IIntroductionAdviceResult<IProperty> IntroduceProperty(
            INamedType targetType,
            string name,
            string? getTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IPropertyBuilder>? buildAction = null,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetType.TypeKind == TypeKind.Interface )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an IntroduceMethod advice to '{targetType}' because it is an interface." ) );
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var getTemplateRef = this.ValidateTemplateName( getTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var parameterReaders = ObjectReader.GetReader( args );

            var advice = new IntroducePropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                name,
                null,
                default,
                getTemplateRef.ForIntroduction( parameterReaders ),
                setTemplateRef.ForIntroduction( parameterReaders ),
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                null );

            return this.ExecuteAdvice<IProperty>( advice );
        }

        public void OverrideAccessors(
            IEvent targetEvent,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( invokeTemplate != null )
            {
                throw GeneralDiagnosticDescriptors.UnsupportedFeature.CreateException( $"Invoker overrides." );
            }

            if ( targetEvent.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an OverrideEventAccessors advice to '{targetEvent}' because it is an abstract." ) );
            }
            
            var compilation = this.ValidateCompilation( targetEvent );

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            if ( invokeTemplate != null )
            {
                throw new NotImplementedException( "Support for overriding event raisers is not yet implemented." );
            }

            var advice = new OverrideEventAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetEvent,
                default,
                addTemplateRef,
                removeTemplateRef,
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.GetReader( args ) );

            this.ExecuteAdvice<IEvent>( advice );
        }

        public IIntroductionAdviceResult<IEvent> IntroduceEvent(
            INamedType targetType,
            string eventTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IEventBuilder>? buildAction = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetType.TypeKind == TypeKind.Interface )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an IntroduceMethod advice to '{targetType}' because it is an interface." ) );
            }

            var compilation = this.ValidateCompilation( targetType );

            var template = this.ValidateTemplateName( eventTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IEvent>( compilation, this.State.ServiceProvider );

            var advice = new IntroduceEventAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                null,
                template,
                default,
                default,
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.Empty );

            return this.ExecuteAdvice<IEvent>( advice );
        }

        public IIntroductionAdviceResult<IEvent> IntroduceEvent(
            INamedType targetType,
            string name,
            string addTemplate,
            string removeTemplate,
            string? invokeTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Action<IEventBuilder>? buildAction = null,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetType.TypeKind == TypeKind.Interface )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an IntroduceMethod advice to '{targetType}' because it is an interface." ) );
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var advice = new IntroduceEventAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                name,
                default,
                addTemplateRef,
                removeTemplateRef,
                scope,
                whenExists,
                buildAction,
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.GetReader( args ) );

            return this.ExecuteAdvice<IEvent>( advice );
        }

        public void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var advice = new ImplementInterfaceAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                interfaceType,
                whenExists,
                null,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            this.ExecuteAdvice<INamedType>( advice );
        }

        public void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            this.ImplementInterface(
                targetType,
                (INamedType) targetType.GetCompilationModel().Factory.GetTypeByReflectionType( interfaceType ),
                whenExists,
                tags );
        }

        public void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var advice = new ImplementInterfaceAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                interfaceType,
                whenExists,
                interfaceMemberSpecifications,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            this.ExecuteAdvice<INamedType>( advice );
        }

        public void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            this.ImplementInterface(
                targetType,
                (INamedType) targetType.GetCompilationModel().Factory.GetTypeByReflectionType( interfaceType ),
                interfaceMemberSpecifications,
                whenExists,
                tags );
        }

        public void AddInitializer( INamedType targetType, string template, InitializerKind kind, object? tags = null, object? args = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetType );

            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var advice = new InitializeAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                templateRef.ForInitializer( ObjectReader.GetReader( args ) ),
                kind,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            this.ExecuteAdvice<INamedType>( advice );
        }

        public void AddInitializer( IConstructor targetConstructor, string template, object? tags = null, object? args = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }
            
            var compilation = this.ValidateCompilation( targetConstructor );

            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            var advice = new InitializeAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetConstructor,
                templateRef.ForInitializer( ObjectReader.GetReader( args ) ),
                InitializerKind.BeforeInstanceConstructor,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            this.ExecuteAdvice<IConstructor>( advice );
        }

        public void Override( IConstructor targetConstructor, string template, object? args = null, object? tags = null )
        {
            throw new NotImplementedException();
        }

        public void IntroduceConstructor(
            INamedType targetType,
            string template,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? args = null,
            object? tags = null )
        {
            throw new NotImplementedException();
        }

        private static void ThrowOnErrors( DiagnosticList diagnosticList )
        {
            if ( diagnosticList.HasErrors() )
            {
                throw new DiagnosticException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }
        }

        public void AddContract(
            IParameter targetParameter,
            string template,
            ContractDirection kind = ContractDirection.Input,
            object? tags = null,
            object? args = null )
        {
            if ( kind == ContractDirection.Output && targetParameter.RefKind is not RefKind.Ref or RefKind.Out )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    UserMessageFormatter.Format(
                        $"Cannot add an output contract to the parameter '{targetParameter}' because it is neither 'ref' nor 'out'." ) );
            }

            if ( kind == ContractDirection.Input && targetParameter.RefKind is not RefKind.None or RefKind.Ref or RefKind.In )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    UserMessageFormatter.Format( $"Cannot add an input contract to the out parameter '{targetParameter}' " ) );
            }

            if ( targetParameter.IsReturnParameter && targetParameter.Type.Is( SpecialType.Void ) )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetParameter),
                    UserMessageFormatter.Format( $"Cannot add a contract to the return parameter of a void method." ) );
            }

            this.AddFilterImpl<IParameter>( targetParameter, targetParameter.DeclaringMember, template, kind, tags, args );
        }

        public IIntroductionAdviceResult<IPropertyOrIndexer> AddContract(
            IFieldOrPropertyOrIndexer targetMember,
            string template,
            ContractDirection kind = ContractDirection.Default,
            object? tags = null,
            object? args = null )
        {
            return this.AddFilterImpl<IPropertyOrIndexer>( targetMember, targetMember, template, kind, tags, args );
        }

        private AdviceResult<T> AddFilterImpl<T>(
            IDeclaration targetDeclaration,
            IMember targetMember,
            string template,
            ContractDirection direction,
            object? tags,
            object? args )
            where T : class, ICompilationElement
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            AdviceResult<T> result;

            var compilation = this.ValidateCompilation( targetDeclaration, targetMember );
            
            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( compilation, this.State.ServiceProvider );

            if ( !this.State.ContractAdvices.TryGetValue( targetMember, out var advice ) )
            {
                this.State.ContractAdvices[targetMember] = advice = new ContractAdvice(
                    this.State.AspectInstance,
                    this._templateInstance,
                    targetMember,
                    this._layerName );

                result = this.ExecuteAdvice<T>( advice );
            }
            else
            {
                result = new AdviceResult<T>(
                    advice.LastAdviceImplementationResult.AssertNotNull().NewDeclaration.As<T>(),
                    this.State.CurrentCompilation,
                    AdviceOutcome.Default );
            }

            // We keep adding contracts to the same advice instance even after it has produced a transformation because the transformation will use this list of advice.
            advice.Contracts.Add( new Contract( targetDeclaration, templateRef, direction, ObjectReader.GetReader( tags ), ObjectReader.GetReader( args ) ) );

            return result;
        }

        public void AddAttribute( IDeclaration targetDeclaration, IAttributeData attribute, OverrideStrategy whenExists = OverrideStrategy.Default )
        {
            this.ExecuteAdvice<IDeclaration>(
                new AddAttributeAdvice( this.State.AspectInstance, this._templateInstance!, targetDeclaration, attribute, whenExists, this._layerName ) );
        }

        public void RemoveAttributes( IDeclaration targetDeclaration, INamedType attributeType )
        {
            this.ExecuteAdvice<IDeclaration>(
                new RemoveAttributesAdvice( this.State.AspectInstance, this._templateInstance!, targetDeclaration, attributeType, this._layerName ) );
        }

        public void RemoveAttributes( IDeclaration targetDeclaration, Type attributeType )
            => this.RemoveAttributes( targetDeclaration, (INamedType) this.State.InitialCompilation.Factory.GetTypeByReflectionType( attributeType ) );
    }
}