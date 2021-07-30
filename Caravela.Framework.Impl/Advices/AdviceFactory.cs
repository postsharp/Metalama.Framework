// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl.CodeModel;
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
            else if ( this._aspect.AspectClass.ImplementedTemplates.TryGetValue( templateName, out var implementationClass ) )
            {
                return new TemplateRef( templateName, implementationClass, templateKind );
            }
            else if ( this._aspect.AspectClass.AbstractTemplates.ContainsKey( templateName ) )
            {
                if ( !required )
                {
                    return default;
                }
            }

            throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMember.CreateException( (this._aspect.AspectClass.DisplayName, templateName!) );
        }

        private TemplateRef SelectTemplate( IMethod targetMethod, in MethodTemplateSelector templates )
        {
            var defaultTemplate = this.ValidateTemplateName( templates.DefaultTemplate, TemplateKind.Default, true )!;
            var asyncTemplate = this.ValidateTemplateName( templates.AsyncTemplate, TemplateKind.Async );
            var enumerableTemplate = this.ValidateTemplateName( templates.EnumerableTemplate, TemplateKind.IEnumerable );
            var enumeratorTemplate = this.ValidateTemplateName( templates.EnumeratorTemplate, TemplateKind.IEnumerator );
            var asyncEnumerableTemplate = this.ValidateTemplateName( templates.AsyncEnumerableTemplate, TemplateKind.IAsyncEnumerable );
            var asyncEnumeratorTemplate = this.ValidateTemplateName( templates.AsyncEnumeratorTemplate, TemplateKind.IAsyncEnumerator );

            var selectedTemplate = defaultTemplate;

            if ( !templates.HasOnlyDefaultTemplate )
            {
                var asyncInfo = targetMethod.GetAsyncInfoImpl();
                var iteratorInfo = targetMethod.GetIteratorInfoImpl();

                if ( !asyncTemplate.IsNull &&
                     (asyncInfo.IsAsync || (templates.UseAsyncTemplateForAnyAwaitable && asyncInfo.IsAwaitable)) )
                {
                    selectedTemplate = asyncTemplate;

                    // We don't return because the result can still be overwritten by async iterators.
                }

                if ( !enumerableTemplate.IsNull && iteratorInfo.IsIterator )
                {
                    selectedTemplate = enumerableTemplate;
                }

                if ( !enumeratorTemplate.IsNull && iteratorInfo.IteratorKind is IteratorKind.IEnumerator or IteratorKind.UntypedIEnumerator )
                {
                    return enumeratorTemplate;
                }

                if ( !asyncEnumerableTemplate.IsNull && iteratorInfo.IsIterator && iteratorInfo.IsAsync )
                {
                    return asyncEnumerableTemplate;
                }

                if ( !asyncEnumeratorTemplate.IsNull && iteratorInfo.IteratorKind == IteratorKind.IAsyncEnumerator )
                {
                    return asyncEnumeratorTemplate;
                }
            }

            return selectedTemplate;
        }

        private TemplateRef SelectTemplate( IFieldOrProperty targetFieldOrProperty, in GetterTemplateSelector templates, bool required )
        {
            var getter = targetFieldOrProperty.Getter;

            if ( getter == null )
            {
                return default;
            }

            var defaultTemplate = this.ValidateTemplateName( templates.DefaultTemplate, TemplateKind.Default, required )!;
            var enumerableTemplate = this.ValidateTemplateName( templates.EnumerableTemplate, TemplateKind.IEnumerable );
            var enumeratorTemplate = this.ValidateTemplateName( templates.EnumeratorTemplate, TemplateKind.IEnumerator );

            var selectedTemplate = defaultTemplate;

            if ( !templates.HasOnlyDefaultTemplate )
            {
                var iteratorInfo = getter.GetIteratorInfoImpl();

                if ( !enumerableTemplate.IsNull && iteratorInfo.IsIterator )
                {
                    selectedTemplate = enumerableTemplate;
                }

                if ( !enumeratorTemplate.IsNull && iteratorInfo.IteratorKind is IteratorKind.IEnumerator or IteratorKind.UntypedIEnumerator )
                {
                    return enumeratorTemplate;
                }
            }

            return selectedTemplate;
        }

        public void OverrideMethod( IMethod targetMethod, in MethodTemplateSelector templates, Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var templateMethod = this.SelectTemplate( targetMethod, templates ).GetTemplate<IMethod>( this._compilation, this._serviceProvider )!;

            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod, _layerName, tags );
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

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Introduction, true )
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

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Introduction, true )
                .GetTemplate<IProperty>( this._compilation, this._serviceProvider )!;

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
            in GetterTemplateSelector getTemplate,
            string? setTemplate,
            Dictionary<string, object?>? tags = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var getTemplateRef = this.SelectTemplate( targetDeclaration, getTemplate, setTemplate == null )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Default, getTemplate.IsNull )
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

            var template = this.ValidateTemplateName( defaultTemplate, TemplateKind.Introduction, true )
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

            var getTemplateRef = this.ValidateTemplateName( getTemplate, TemplateKind.Introduction, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var setTemplateRef = this.ValidateTemplateName( setTemplate, TemplateKind.Introduction, true )
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

            var template = this.ValidateTemplateName( eventTemplate, TemplateKind.Introduction, true )
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

            var addTemplateRef = this.ValidateTemplateName( addTemplate, TemplateKind.Introduction, true )
                .GetTemplate<IMethod>( this._compilation, this._serviceProvider );

            var removeTemplateRef = this.ValidateTemplateName( removeTemplate, TemplateKind.Introduction, true )
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