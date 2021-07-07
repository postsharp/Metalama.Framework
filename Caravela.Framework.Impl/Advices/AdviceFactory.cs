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
        private readonly INamedType _aspectType;
        private readonly AspectInstance _aspect;
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly IReadOnlyList<Advice> _declarativeAdvices;
        private readonly List<Advice> _advices = new();

        private readonly Dictionary<INamedType, ImplementInterfaceAdvice> _implementInterfaceAdvices;

        internal IReadOnlyList<Advice> Advices => this._advices;

        public Dictionary<string, object?> Tags { get; } = new( StringComparer.Ordinal );

        public AdviceFactory(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            IReadOnlyList<Advice> declarativeAdvices,
            INamedType aspectType,
            AspectInstance aspect )
        {
            this._aspectType = aspectType;
            this._aspect = aspect;
            this._compilation = compilation;
            this._diagnosticAdder = diagnosticAdder;
            this._declarativeAdvices = declarativeAdvices;
            this._implementInterfaceAdvices = new Dictionary<INamedType, ImplementInterfaceAdvice>( compilation.InvariantComparer );
        }

        public void OverrideMethod( IMethod targetMethod, string defaultTemplate, Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();
            var templateMethod = this._aspectType.GetTemplateMethod( this._compilation, defaultTemplate, nameof(this.OverrideMethod) );

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
            var templateMethod = this._aspectType.GetTemplateMethod( this._compilation, defaultTemplate, nameof(this.IntroduceMethod) );

            var advice = new IntroduceMethodAdvice(
                this._aspect,
                targetType,
                templateMethod,
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
            var templateProperty = this._aspectType.GetTemplateProperty( this._compilation, defaultTemplate, nameof(this.OverrideFieldOrProperty) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                templateProperty,
                null,
                null,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public void OverrideFieldOrPropertyAccessors(
            IFieldOrProperty targetDeclaration,
            string? getTemplate,
            string? setTemplate,
            Dictionary<string, object?>? tags = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();
            var getTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, getTemplate, nameof(this.OverrideFieldOrPropertyAccessors) );
            var setTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, setTemplate, nameof(this.OverrideFieldOrPropertyAccessors) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                null,
                getTemplateMethod,
                setTemplateMethod,
                _layerName,
                tags );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            ThrowOnErrors( diagnosticList );
            this._advices.Add( advice );

            this._diagnosticAdder.Report( diagnosticList );
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            throw new NotImplementedException();
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();

            var templateProperty = this._aspectType.GetTemplateProperty( this._compilation, defaultTemplate, nameof(this.IntroduceProperty) );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                null,
                templateProperty,
                null,
                null,
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
            string? defaultGetTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            Dictionary<string, object?>? tags = null )
        {
            var diagnosticList = new DiagnosticList();
            var getTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, defaultGetTemplate, nameof(this.OverrideFieldOrPropertyAccessors) );
            var setTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, setTemplate, nameof(this.OverrideFieldOrPropertyAccessors) );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                name,
                null,
                getTemplateMethod,
                setTemplateMethod,
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
            var addTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, addTemplate, nameof(this.OverrideEventAccessors) );
            var removeTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, removeTemplate, nameof(this.OverrideEventAccessors) );

            var advice = new OverrideEventAdvice(
                this._aspect,
                targetDeclaration,
                null,
                addTemplateMethod,
                removeTemplateMethod,
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
            var templateEvent = this._aspectType.GetTemplateEvent( this._compilation, eventTemplate, nameof(this.IntroduceProperty) );

            var advice = new IntroduceEventAdvice(
                this._aspect,
                targetType,
                null,
                templateEvent,
                null,
                null,
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
            var addTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, addTemplate, nameof(this.OverrideEventAccessors) );
            var removeTemplateMethod = this._aspectType.GetTemplateMethod( this._compilation, removeTemplate, nameof(this.OverrideEventAccessors) );

            var advice = new IntroduceEventAdvice(
                this._aspect,
                targetType,
                name,
                null,
                addTemplateMethod,
                removeTemplateMethod,
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