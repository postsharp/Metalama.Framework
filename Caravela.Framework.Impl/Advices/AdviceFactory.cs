﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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

        private IMethod GetTemplateMethod( string methodName, Type expectedAttributeType, string adviceName )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var methods = this._aspectType.GetSymbol().GetMembers( methodName ).OfType<IMethodSymbol>().ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( expectedAttributeType );

            if ( methods.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (this._aspectType, methodName) );
            }

            var method = methods.Single();

            if ( !method.SelectRecursive( m => m.OverriddenMethod, includeThis: true )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => StructuralSymbolComparer.Default.Equals( a.AttributeClass,  expectedAttributeTypeSymbol  )  ) )
            {
                throw GeneralDiagnosticDescriptors.TemplateMethodMissesAttribute.CreateException( (method, expectedAttributeTypeSymbol, adviceName) );
            }

            return this._compilation.Factory.GetMethod( method );
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

            this._diagnosticAdder.ReportDiagnostics( diagnosticList );

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

            this._diagnosticAdder.ReportDiagnostics( diagnosticList );

            return advice;
        }

        public IAdviceFactory ForLayer( string layerName ) => throw new NotImplementedException();
    }
}