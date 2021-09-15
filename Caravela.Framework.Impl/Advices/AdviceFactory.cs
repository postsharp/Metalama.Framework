// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal class AdviceFactory : IAdviceFactory
    {
        private const string? _layerName = null;

        private readonly CompilationModel _compilation;
        private readonly AspectInstance _aspect;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly IReadOnlyList<Advice> _declarativeAdvices;
        private readonly List<Advice> _advices = new();

        private readonly Dictionary<INamedType, ImplementInterfaceAdvice> _implementInterfaceAdvices;

        internal IReadOnlyList<Advice> Advices => this._advices;

        public AdviceFactory(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            IReadOnlyList<Advice> declarativeAdvices,
            AspectInstance aspect,
            IServiceProvider serviceProvider )
        {
            this._aspect = aspect;
            this._serviceProvider = serviceProvider;
            this._compilation = compilation;
            this._diagnosticAdder = diagnosticAdder;
            this._declarativeAdvices = declarativeAdvices;
            this._implementInterfaceAdvices = new Dictionary<INamedType, ImplementInterfaceAdvice>( compilation.InvariantComparer );
        }

        private TemplateRef ValidateTemplateName( string? templateName, TemplateKind templateKind, bool required = false )
        {
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
            else if ( this._aspect.AspectClass.Members.TryGetValue( templateName, out var template ) )
            {
                if ( template.TemplateInfo.IsNone )
                {
                    // It is possible that the aspect has a member of the required name, but the user did not use the custom attribute. In this case,
                    // we want a proper error message.

                    throw GeneralDiagnosticDescriptors.MemberDoesNotHaveTemplateAttribute.CreateException(
                        (template.AspectClass.FullName, templateName,
                         templateKind == TemplateKind.Introduction ? nameof(IntroduceAttribute) : nameof(TemplateAttribute)) );
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

                var expectedTemplateType = templateKind == TemplateKind.Introduction
                    ? TemplateAttributeType.Introduction
                    : TemplateAttributeType.Template;

                if ( expectedTemplateType != template.TemplateInfo.AttributeType )
                {
                    var expectedAttribute = templateKind == TemplateKind.Introduction
                        ? nameof(IntroduceAttribute)
                        : nameof(TemplateAttribute);

                    var actualAttribute = template.TemplateInfo.AttributeType == TemplateAttributeType.Introduction
                        ? nameof(IntroduceAttribute)
                        : nameof(TemplateAttribute);

                    throw GeneralDiagnosticDescriptors.TemplateIsOfTheWrongType.CreateException(
                        (template.AspectClass.FullName, templateName, expectedAttribute, actualAttribute) );
                }

                return new TemplateRef( template, templateKind );
            }
            else
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMember.CreateException(
                    (this._aspect.AspectClass.DisplayName, templateName) );
            }
        }

        private TemplateRef SelectTemplate( IMethod targetMethod, in MethodTemplateSelector templateSelector )
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

        private TemplateRef SelectTemplate( IFieldOrProperty targetFieldOrProperty, in GetterTemplateSelector templateSelector, bool required )
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

        public void OverrideMethod( IMethod targetMethod, in MethodTemplateSelector templateSelector, Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var template = this.SelectTemplate( targetMethod, templateSelector )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, template, _layerName, tags );
            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public IMethodBuilder IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var advice = new IntroduceMethodAdvice(
                this._aspect,
                targetType,
                template,
                scope,
                whenExists,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public void OverrideFieldOrProperty(
            IFieldOrProperty targetDeclaration,
            string defaultTemplate,
            Dictionary<string, object?>? tags = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplate<IProperty>( this._compilation, this._serviceProvider );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                template,
                default,
                default,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public void OverrideFieldOrPropertyAccessors(
            IFieldOrProperty targetDeclaration,
            in GetterTemplateSelector getTemplateSelector,
            string? setTemplate,
            Dictionary<string, object?>? tags = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var getTemplateRef = this.SelectTemplate( targetDeclaration, getTemplateSelector, setTemplate == null )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, getTemplateSelector.IsNull )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            if ( getTemplateRef.IsNull && setTemplateRef.IsNull )
            {
                // There is no applicable template because the property has no getter or no setter matching the selection.
                return;
            }

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                default,
                getTemplateRef,
                setTemplateRef,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            string name,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default )
        {
            var diagnosticList = new DiagnosticList();

            var advice = new IntroduceFieldAdvice(
                this._aspect,
                targetType,
                name,
                default,
                scope,
                whenExists,
                _layerName );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Default, true )
                .GetTemplate<IProperty>( this._compilation, this._serviceProvider );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                null,
                template,
                default,
                default,
                scope,
                whenExists,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string name,
            string? getTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var getTemplateRef = this.ValidateTemplateName( getTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                name,
                default,
                getTemplateRef,
                setTemplateRef,
                scope,
                whenExists,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public void OverrideEventAccessors(
            IEvent targetDeclaration,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate,
            Dictionary<string, object?>? tags = null )
        {
            if ( invokeTemplate != null )
            {
                throw GeneralDiagnosticDescriptors.UnsupportedFeature.CreateException( $"Invoker overrides." );
            }

            var diagnosticList = new DiagnosticList();

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            if ( invokeTemplate != null )
            {
                throw new NotImplementedException( "Support for overriding event raisers is not yet implemented." );
            }

            var advice = new OverrideEventAdvice(
                this._aspect,
                targetDeclaration,
                default,
                addTemplateRef,
                removeTemplateRef,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public IEventBuilder IntroduceEvent(
            INamedType targetType,
            string eventTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var template = this.ValidateTemplateName( eventTemplate, TemplateKind.Default, true )
                .GetTemplate<IEvent>( this._compilation, this._serviceProvider );

            var advice = new IntroduceEventAdvice(
                this._aspect,
                targetType,
                null,
                template,
                default,
                default,
                scope,
                whenExists,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

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
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Default, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var advice = new IntroduceEventAdvice(
                this._aspect,
                targetType,
                name,
                default,
                addTemplateRef,
                removeTemplateRef,
                scope,
                whenExists,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            if ( !this._implementInterfaceAdvices.TryGetValue( targetType, out var advice ) )
            {
                this._implementInterfaceAdvices[targetType] = advice = new ImplementInterfaceAdvice( this._aspect, targetType, _layerName );
                advice.Initialize( this._declarativeAdvices, diagnosticList );
                this._advices.Add( advice );
            }

            advice.AddInterfaceImplementation( interfaceType, whenExists, null, diagnosticList, tags );
            ThrowOnErrors( diagnosticList );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            this.ImplementInterface(
                targetType,
                (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( interfaceType ),
                whenExists,
                tags );
        }

        public void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            if ( !this._implementInterfaceAdvices.TryGetValue( targetType, out var advice ) )
            {
                this._implementInterfaceAdvices[targetType] = advice = new ImplementInterfaceAdvice( this._aspect, targetType, _layerName );
                advice.Initialize( this._declarativeAdvices, diagnosticList );
                this._advices.Add( advice );
            }

            advice.AddInterfaceImplementation( interfaceType, whenExists, null, diagnosticList, tags );
            ThrowOnErrors( diagnosticList );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            this.ImplementInterface(
                targetType,
                (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( interfaceType ),
                interfaceMemberSpecifications,
                whenExists,
                tags );
        }

        private static void ThrowOnErrors( DiagnosticList diagnosticList )
        {
            if ( diagnosticList.HasErrors() )
            {
                throw new InvalidUserCodeException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }
        }
    }
}