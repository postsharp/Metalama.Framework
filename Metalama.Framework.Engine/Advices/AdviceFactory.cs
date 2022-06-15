// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
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

        public IAdviceFactory ForLayer( string? layerName )
        {
            if ( layerName == this._layerName )
            {
                return this;
            }

            if ( !this.State.AspectInstance.AspectClass.Layers.Any( l => l.LayerName == layerName ) )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(layerName),
                    $"The aspect '{this.State.AspectInstance.AspectClass.ShortName}' does not contain an aspect layer '{layerName}'." );
            }

            return new AdviceFactory( this.State, this._templateInstance, this._layerName );
        }

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
                    (this.State.AspectInstance.AspectClass.ShortName, templateName) );
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

            var diagnosticList = new DiagnosticList();

            var template = this.SelectTemplate( targetMethod, templateSelector )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider )
                .ForOverride( targetMethod, ObjectReader.GetReader( args ) );

            var advice = new OverrideMethodAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetMethod,
                template,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public void Override( IFinalizer targetFinalizer, string template, object? args = null, object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var diagnosticList = new DiagnosticList();

            var templateMember = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider )
                .ForOverride( targetFinalizer, ObjectReader.GetReader( args ) );

            var advice = new OverrideFinalizerAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetFinalizer,
                templateMember,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public IMethodBuilder IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
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

            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var advice = new IntroduceMethodAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                template.ForIntroduction( ObjectReader.GetReader( args ) ),
                scope,
                whenExists,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
        }

        public void Override(
            IFieldOrPropertyOrIndexer targetDeclaration,
            string defaultTemplate,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetDeclaration.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an OverrideFieldOrProperty advice to '{targetDeclaration}' because it is an abstract." ) );
            }

            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var propertyTemplate = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IProperty>( this.State.Compilation, this.State.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();
            var getTemplate = accessorTemplates.Get;
            var setTemplate = accessorTemplates.Set;

            var advice = new OverrideFieldOrPropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetDeclaration,
                propertyTemplate,
                getTemplate.ForIntroduction(),
                setTemplate.ForIntroduction(),
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public void OverrideAccessors(
            IFieldOrPropertyOrIndexer targetDeclaration,
            in GetterTemplateSelector getTemplateSelector,
            string? setTemplate = null,
            object? args = null,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            if ( targetDeclaration.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format(
                        $"Cannot add an OverrideFieldOrPropertyAccessors advice to '{targetDeclaration}' because it is an abstract." ) );
            }

            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var getTemplateRef = this.SelectTemplate( targetDeclaration, getTemplateSelector, setTemplate == null )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider )
                .ForOverride( targetDeclaration.GetMethod, ObjectReader.GetReader( args ) );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, getTemplateSelector.IsNull )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider )
                .ForOverride( targetDeclaration.SetMethod, ObjectReader.GetReader( args ) );

            if ( getTemplateRef.IsNull && setTemplateRef.IsNull )
            {
                // There is no applicable template because the property has no getter or no setter matching the selection.
                return;
            }

            var advice = new OverrideFieldOrPropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetDeclaration,
                default,
                getTemplateRef,
                setTemplateRef,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            string templateName,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( templateName, TemplateKind.Default, true )
                .GetTemplateMember<IField>( this.State.Compilation, this.State.ServiceProvider );

            var advice = new IntroduceFieldAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                null,
                template,
                scope,
                whenExists,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            string templateName,
            IType fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var diagnosticList = new DiagnosticList();

            var advice = new IntroduceFieldAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                templateName,
                default,
                scope,
                whenExists,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            advice.Builder.Type = fieldType;

            return advice.Builder;
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            string templateName,
            Type fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null )
            => this.IntroduceField( targetType, templateName, this.State.Compilation.Factory.GetTypeByReflectionType( fieldType ), scope, whenExists, tags );

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
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

            var diagnosticList = new DiagnosticList();

            var propertyTemplate = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IProperty>( this.State.Compilation, this.State.ServiceProvider );

            var accessorTemplates = propertyTemplate.GetAccessorTemplates();

            var advice = new IntroducePropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                null,
                propertyTemplate,
                accessorTemplates.Get.ForIntroduction(),
                accessorTemplates.Set.ForIntroduction(),
                scope,
                whenExists,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string name,
            string? getTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
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

            var diagnosticList = new DiagnosticList();

            var getTemplateRef = this.ValidateTemplateName( getTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var parameterReaders = ObjectReader.GetReader( args );

            var advice = new IntroducePropertyAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                name,
                default,
                getTemplateRef.ForIntroduction( parameterReaders ),
                setTemplateRef.ForIntroduction( parameterReaders ),
                scope,
                whenExists,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
        }

        public void OverrideAccessors(
            IEvent targetDeclaration,
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

            if ( targetDeclaration.IsAbstract )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot add an OverrideEventAccessors advice to '{targetDeclaration}' because it is an abstract." ) );
            }

            var diagnosticList = new DiagnosticList();

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            if ( invokeTemplate != null )
            {
                throw new NotImplementedException( "Support for overriding event raisers is not yet implemented." );
            }

            var advice = new OverrideEventAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetDeclaration,
                default,
                addTemplateRef,
                removeTemplateRef,
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.GetReader( args ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public IEventBuilder IntroduceEvent(
            INamedType targetType,
            string eventTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
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

            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( eventTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IEvent>( this.State.Compilation, this.State.ServiceProvider );

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
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.Empty );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
        }

        public IEventBuilder IntroduceEvent(
            INamedType targetType,
            string name,
            string addTemplate,
            string removeTemplate,
            string? invokeTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
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

            var diagnosticList = new DiagnosticList();

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

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
                this._layerName,
                ObjectReader.GetReader( tags ),
                ObjectReader.GetReader( args ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );

            return advice.Builder;
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

            var diagnosticList = new DiagnosticList();

            var advice = new ImplementInterfaceAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                interfaceType,
                whenExists,
                null,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );
            this.State.Diagnostics.Report( diagnosticList );
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

            var diagnosticList = new DiagnosticList();

            var advice = new ImplementInterfaceAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                interfaceType,
                whenExists,
                interfaceMemberSpecifications,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
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

            var diagnosticList = new DiagnosticList();

            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var advice = new InitializeAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetType,
                templateRef.ForInitializer( ObjectReader.GetReader( args ) ),
                kind,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
        }

        public void AddInitializer( IConstructor targetConstructor, string template, object? tags = null, object? args = null )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var diagnosticList = new DiagnosticList();

            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            var advice = new InitializeAdvice(
                this.State.AspectInstance,
                this._templateInstance,
                targetConstructor,
                templateRef.ForInitializer( ObjectReader.GetReader( args ) ),
                InitializerKind.BeforeInstanceConstructor,
                this._layerName,
                ObjectReader.GetReader( tags ) );

            advice.Initialize( diagnosticList );
            ThrowOnErrors( diagnosticList );
            this.State.Advices.Add( advice );

            this.State.Diagnostics.Report( diagnosticList );
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

            this.AddFilterImpl( targetParameter, targetParameter.DeclaringMember, template, kind, tags, args );
        }

        public void AddContract(
            IFieldOrPropertyOrIndexer targetMember,
            string template,
            ContractDirection kind = ContractDirection.Input,
            object? tags = null,
            object? args = null )
        {
            this.AddFilterImpl( targetMember, targetMember, template, kind, tags, args );
        }

        public IParameterBuilder IntroduceParameterAndPull(
            IConstructor targetConstructor,
            string parameterName,
            IType parameterType,
            IExpression? defaultValue = null )
            => throw new NotImplementedException();

        public IParameterBuilder IntroduceParameterAndPull(
            IConstructor targetConstructor,
            string parameterName,
            Type parameterType,
            IExpression? defaultValue = null )
            => throw new NotImplementedException();

        private void AddFilterImpl(
            IDeclaration targetDeclaration,
            IMember targetMember,
            string template,
            ContractDirection direction,
            object? tags,
            object? args )
        {
            if ( this._templateInstance == null )
            {
                throw new InvalidOperationException();
            }

            var diagnosticList = new DiagnosticList();

            var templateRef = this.ValidateTemplateName( template, TemplateKind.Default, true )
                .GetTemplateMember<IMethod>( this.State.Compilation, this.State.ServiceProvider );

            if ( !this.State.ContractAdvices.TryGetValue( targetMember, out var advice ) )
            {
                this.State.ContractAdvices[targetMember] = advice = new ContractAdvice(
                    this.State.AspectInstance,
                    this._templateInstance,
                    targetMember,
                    this._layerName );

                advice.Initialize( diagnosticList );
                ThrowOnErrors( diagnosticList );
                this.State.Advices.Add( advice );

                this.State.Diagnostics.Report( diagnosticList );
            }

            advice.Contracts.Add( new Contract( targetDeclaration, templateRef, direction, ObjectReader.GetReader( tags ), ObjectReader.GetReader( args ) ) );
        }
    }
}