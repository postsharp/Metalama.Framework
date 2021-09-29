// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    // TODO: AspectDriver should not store a reference to a Compilation we should not store references to a Roslyn compilation.

    internal interface IHighLevelAspectDriver : IAspectDriver
    {
        AspectInstanceResult ExecuteAspect(
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken );
    }
    
    /// <summary>
    /// Executes aspects.
    /// </summary>
    internal class AspectDriver : IHighLevelAspectDriver
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly IServiceProvider _serviceProvider;
        private readonly Compilation _compilation;
        private readonly List<TemplateClassMember> _declarativeAdviceAttributes;

        public IAspectClassImpl AspectClass { get; }

        public AspectDriver( IServiceProvider serviceProvider, IAspectClassImpl aspectClass, Compilation compilation )
        {
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this._serviceProvider = serviceProvider;
            this._compilation = compilation;
            this.AspectClass = aspectClass;

            // Introductions must have a deterministic order because of testing.
            this._declarativeAdviceAttributes = aspectClass
                .TemplateClasses.SelectMany( c => c.Members )
                .Where( m => m.Value.TemplateInfo.AttributeType == TemplateAttributeType.Introduction )
                .Select( m => m.Value )
                .OrderBy( m => m.Symbol.GetPrimarySyntaxReference()?.SyntaxTree.FilePath )
                .ThenBy( m => m.Symbol.GetPrimarySyntaxReference()?.Span.Start )
                .ToList();
        }

        public AspectInstanceResult ExecuteAspect(
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken )
            => aspectInstance.TargetDeclaration switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspectInstance, compilationModelRevision, cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, compilationModelRevision, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, compilationModelRevision, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, compilationModelRevision, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, compilationModelRevision, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect( constructor, aspectInstance, compilationModelRevision, cancellationToken ),
                IParameter parameter => this.EvaluateAspect( parameter, aspectInstance, compilationModelRevision, cancellationToken ),
                IGenericParameter genericParameter => this.EvaluateAspect( genericParameter, aspectInstance, compilationModelRevision, cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, compilationModelRevision, cancellationToken ),
                _ => throw new NotImplementedException()
            };

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken )
            where T : class, IDeclaration
        {
            static AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
            {
                return new AspectInstanceResult(
                    false,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty );
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Map the target declaration to the correct revision of the compilation model.
            targetDeclaration = compilationModelRevision.Factory.GetDeclaration( targetDeclaration );

            if ( aspectInstance.Aspect is not IAspect<T> aspectOfT )
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?

                // Get the code model type for the reflection type so we have better formatting of the diagnostic.
                var interfaceType = this._compilation.GetTypeByReflectionType( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectDeclaration.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (AspectType: this.AspectClass.DisplayName, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( this.AspectClass.Project, targetDeclaration );

            using ( DiagnosticContext.WithDefaultLocation( diagnosticSink.DefaultScope?.DiagnosticLocation ) )
            {
                var declarativeAdvices =
                    this._declarativeAdviceAttributes
                        .Select(
                            x => CreateDeclarativeAdvice(
                                aspectInstance,
                                aspectInstance.TemplateInstances[x.TemplateClass],
                                diagnosticSink,
                                targetDeclaration,
                                x.TemplateInfo,
                                x.Symbol ) )
                        .WhereNotNull()
                        .ToArray();

                var adviceFactory = new AdviceFactory(
                    compilationModelRevision,
                    diagnosticSink,
                    declarativeAdvices,
                    aspectInstance,
                    aspectInstance.TemplateInstances.Count == 1 ? aspectInstance.TemplateInstances.Values.Single() : null,
                    this._serviceProvider );

                var aspectBuilder = new AspectBuilder<T>( targetDeclaration, diagnosticSink, declarativeAdvices, adviceFactory, cancellationToken );

                try
                {
                    this._userCodeInvoker.Invoke( () => aspectOfT.BuildAspect( aspectBuilder ) );
                }
                catch ( InvalidUserCodeException e )
                {
                    aspectInstance.Skip();

                    return
                        new AspectInstanceResult(
                            false,
                            new ImmutableUserDiagnosticList( e.Diagnostics, ImmutableArray<ScopedSuppression>.Empty ),
                            ImmutableArray<Advice>.Empty,
                            aspectBuilder.AspectSources.ToImmutableArray() );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (AspectType: this.AspectClass.DisplayName, MethodName: nameof(IAspect<T>.BuildAspect), e.GetType().Name, e.Format( 5 )) );

                    return CreateResultForError( diagnostic );
                }

                var aspectResult = aspectBuilder.ToResult();

                if ( !aspectResult.Success )
                {
                    aspectInstance.Skip();
                }

                return aspectResult;
            }
        }

        private static Advice? CreateDeclarativeAdvice<T>(
            AspectInstance aspect,
            TemplateClassInstance templateInstance,
            IDiagnosticAdder diagnosticAdder,
            T aspectTarget,
            TemplateInfo template,
            ISymbol templateDeclaration )
            where T : IDeclaration
        {
            template.TryCreateAdvice(
                aspect,
                templateInstance,
                diagnosticAdder,
                aspectTarget,
                ((CompilationModel) aspectTarget.Compilation).Factory.GetDeclaration( templateDeclaration ),
                null,
                out var advice );

            return advice;
        }
    }
}