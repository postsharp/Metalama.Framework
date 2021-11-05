// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Attribute = System.Attribute;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IDeclarationSelection{TDeclaration}"/>, which offers a fluent
    /// API to programmatically add children aspects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DeclarationSelection<T> : IDeclarationSelection<T>
        where T : class, IDeclaration
    {
        private readonly IDeclaration _containingDeclaration;
        private readonly AspectPredecessor _predecessor;
        private readonly Action<IAspectSource> _registerAspectSource;
        private readonly Func<CompilationModel, IEnumerable<T>> _selector;
        private readonly AspectPipelineConfiguration _pipelineConfiguration;

        public DeclarationSelection(
            IDeclaration containingDeclaration,
            AspectPredecessor predecessor,
            Action<IAspectSource> registerAspectSource,
            Func<CompilationModel, IEnumerable<T>> selectTargets,
            AspectPipelineConfiguration pipelineConfiguration )
        {
            this._containingDeclaration = containingDeclaration;
            this._predecessor = predecessor;
            this._registerAspectSource = registerAspectSource;
            this._selector = selectTargets;
            this._pipelineConfiguration = pipelineConfiguration;
        }

        private AspectClass GetAspectClass<TAspect>()
            where TAspect : IAspect
        {
            var aspectClass = this._pipelineConfiguration.GetAspectClass( typeof(TAspect).FullName );

            if ( aspectClass.IsAbstract )
            {
                throw new ArgumentOutOfRangeException( nameof(TAspect), UserMessageFormatter.Format( $"'{typeof(TAspect)}' is an abstract type." ) );
            }

            return (AspectClass) aspectClass;
        }

        private void RegisterAspectSource( IAspectSource aspectSource ) => this._registerAspectSource( aspectSource );

        public IDeclarationSelection<T> AddAspect<TAspect>( Func<T, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnostics ) => this.SelectAndValidateTargets(
                        compilation,
                        diagnostics,
                        aspectClass,
                        item =>
                        {
                            var lambda = Expression.Lambda<Func<IAspect>>(
                                this._pipelineConfiguration.UserCodeInvoker.Invoke( () => createAspect( item ) ).Body,
                                Array.Empty<ParameterExpression>() );

                            return new AspectInstance(
                                this._pipelineConfiguration.ServiceProvider,
                                lambda,
                                item,
                                aspectClass,
                                this._predecessor );
                        } ) ) );

            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateTargets(
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        t => new AspectInstance(
                            this._pipelineConfiguration.UserCodeInvoker.Invoke( () => createAspect( t ) ),
                            t,
                            aspectClass,
                            this._predecessor ) ) ) );

            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateTargets(
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        t => new AspectInstance(
                            new TAspect(),
                            t,
                            aspectClass,
                            this._predecessor ) ) ) );

            return this;
        }

        private IEnumerable<AspectInstance> SelectAndValidateTargets(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectClass aspectClass,
            Func<T, AspectInstance> createAspectInstance )
        {
            foreach ( var item in this._selector( compilation ) )
            {
                var predecessorInstance = (IAspectPredecessorImpl) this._predecessor.Instance;

                if ( !item.IsContainedIn( this._containingDeclaration ) || item.DeclaringAssembly.IsExternal )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.CanAddChildAspectOnlyUnderParent.CreateDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor(), aspectClass.ShortName, item, this._containingDeclaration) ) );

                    continue;
                }

                var eligibility = aspectClass.GetEligibility( item );
                var canBeInherited = ((IDeclarationImpl) item).CanBeInherited;
                var requiredEligibility = canBeInherited ? EligibleScenarios.Aspect | EligibleScenarios.Inheritance : EligibleScenarios.Aspect;

                if ( !eligibility.IncludesAny( requiredEligibility ) )
                {
                    var reason = aspectClass.GetIneligibilityJustification( requiredEligibility, new DescribedObject<IDeclaration>( item ) )!;

                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor(), aspectClass.ShortName, item, reason) ) );

                    continue;
                }

                yield return createAspectInstance( item );
            }
        }

        [Obsolete( "Not implemented." )]
        public IDeclarationSelection<T> RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new()
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public IDeclarationSelection<T> AddAnnotation<TAspect, TAnnotation>( Func<T, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<T, TAspect>, IEligible<T>
            => throw new NotImplementedException();
    }
}