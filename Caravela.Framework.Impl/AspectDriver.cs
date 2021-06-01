﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Executes aspects.
    /// </summary>
    internal class AspectDriver : IAspectDriver
    {
        private readonly Compilation _compilation;
        private readonly List<(AttributeData Attribute, ISymbol Member)> _declarativeAdviceAttributes;

        public INamedTypeSymbol AspectType { get; }

        public AspectDriver( INamedTypeSymbol aspectType, Compilation compilation )
        {
            this._compilation = compilation;
            this.AspectType = aspectType;

            this._declarativeAdviceAttributes =
                (from member in aspectType.GetMembers()
                 from attribute in member.GetAttributes()
                 where attribute.AttributeClass?.Is( typeof(AdviceAttribute) ) ?? false
                 select (attribute, member)).ToList();
        }

        internal AspectInstanceResult ExecuteAspect( AspectInstance aspectInstance, CancellationToken cancellationToken )
        {
            return aspectInstance.TargetDeclaration switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspectInstance, cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect( constructor, aspectInstance, cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, cancellationToken ),
                _ => throw new NotImplementedException()
            };
        }

        private AspectInstanceResult EvaluateAspect<T>( T targetDeclaration, AspectInstance aspect, CancellationToken cancellationToken )
            where T : class, IDeclaration
        {
            static AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
                => new(
                    false,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty );

            cancellationToken.ThrowIfCancellationRequested();

            if ( aspect.Aspect is not IAspect<T> aspectOfT )
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?

                // Get the code model type for the reflection type so we have better formatting of the diagnostic.
                var interfaceType = this._compilation.GetTypeByReflectionType( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectDeclaration.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectType, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( aspect.AspectClass.Project, targetDeclaration );

            using ( DiagnosticContext.WithDefaultLocation( diagnosticSink.DefaultScope?.DiagnosticLocation ) )
            {
                var declarativeAdvices =
                    this._declarativeAdviceAttributes.Select( x => CreateDeclarativeAdvice( aspect, diagnosticSink, targetDeclaration, x.Attribute, x.Member ) )
                        .ToArray();

                var compilationModel = (CompilationModel) targetDeclaration.Compilation;

                var adviceFactory = new AdviceFactory(
                    compilationModel,
                    diagnosticSink,
                    declarativeAdvices,
                    compilationModel.Factory.GetNamedType( this.AspectType ),
                    aspect );

                var aspectBuilder = new AspectBuilder<T>( targetDeclaration, diagnosticSink, declarativeAdvices, adviceFactory, cancellationToken );

                try
                {
                    aspectOfT.BuildAspect( aspectBuilder );
                }
                catch ( InvalidUserCodeException e )
                {
                    return
                        new AspectInstanceResult(
                            false,
                            new ImmutableUserDiagnosticList( e.Diagnostics, ImmutableArray<ScopedSuppression>.Empty ),
                            ImmutableArray<Advice>.Empty,
                            ImmutableArray<IAspectSource>.Empty );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectType, e.GetType().Name, e) );

                    return CreateResultForError( diagnostic );
                }

                return aspectBuilder.ToResult();
            }
        }

        private static Advice CreateDeclarativeAdvice<T>(
            AspectInstance aspect,
            IDiagnosticAdder diagnosticAdder,
            T aspectTarget,
            AttributeData templateAttributeData,
            ISymbol templateDeclaration )
            where T : IDeclaration
            => templateAttributeData.CreateAdvice(
                aspect,
                diagnosticAdder,
                aspectTarget,
                ((CompilationModel) aspectTarget.Compilation).Factory.GetDeclaration( templateDeclaration ),
                null );
    }
}