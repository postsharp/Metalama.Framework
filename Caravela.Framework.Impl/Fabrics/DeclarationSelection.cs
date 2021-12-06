// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
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
        private readonly ISdkRef<IDeclaration> _containingDeclaration;
        private readonly AspectPredecessor _predecessor;
        private readonly Action<IAspectSource> _registerAspectSource;
        private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<T>> _selector;
        private readonly BoundAspectClassCollection _aspectClasses;
        private readonly IServiceProvider _serviceProvider;

        public DeclarationSelection(
            ISdkRef<IDeclaration> containingDeclaration,
            AspectPredecessor predecessor,
            Action<IAspectSource> registerAspectSource,
            Func<CompilationModel, IDiagnosticAdder, IEnumerable<T>> selectTargets,
            BoundAspectClassCollection aspectClasses,
            IServiceProvider serviceProvider )
        {
            this._containingDeclaration = containingDeclaration;
            this._predecessor = predecessor;
            this._registerAspectSource = registerAspectSource;
            this._selector = selectTargets;
            this._aspectClasses = aspectClasses;
            this._serviceProvider = serviceProvider;
        }

        private AspectClass GetAspectClass<TAspect>()
            where TAspect : IAspect
        {
            var aspectClass = this._aspectClasses[typeof(TAspect).FullName];

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
            var userCodeInvoker = this._serviceProvider.GetService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnostics ) => this.SelectAndValidateTargets(
                        compilation,
                        diagnostics,
                        aspectClass,
                        item =>
                        {
                            if ( !userCodeInvoker.TryInvoke(
                                () => createAspect( item ),
                                executionContext.WithDiagnosticAdder( diagnostics ),
                                out var expression ) )
                            {
                                return null;
                            }

                            var lambda = Expression.Lambda<Func<IAspect>>( expression!.Body, Array.Empty<ParameterExpression>() );

                            if ( !AspectInstance.TryCreateInstance(
                                this._serviceProvider,
                                diagnostics,
                                lambda,
                                item.ToTypedRef<IDeclaration>(),
                                aspectClass,
                                this._predecessor,
                                out var aspectInstance ) )
                            {
                                return null;
                            }
                            else
                            {
                                return aspectInstance;
                            }
                        } ) ) );

            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();
            var userCodeInvoker = this._serviceProvider.GetService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateTargets(
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        t =>
                        {
                            if ( !userCodeInvoker.TryInvoke(
                                () => createAspect( t ),
                                executionContext.WithDiagnosticAdder( diagnosticAdder ),
                                out var aspect ) )
                            {
                                return null;
                            }

                            return new AspectInstance(
                                aspect!,
                                t.ToTypedRef<IDeclaration>(),
                                aspectClass,
                                this._predecessor );
                        } ) ) );

            return this;
        }

        public IDeclarationSelection<T> AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            var userCodeInvoker = this._serviceProvider.GetService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) =>
                    {
                        return this.SelectAndValidateTargets(
                            compilation,
                            diagnosticAdder,
                            aspectClass,
                            t =>
                            {
                                if ( !userCodeInvoker.TryInvoke(
                                    () => new TAspect(),
                                    executionContext.WithDiagnosticAdder( diagnosticAdder ),
                                    out var aspect ) )
                                {
                                    return null;
                                }

                                return new AspectInstance(
                                    aspect!,
                                    t.ToTypedRef<IDeclaration>(),
                                    aspectClass,
                                    this._predecessor );
                            } );
                    } ) );

            return this;
        }

        private IEnumerable<AspectInstance> SelectAndValidateTargets(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectClass aspectClass,
            Func<T, AspectInstance?> createAspectInstance )
        {
            foreach ( var targetDeclaration in this._selector( compilation, diagnosticAdder ) )
            {
                var predecessorInstance = (IAspectPredecessorImpl) this._predecessor.Instance;

                var containingDeclaration = this._containingDeclaration.GetTarget( compilation ).AssertNotNull();

                if ( !targetDeclaration.IsContainedIn( containingDeclaration ) || targetDeclaration.DeclaringAssembly.IsExternal )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.CanAddChildAspectOnlyUnderParent.CreateDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, containingDeclaration) ) );

                    continue;
                }

                var eligibility = aspectClass.GetEligibility( targetDeclaration );
                var canBeInherited = ((IDeclarationImpl) targetDeclaration).CanBeInherited;
                var requiredEligibility = canBeInherited ? EligibleScenarios.Aspect | EligibleScenarios.Inheritance : EligibleScenarios.Aspect;

                if ( !eligibility.IncludesAny( requiredEligibility ) )
                {
                    var reason = aspectClass.GetIneligibilityJustification( requiredEligibility, new DescribedObject<IDeclaration>( targetDeclaration ) )!;

                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, reason) ) );

                    continue;
                }

                var aspectInstance = createAspectInstance( targetDeclaration );

                if ( aspectInstance != null )
                {
                    yield return aspectInstance;
                }
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