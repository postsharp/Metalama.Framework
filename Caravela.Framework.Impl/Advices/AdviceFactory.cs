// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
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
        private readonly CompilationModel _compilation;
        private readonly INamedType _aspectType;
        private readonly AspectInstance _aspect;
        private readonly IDiagnosticAdder _diagnosticAdder;

        private readonly List<IAdvice> _advices = new();

        internal IReadOnlyList<IAdvice> Advices => this._advices;

        public Dictionary<string, object?> Tags { get; } = new( StringComparer.Ordinal );

        public AdviceFactory( CompilationModel compilation, IDiagnosticAdder diagnosticAdder, INamedType aspectType, AspectInstance aspect )
        {
            this._aspectType = aspectType;
            this._aspect = aspect;
            this._compilation = compilation;
            this._diagnosticAdder = diagnosticAdder;
        }

        private IMethod? GetTemplateMethod(
            string methodName,
            Type expectedAttributeType,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var members = this._aspectType.GetSymbol().GetMembers( methodName ).ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( expectedAttributeType );

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
                        (CodeElementKind.Method, method, expectedAttributeTypeSymbol, adviceName) );
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
            Type expectedAttributeType,
            string adviceName,
            [DoesNotReturnIf( true )] bool throwIfMissing = true )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var members = this._aspectType.GetSymbol().GetMembers( propertyName ).ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( expectedAttributeType );

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
                        (CodeElementKind.Property, property, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return this._compilation.Factory.GetProperty( property );
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var diagnosticList = new DiagnosticList();
            var templateMethod = this.GetTemplateMethod( defaultTemplate, typeof(OverrideMethodTemplateAttribute), nameof(this.OverrideMethod) );

            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod, this.Tags.ToImmutableDictionary(), aspectLinkerOptions );
            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            if ( diagnosticList.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                // Report any errors
                throw new InvalidUserCodeException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }

            this._diagnosticAdder.Report( diagnosticList );

            return advice;
        }

        public IIntroduceMethodAdvice IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var diagnosticList = new DiagnosticList();
            var templateMethod = this.GetTemplateMethod( defaultTemplate, typeof(IntroduceMethodTemplateAttribute), nameof(this.IntroduceMethod) );

            var advice = new IntroduceMethodAdvice(
                this._aspect,
                targetType,
                templateMethod,
                scope,
                conflictBehavior,
                aspectLinkerOptions,
                this.Tags.ToImmutableDictionary() );

            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            if ( diagnosticList.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                throw new InvalidUserCodeException(
                    "Errors have occured while creating advice.",
                    diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
            }

            this._diagnosticAdder.Report( diagnosticList );

            return advice;
        }

        public IOverrideFieldOrPropertyAdvice OverrideFieldOrProperty(
            IFieldOrProperty targetDeclaration,
            string defaultTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var templateProperty = this.GetTemplateProperty(
                defaultTemplate,
                typeof(OverrideFieldOrPropertyTemplateAttribute),
                nameof(this.OverrideFieldOrProperty) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                templateProperty,
                null,
                null,
                this.Tags.ToImmutableDictionary(),
                aspectLinkerOptions );

            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            return advice;
        }

        public IOverrideFieldOrPropertyAdvice OverrideFieldOrPropertyAccessors(
            IFieldOrProperty targetDeclaration,
            string? defaultGetTemplate,
            string? setTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            // Set template represents both set and init accessors.
            var diagnosticList = new DiagnosticList();

            var getTemplateMethod = this.GetTemplateMethod(
                defaultGetTemplate,
                typeof(OverrideFieldOrPropertyGetTemplateAttribute),
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var setTemplateMethod = this.GetTemplateMethod(
                setTemplate,
                typeof(OverrideFieldOrPropertySetTemplateAttribute),
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var advice = new OverrideFieldOrPropertyAdvice(
                this._aspect,
                targetDeclaration,
                null,
                getTemplateMethod,
                setTemplateMethod,
                this.Tags.ToImmutableDictionary(),
                aspectLinkerOptions );

            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroduceFieldAdvice IntroduceField(
            INamedType targetType,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IIntroducePropertyAdvice IntroduceProperty(
            INamedType targetType,
            string? defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var diagnosticList = new DiagnosticList();

            var templateProperty = this.GetTemplateProperty(
                defaultTemplate,
                typeof(IntroducePropertyTemplateAttribute),
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
                this.Tags.ToImmutableDictionary(),
                aspectLinkerOptions );

            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroducePropertyAdvice IntroduceProperty(
            INamedType targetType,
            string name,
            string? defaultGetTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var diagnosticList = new DiagnosticList();

            var getTemplateMethod = this.GetTemplateMethod(
                defaultGetTemplate,
                typeof(IntroducePropertyGetTemplateAttribute),
                nameof(this.OverrideFieldOrPropertyAccessors) );

            var setTemplateMethod = this.GetTemplateMethod(
                setTemplate,
                typeof(IntroducePropertySetTemplateAttribute),
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
                this.Tags.ToImmutableDictionary(),
                aspectLinkerOptions );

            advice.Initialize( diagnosticList );
            this._advices.Add( advice );

            return advice;
        }

        public IOverrideEventAdvice OverrideEventAccessors(
            IEvent targetDeclaration,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IIntroducePropertyAdvice IntroduceEvent(
            INamedType targetType,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IAdviceFactory ForLayer( string layerName ) => throw new NotImplementedException();
    }
}