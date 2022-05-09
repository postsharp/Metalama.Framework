// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Attribute = System.Attribute;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IAspectReceiver{TDeclaration}"/>, which offers a fluent
    /// API to programmatically add children aspects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AspectReceiver<T> : IAspectReceiver<T>
        where T : class, IDeclaration
    {
        private readonly ISdkRef<IDeclaration> _containingDeclaration;
        private readonly IAspectReceiverParent _parent;
        private readonly CompilationModelVersion _compilationModelVersion;
        private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<T>> _selector;

        public AspectReceiver(
            ISdkRef<IDeclaration> containingDeclaration,
            IAspectReceiverParent parent,
            CompilationModelVersion compilationModelVersion,
            Func<CompilationModel, IDiagnosticAdder, IEnumerable<T>> selectTargets )
        {
            this._containingDeclaration = containingDeclaration;
            this._parent = parent;
            this._compilationModelVersion = compilationModelVersion;
            this._selector = selectTargets;
        }

        private AspectClass GetAspectClass<TAspect>()
            where TAspect : IAspect
        {
            var aspectClass = this._parent.AspectClasses[typeof(TAspect).FullName];

            if ( aspectClass.IsAbstract )
            {
                throw new ArgumentOutOfRangeException( nameof(TAspect), UserMessageFormatter.Format( $"'{typeof(TAspect)}' is an abstract type." ) );
            }

            return (AspectClass) aspectClass;
        }

        private void RegisterAspectSource( IAspectSource aspectSource ) => this._parent.AddAspectSource( aspectSource );

        private void RegisterValidatorSource( ProgrammaticValidatorSource validatorSource ) => this._parent.AddValidatorSource( validatorSource );

        public void ValidateReferences( ValidatorDelegate<ReferenceValidationContext> validateMethod, ReferenceKinds referenceKinds )
        {
            var methodInfo = validateMethod.Method;

            if ( methodInfo.DeclaringType != this._parent.Type )
            {
                throw new ArgumentOutOfRangeException( nameof(validateMethod), $"The delegate must point to a method of type '{this._parent.Type};." );
            }

            if ( methodInfo.DeclaringType != null &&
                 methodInfo.DeclaringType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static )
                     .Count( m => m.Name == methodInfo.Name ) > 1 )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(validateMethod),
                    $"The type '{this._parent.Type}' must have only one method called '{methodInfo.Name}'." );
            }

            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent,
                    ValidatorKind.Reference,
                    CompilationModelVersion.Current,
                    this._parent.AspectPredecessor,
                    validateMethod.Method,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorTargets(
                        compilation,
                        diagnostics,
                        item => new ReferenceValidatorInstance(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( source.Predecessor.Instance ),
                            referenceKinds ) ) ) );
        }

        public void Validate( ValidatorDelegate<DeclarationValidationContext> validateMethod )
        {
            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent,
                    ValidatorKind.Definition,
                    this._compilationModelVersion,
                    this._parent.AspectPredecessor,
                    validateMethod,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorTargets(
                        compilation,
                        diagnostics,
                        item => new DeclarationValidatorInstance(
                            item,
                            (ValidatorDriver<DeclarationValidationContext>) source.Driver,
                            ValidatorImplementation.Create( source.Predecessor.Instance ) ) ) ) );
        }

        public void ReportDiagnostic( Func<T, IDiagnostic> diagnostic )
        {
            this.Validate( new FinalValidatorHelper<IDiagnostic>( diagnostic ).ReportDiagnostic );
        }

        public void SuppressDiagnostic( Func<T, SuppressionDefinition> suppression )
        {
            this.Validate( new FinalValidatorHelper<SuppressionDefinition>( suppression ).SuppressDiagnostic );
        }

        public void SuggestCodeFix( Func<T, CodeFix> codeFix )
        {
            this.Validate( new FinalValidatorHelper<CodeFix>( codeFix ).SuggestCodeFix );
        }

        public IValidatorReceiver<IDeclaration> AfterAllAspects()
            => new AspectReceiver<IDeclaration>( this._containingDeclaration, this._parent, CompilationModelVersion.Final, this._selector );

        public IValidatorReceiver<IDeclaration> BeforeAnyAspect()
            => new AspectReceiver<IDeclaration>( this._containingDeclaration, this._parent, CompilationModelVersion.Initial, this._selector );

        private class FinalValidatorHelper<TOutput>
        {
            private readonly Func<T, TOutput> _func;

            public FinalValidatorHelper( Func<T, TOutput> func )
            {
                this._func = func;
            }

            public void ReportDiagnostic( in DeclarationValidationContext context )
            {
                context.Diagnostics.Report( (IDiagnostic) this._func( (T) context.Declaration )! );
            }

            public void SuppressDiagnostic( in DeclarationValidationContext context )
            {
                context.Diagnostics.Suppress( (SuppressionDefinition) (object) this._func( (T) context.Declaration )! );
            }

            public void SuggestCodeFix( in DeclarationValidationContext context )
            {
                context.Diagnostics.Suggest( (CodeFix) (object) this._func( (T) context.Declaration )! );
            }
        }

        public void AddAspect<TAspect>( Func<T, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnostics ) => this.SelectAndValidateAspectTargets(
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
                                    this._parent.ServiceProvider,
                                    diagnostics,
                                    lambda,
                                    item.ToTypedRef<IDeclaration>(),
                                    aspectClass,
                                    this._parent.AspectPredecessor,
                                    out var aspectInstance ) )
                            {
                                return null;
                            }
                            else
                            {
                                return aspectInstance;
                            }
                        } ) ) );
        }

        public void AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : Attribute, IAspect<T>
        {
            var aspectClass = this.GetAspectClass<TAspect>();
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
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
                                this._parent.AspectPredecessor );
                        } ) ) );
        }

        public void AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
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
                                this._parent.AspectPredecessor );
                        } ) ) );
        }

        private IEnumerable<TResult> SelectAndValidateAspectTargets<TResult>(
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectClass aspectClass,
            Func<T, TResult?> createResult )
        {
            foreach ( var targetDeclaration in this._selector( compilation, diagnosticAdder ) )
            {
                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                var containingDeclaration = this._containingDeclaration.GetTarget( compilation ).AssertNotNull();

                if ( !(targetDeclaration.IsContainedIn( containingDeclaration )
                       || (containingDeclaration is IParameter p && p.DeclaringMember.Equals( targetDeclaration ))
                       || (containingDeclaration is IMember m && m.DeclaringType.Equals( targetDeclaration )))
                     || targetDeclaration.DeclaringAssembly.IsExternal )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.CanAddChildAspectOnlyUnderParent.CreateRoslynDiagnostic(
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
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, reason) ) );

                    continue;
                }

                var aspectInstance = createResult( targetDeclaration );

                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if ( aspectInstance != null )
                {
                    yield return aspectInstance;
                }
            }
        }

        private IEnumerable<ValidatorInstance> SelectAndValidateValidatorTargets(
            CompilationModel compilation,
            IDiagnosticSink diagnosticSink,
            Func<T, ValidatorInstance?> createResult )
        {
            var diagnosticAdder = (IDiagnosticAdder) diagnosticSink;

            foreach ( var targetDeclaration in this._selector( compilation, diagnosticAdder ) )
            {
                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                var containingDeclaration = this._containingDeclaration.GetTarget( compilation ).AssertNotNull();

                if ( !targetDeclaration.IsContainedIn( containingDeclaration ) || targetDeclaration.DeclaringAssembly.IsExternal )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), targetDeclaration, containingDeclaration) ) );

                    continue;
                }

                var validatorInstance = createResult( targetDeclaration );

                if ( validatorInstance != null )
                {
                    yield return validatorInstance;
                }
            }
        }

        public void RequireAspect<TAspect>()
            where TAspect : IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            this.RegisterAspectSource(
                new ProgrammaticAspectSource<TAspect, T>(
                    aspectClass,
                    getRequirements: ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        t => new AspectRequirement(
                            t.ToTypedRef<IDeclaration>(),
                            this._parent.AspectPredecessor.Instance ) ) ) );
        }

        [Obsolete( "Not implemented." )]
        public void AddAnnotation<TAspect, TAnnotation>( Func<T, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<T, TAspect>, IEligible<T>
            => throw new NotImplementedException();
    }
}