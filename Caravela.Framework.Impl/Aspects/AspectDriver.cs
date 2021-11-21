// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Eligibility.Implementation;
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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Executes aspects.
    /// </summary>
    internal class AspectDriver : IAspectDriver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ImmutableArray<TemplateClassMember> _declarativeAdviceAttributes;
        private readonly ReflectionMapper _reflectionMapper;
        private readonly IAspectClassImpl _aspectClass;

        public IEligibilityRule<IDeclaration>? EligibilityRule { get; }

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
                .ToImmutableArray();

            // If we have any declarative introduction, the aspect cannot be added to an interface.
            if ( this._declarativeAdviceAttributes.Any( a => a.TemplateInfo.AttributeType is TemplateAttributeType.Introduction ) )
            {
                this.EligibilityRule = new EligibilityRule<IDeclaration>(
                    EligibleScenarios.Inheritance,
                    x => x is INamedType t && t.TypeKind != TypeKind.Interface,
                    _ => $"the aspect {this._aspectClass.ShortName} cannot be added to an interface (because the aspect contains a declarative introduction)" );
            }
        }

        public AspectInstanceResult ExecuteAspect(
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
        {
            var target = aspectInstance.TargetDeclaration.GetTarget( compilationModelRevision );

            return target switch
            {
                ICompilation compilation => this.EvaluateAspect(
                    compilation,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect(
                    constructor,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IParameter parameter => this.EvaluateAspect( parameter, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                ITypeParameter genericParameter => this.EvaluateAspect(
                    genericParameter,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                INamespace ns => this.EvaluateAspect( ns, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                _ => throw new NotSupportedException( $"Cannot add an aspect to a declaration of type {target.DeclarationKind}." )
            };
        }

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectPipelineConfiguration pipelineConfiguration,
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
                        (AspectType: this._aspectClass.ShortName, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( this._aspectClass.Project, pipelineConfiguration.CodeFixFilter, targetDeclaration );

          
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
                    pipelineConfiguration,
                    aspectInstance,
                    cancellationToken );

                var executionContext = new UserCodeExecutionContext(
                    this._serviceProvider,
                    diagnosticSink,
                    UserCodeMemberInfo.FromDelegate( new Action<IAspectBuilder<T>>( aspectOfT.BuildAspect ) ),
                    new AspectLayerId( this._aspectClass ),
                    compilationModelRevision,
                    targetDeclaration );

                if ( !this._serviceProvider.GetService<UserCodeInvoker>().TryInvoke( () => aspectOfT.BuildAspect( aspectBuilder ), executionContext ) )
                {
                    aspectInstance.Skip();

                    return
                        new AspectInstanceResult(
                            false,
                            diagnosticSink.ToImmutable(),
                            ImmutableArray<Advice>.Empty,
                            ImmutableArray<IAspectSource>.Empty );
                }

                var aspectResult = aspectBuilder.ToResult();

                if ( !aspectResult.Success )
                {
                    aspectInstance.Skip();
                }

                return aspectResult;
            
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