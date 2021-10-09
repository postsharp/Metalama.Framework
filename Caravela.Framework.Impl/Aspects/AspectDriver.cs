﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Executes aspects.
    /// </summary>
    internal class AspectDriver : IAspectDriver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<TemplateClassMember> _declarativeAdviceAttributes;
        private readonly ReflectionMapper _reflectionMapper;
        private readonly IAspectClassImpl _aspectClass;

        public AspectDriver( IServiceProvider serviceProvider, IAspectClassImpl aspectClass, Compilation compilation )
        {
            this._serviceProvider = serviceProvider;
            this._reflectionMapper = serviceProvider.GetService<ReflectionMapperFactory>().GetInstance( compilation );
            this._aspectClass = aspectClass;

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
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectProjectConfiguration projectConfiguration,
            CancellationToken cancellationToken )
            => aspectInstance.TargetDeclaration switch
            {
                ICompilation compilation => this.EvaluateAspect(
                    compilation,
                    aspectInstance,
                    compilationModelRevision,
                    projectConfiguration,
                    cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect(
                    constructor,
                    aspectInstance,
                    compilationModelRevision,
                    projectConfiguration,
                    cancellationToken ),
                IParameter parameter => this.EvaluateAspect( parameter, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                IGenericParameter genericParameter => this.EvaluateAspect(
                    genericParameter,
                    aspectInstance,
                    compilationModelRevision,
                    projectConfiguration,
                    cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                INamespace ns => this.EvaluateAspect( ns, aspectInstance, compilationModelRevision, projectConfiguration, cancellationToken ),
                _ => throw new NotSupportedException( $"Cannot add an aspect to a declaration of type {aspectInstance.TargetDeclaration.DeclarationKind}." )
            };

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectProjectConfiguration projectConfiguration,
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
                var interfaceType = this._reflectionMapper.GetTypeSymbol( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectDeclaration.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (AspectType: this._aspectClass.DisplayName, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( this._aspectClass.Project, targetDeclaration );

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

                var aspectBuilder = new AspectBuilder<T>(
                    targetDeclaration,
                    diagnosticSink,
                    declarativeAdvices,
                    adviceFactory,
                    projectConfiguration,
                    aspectInstance,
                    cancellationToken );

                try
                {
                    this._serviceProvider.GetService<UserCodeInvoker>().Invoke( () => aspectOfT.BuildAspect( aspectBuilder ) );
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
                        (AspectType: this._aspectClass.DisplayName, MethodName: nameof(IAspect<T>.BuildAspect), e.GetType().Name, e.Format( 5 )) );

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
            IAspectInstanceInternal aspect,
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