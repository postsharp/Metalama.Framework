// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
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
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly Compilation _compilation;
        private readonly List<(AttributeData Attribute, ISymbol Member)> _declarativeAdviceAttributes;

        public INamedTypeSymbol AspectType { get; }

        public AspectDriver( IServiceProvider serviceProvider, INamedTypeSymbol aspectType, Compilation compilation )
        {
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this._compilation = compilation;
            this.AspectType = aspectType;

            this._declarativeAdviceAttributes =
                (from member in aspectType.GetMembers()
                 from attribute in member.GetAttributes()
                 where attribute.AttributeClass?.Is( typeof(AdviceAttribute) ) ?? false
                 select (attribute, member)).ToList();
        }

        internal AspectInstanceResult ExecuteAspect(
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken )
        {
            return aspectInstance.TargetDeclaration switch
            {
                ICompilation compilation => this.EvaluateAspect( compilation, aspectInstance, compilationModelRevision, cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, compilationModelRevision, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, compilationModelRevision, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, compilationModelRevision, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, compilationModelRevision, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect( constructor, aspectInstance, compilationModelRevision, cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, compilationModelRevision, cancellationToken ),
                _ => throw new NotImplementedException()
            };
        }

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            AspectInstance aspectInstance,
            CompilationModel compilationModelRevision,
            CancellationToken cancellationToken )
            where T : class, IDeclaration
        {
            static AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
                => new(
                    false,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty );

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
                        (this.AspectType, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( aspectInstance.AspectClass.Project, targetDeclaration );

            using ( DiagnosticContext.WithDefaultLocation( diagnosticSink.DefaultScope?.DiagnosticLocation ) )
            {
                var declarativeAdvices =
                    this._declarativeAdviceAttributes
                        .Select( x => CreateDeclarativeAdvice( aspectInstance, diagnosticSink, targetDeclaration, x.Attribute, x.Member ) )
                        .WhereNotNull()
                        .ToArray();

                var adviceFactory = new AdviceFactory(
                    compilationModelRevision,
                    diagnosticSink,
                    declarativeAdvices,
                    compilationModelRevision.Factory.GetNamedType( this.AspectType ),
                    aspectInstance );

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
                            ImmutableArray<IAspectSource>.Empty );
                }
                catch ( Exception e )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.ExceptionInUserCode.CreateDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.AspectType, e.GetType().Name, e.Format( 5 )) );

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
            IDiagnosticAdder diagnosticAdder,
            T aspectTarget,
            AttributeData templateAttributeData,
            ISymbol templateDeclaration )
            where T : IDeclaration
        {
            templateAttributeData.TryCreateAdvice(
                aspect,
                diagnosticAdder,
                aspectTarget,
                ((CompilationModel) aspectTarget.Compilation).Factory.GetDeclaration( templateDeclaration ),
                null,
                out var advice );

            return advice;
        }
    }
}