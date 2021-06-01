// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
        }

        private IMethod? GetTemplateMethod(
            string? methodName,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            if ( methodName == null )
            {
                return null;
            }

            // We do the search against the Roslyn compilation because it is cheaper.

            var members = this._aspectType.GetSymbol().GetMembers( methodName ).ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( typeof(TemplateAttribute) );

            if ( members.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (this._aspectType, methodName) );
            }

            var method = members.OfType<IMethodSymbol>().Single();

            if ( !method.SelectRecursive( m => m.OverriddenMethod, includeThis: true )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => a.AttributeClass != null && StructuralSymbolComparer.Default.Equals( a.AttributeClass, expectedAttributeTypeSymbol ) ) )
            {
                if ( throwIfMissing )
                {
                    throw GeneralDiagnosticDescriptors.TemplateMemberMissesAttribute.CreateException(
                        (DeclarationKind.Method, method, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return this._compilation.Factory.GetMethod( method );
        }

        private IProperty? GetTemplateProperty(
            string propertyName,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var members = this._aspectType.GetSymbol().GetMembers( propertyName ).ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( typeof(TemplateAttribute) );

            if ( members.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (this._aspectType, propertyName) );
            }

            var property = members.OfType<IPropertySymbol>().Single();

            if ( !property.SelectRecursive( m => m.OverriddenProperty, includeThis: true )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => a.AttributeClass?.Equals( expectedAttributeTypeSymbol, SymbolEqualityComparer.Default ) ?? false ) )
            {
                if ( throwIfMissing )
                {
                    throw GeneralDiagnosticDescriptors.TemplateMemberMissesAttribute.CreateException(
                        (DeclarationKind.Property, property, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return this._compilation.Factory.GetProperty( property );
        }

        public void OverrideMethod( IMethod targetMethod, string defaultTemplate, AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();
            var templateMethod = this.GetTemplateMethod( defaultTemplate, nameof(this.OverrideMethod) );

            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod, _layerName, options );
            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );

            if ( diagnosticList.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                // Report any errors
                throw new InvalidUserCodeException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }

            this._diagnosticAdder.Report( diagnosticList );
        }

        public IMethodBuilder IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();
            var templateMethod = this.GetTemplateMethod( defaultTemplate, nameof(this.IntroduceMethod) );

            var advice = new IntroduceMethodAdvice(
                this._aspect,
                targetType,
                templateMethod,
                scope,
                conflictBehavior,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );

            if ( diagnosticList.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                throw new InvalidUserCodeException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }

            this._diagnosticAdder.Report( diagnosticList );

            return advice.Builder;
        }

        public void OverrideFieldOrProperty(
            IFieldOrProperty targetDeclaration,
            string defaultTemplate,
            AdviceOptions? options = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var templateProperty = this.GetTemplateProperty(
                defaultTemplate,
                nameof(this.OverrideFieldOrProperty) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                templateProperty,
                null,
                null,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );
        }

        public void OverrideFieldOrPropertyAccessors(
            IFieldOrProperty targetDeclaration,
            string? getTemplate,
            string? setTemplate,
            AdviceOptions? options = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var getTemplateMethod = this.GetTemplateMethod(
                getTemplate,
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var setTemplateMethod = this.GetTemplateMethod(
                setTemplate,
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                null,
                getTemplateMethod,
                setTemplateMethod,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );
        }

        public IFieldBuilder IntroduceField(
            INamedType targetType,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            throw new NotImplementedException();
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();

            var templateProperty = this.GetTemplateProperty(
                defaultTemplate,
                nameof(this.IntroduceProperty) );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                templateProperty,
                null,
                null,
                null,
                scope,
                conflictBehavior,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );

            return advice.Builder;
        }

        public IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string name,
            string defaultGetTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();

            var getTemplateMethod = this.GetTemplateMethod(
                defaultGetTemplate,
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var setTemplateMethod = this.GetTemplateMethod(
                setTemplate,
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var advice = new IntroducePropertyAdvice(
                this._aspect,
                targetType,
                null,
                name,
                getTemplateMethod,
                setTemplateMethod,
                scope,
                conflictBehavior,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );

            return advice.Builder;
        }

        public void OverrideEventAccessors(
            IEvent targetDeclaration,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate,
            AdviceOptions? options = null )
        {
            throw new NotImplementedException();
        }

        public IEventBuilder IntroduceEvent(
            INamedType targetType,
            string addTemplate,
            string removeTemplate,
            string? invokeTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            throw new NotImplementedException();
        }

        public void IntroduceInterface(
            INamedType targetType,
            INamedType interfaceType,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();

            var advice = new IntroduceInterfaceAdvice(
                this._aspect,
                targetType,
                interfaceType,
                explicitImplementation,
                null,
                conflictBehavior,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );
        }

        public void IntroduceInterface(
            INamedType targetType,
            Type interfaceType,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            this.IntroduceInterface(
                targetType,
                (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( interfaceType ),
                explicitImplementation,
                conflictBehavior,
                options );
        }

        public void IntroduceInterface(
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyDictionary<IMember, IMember> memberMap,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            var diagnosticList = new DiagnosticList();

            var advice = new IntroduceInterfaceAdvice(
                this._aspect,
                targetType,
                interfaceType,
                explicitImplementation,
                memberMap,
                conflictBehavior,
                _layerName,
                options );

            advice.Initialize( this._declarativeAdvices, diagnosticList );
            this._advices.Add( advice );
        }

        public void IntroduceInterface(
            INamedType targetType,
            Type interfaceType,
            IReadOnlyDictionary<IMember, IMember> memberMap,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AdviceOptions? options = null )
        {
            this.IntroduceInterface(
                targetType,
                (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( interfaceType ),
                memberMap,
                explicitImplementation,
                conflictBehavior,
                options );
        }

        public IAdviceFactory ForLayer( string layerName ) => throw new NotImplementedException();
    }
}