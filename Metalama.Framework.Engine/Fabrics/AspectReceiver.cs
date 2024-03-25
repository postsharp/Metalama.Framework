// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Options;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="IAspectReceiver{TDeclaration}"/>, which offers a fluent
    /// API to programmatically add children aspects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class AspectReceiver<T> : IAspectReceiver<T>
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
            => this.GetAspectClass( typeof(TAspect) );

        private AspectClass GetAspectClass( Type aspectType )
        {
            var aspectClass = this._parent.AspectClasses[aspectType.FullName.AssertNotNull()];

            if ( aspectClass.IsAbstract )
            {
                throw new ArgumentOutOfRangeException( nameof(aspectType), MetalamaStringFormatter.Format( $"'{aspectType}' is an abstract type." ) );
            }

            return (AspectClass) aspectClass;
        }

        private void RegisterAspectSource( IAspectSource aspectSource )
        {
            this._parent.LicenseVerifier?.VerifyCanAddChildAspect( this._parent.AspectPredecessor );

            this._parent.AddAspectSource( aspectSource );
        }

        private void RegisterValidatorSource( ProgrammaticValidatorSource validatorSource )
        {
            this._parent.LicenseVerifier?.VerifyCanAddValidator( this._parent.AspectPredecessor );

            this._parent.AddValidatorSource( validatorSource );
        }

        private void RegisterOptionsSource( IHierarchicalOptionsSource hierarchicalOptionsSource )
        {
            this._parent.AddOptionsSource( hierarchicalOptionsSource );
        }

        private IEnumerable<ReferenceValidatorInstance> SelectReferenceValidatorInstances(
            IDeclaration validatedDeclaration,
            ValidatorDriver driver,
            ValidatorImplementation implementation,
            ReferenceKinds referenceKinds,
            bool includeDerivedTypes )
        {
            var description = MetalamaStringFormatter.Format(
                $"reference validator for {validatedDeclaration.DeclarationKind} '{validatedDeclaration.ToDisplayString()}' provided by {this._parent.DiagnosticSourceDescription}" );

            yield return new ReferenceValidatorInstance(
                validatedDeclaration,
                driver,
                implementation,
                referenceKinds,
                includeDerivedTypes,
                description );

            if ( validatedDeclaration is Method validatedMethod )
            {
                switch ( validatedMethod.MethodKind )
                {
                    case MethodKind.PropertyGet:
                        yield return new ReferenceValidatorInstance(
                            validatedMethod.DeclaringMember!,
                            driver,
                            implementation,
                            ReferenceKinds.Default,
                            includeDerivedTypes,
                            description );

                        break;

                    // TODO: The validator doesn't distinguish event add and event remove.
                    case MethodKind.PropertySet:
                    case MethodKind.EventAdd:
                    case MethodKind.EventRemove:
                        yield return new ReferenceValidatorInstance(
                            validatedMethod.DeclaringMember!,
                            driver,
                            implementation,
                            ReferenceKinds.Assignment,
                            includeDerivedTypes,
                            description );

                        break;
                }
            }
        }

        public void ValidateReferences(
            ValidatorDelegate<ReferenceValidationContext> validateMethod,
            ReferenceKinds referenceKinds,
            bool includeDerivedTypes )
        {
            var methodInfo = validateMethod.Method;

            if ( methodInfo.DeclaringType?.IsAssignableFrom( this._parent.Type ) != true )
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

            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent.GetReferenceValidatorDriver( validateMethod.Method ),
                    ValidatorKind.Reference,
                    CompilationModelVersion.Current,
                    this._parent.AspectPredecessor,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorOrConfiguratorTargets<ValidatorInstance>(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, (IDiagnosticAdder) diagnostics ),
                        compilation,
                        (IDiagnosticAdder) diagnostics,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        item => this.SelectReferenceValidatorInstances(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( source.Predecessor.Instance ),
                            referenceKinds,
                            includeDerivedTypes ) ) ) );
        }

        public void ValidateReferences( ReferenceValidator validator )
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent.GetReferenceValidatorDriver( validator.GetType() ),
                    ValidatorKind.Reference,
                    CompilationModelVersion.Current,
                    this._parent.AspectPredecessor,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorOrConfiguratorTargets<ValidatorInstance>(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, (IDiagnosticAdder) diagnostics ),
                        compilation,
                        (IDiagnosticAdder) diagnostics,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        item => this.SelectReferenceValidatorInstances(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( validator ),
                            validator.ValidatedReferenceKinds,
                            validator.IncludeDerivedTypes ) ) ) );
        }

        public void ValidateReferences<TValidator>( Func<T, TValidator> getValidator )
            where TValidator : ReferenceValidator
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent.GetReferenceValidatorDriver( typeof(TValidator) ),
                    ValidatorKind.Reference,
                    CompilationModelVersion.Current,
                    this._parent.AspectPredecessor,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorOrConfiguratorTargets<ValidatorInstance>(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, (IDiagnosticAdder) diagnostics ),
                        compilation,
                        (IDiagnosticAdder) diagnostics,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        item =>
                        {
                            var validator = userCodeInvoker.Invoke( () => getValidator( item ), executionContext );

                            return this.SelectReferenceValidatorInstances(
                                item,
                                source.Driver,
                                ValidatorImplementation.Create( validator ),
                                validator.ValidatedReferenceKinds,
                                validator.IncludeDerivedTypes );
                        } ) ) );
        }

        public void Validate( ValidatorDelegate<DeclarationValidationContext> validateMethod )
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterValidatorSource(
                new ProgrammaticValidatorSource(
                    this._parent,
                    ValidatorKind.Definition,
                    this._compilationModelVersion,
                    this._parent.AspectPredecessor,
                    validateMethod,
                    ( source, compilation, diagnostics ) => this.SelectAndValidateValidatorOrConfiguratorTargets<ValidatorInstance>(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, (IDiagnosticAdder) diagnostics ),
                        compilation,
                        (IDiagnosticAdder) diagnostics,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        item => new[]
                        {
                            new DeclarationValidatorInstance(
                                item,
                                (ValidatorDriver<DeclarationValidationContext>) source.Driver,
                                ValidatorImplementation.Create( source.Predecessor.Instance ),
                                this._parent.DiagnosticSourceDescription )
                        } ) ) );
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

        public IValidatorReceiver<T> AfterAllAspects()
            => new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Final, this._selector );

        public IValidatorReceiver<T> BeforeAnyAspect()
            => new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Initial, this._selector );

        public IAspectReceiver<TMember> SelectMany<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => new AspectReceiver<TMember>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( c, d ) => this._selector( c, d ).SelectMany( selector ) );

        public IAspectReceiver<TMember> Select<TMember>( Func<T, TMember> selector )
            where TMember : class, IDeclaration
            => new AspectReceiver<TMember>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( c, d ) => this._selector( c, d ).Select( selector ) );

        public IAspectReceiver<T> Where( Func<T, bool> predicate )
            => new AspectReceiver<T>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( c, d ) => this._selector( c, d ).Where( predicate ) );

        public void SetOptions<TOptions>( Func<T, TOptions> func )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( compilation, diagnosticAdder )
                        => this.SelectAndValidateValidatorOrConfiguratorTargets<HierarchicalOptionsInstance>(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnosticAdder ),
                            compilation,
                            diagnosticAdder,
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            t =>
                            {
                                if ( !userCodeInvoker.TryInvoke(
                                        () => func( t ),
                                        executionContext,
                                        out var options ) )
                                {
                                    return null;
                                }

                                return new[]
                                {
                                    new HierarchicalOptionsInstance(
                                        t,
                                        options )
                                };
                            } ) ) );
        }

        public void SetOptions<TOptions>( TOptions options )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( compilation, diagnosticAdder )
                        => this.SelectAndValidateValidatorOrConfiguratorTargets<HierarchicalOptionsInstance>(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnosticAdder ),
                            compilation,
                            diagnosticAdder,
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            t =>
                            {
                                return new[]
                                {
                                    new HierarchicalOptionsInstance(
                                        t,
                                        options )
                                };
                            } ) ) );
        }

        IValidatorReceiver<T> IValidatorReceiver<T>.Where( Func<T, bool> predicate ) => this.Where( predicate );

        IValidatorReceiver<TMember> IValidatorReceiver<T>.SelectMany<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.SelectMany( selector );

        IValidatorReceiver<TMember> IValidatorReceiver<T>.Select<TMember>( Func<T, TMember> selector ) => this.Select( selector );

        private sealed class FinalValidatorHelper<TOutput>
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

        public void AddAspect<TAspect>( Func<T, TAspect> createAspect )
            where TAspect : class, IAspect<T>
            => this.AddAspectIfEligible( createAspect, EligibleScenarios.None );

        public void AddAspect( Type aspectType, Func<T, IAspect> createAspect ) => this.AddAspectIfEligible( aspectType, createAspect, EligibleScenarios.None );

        public void AddAspectIfEligible( Type aspectType, Func<T, IAspect> createAspect, EligibleScenarios eligibility )
        {
            var aspectClass = this.GetAspectClass( aspectType );
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnosticAdder ),
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        eligibility,
                        t =>
                        {
                            if ( !userCodeInvoker.TryInvoke(
                                    () => createAspect( t ),
                                    executionContext,
                                    out var aspect ) )
                            {
                                return null;
                            }

                            return new AspectInstance(
                                aspect,
                                t,
                                aspectClass,
                                this._parent.AspectPredecessor );
                        } ) ) );
        }

        public void AddAspectIfEligible<TAspect>( Func<T, TAspect> createAspect, EligibleScenarios eligibility )
            where TAspect : class, IAspect<T>
            => this.AddAspectIfEligible( typeof(TAspect), createAspect, eligibility );

        public void AddAspect<TAspect>()
            where TAspect : class, IAspect<T>, new()
            => this.AddAspectIfEligible<TAspect>( EligibleScenarios.None );

        public void AddAspectIfEligible<TAspect>( EligibleScenarios eligibility )
            where TAspect : class, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();

            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource(
                    aspectClass,
                    ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnosticAdder ),
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        eligibility,
                        t =>
                        {
                            if ( !userCodeInvoker.TryInvoke(
                                    () => new TAspect(),
                                    executionContext,
                                    out var aspect ) )
                            {
                                return null;
                            }

                            return new AspectInstance(
                                aspect,
                                t,
                                aspectClass,
                                this._parent.AspectPredecessor );
                        } ) ) );
        }

        private IEnumerable<TResult> SelectAndValidateAspectTargets<TResult>(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            AspectClass aspectClass,
            EligibleScenarios filteredEligibility,
            Func<T, TResult?> createResult )
        {
            IReadOnlyList<T> targets;

            if ( invoker != null && executionContext != null )
            {
#if DEBUG
                if ( !ReferenceEquals( executionContext.Compilation, compilation ) )
                {
                    throw new AssertionFailedException( "Execution context mismatch." );
                }
#endif

                targets = invoker.Invoke( () => this._selector( compilation, diagnosticAdder ).ToReadOnlyList(), executionContext );
            }
            else
            {
                targets = this._selector( compilation, diagnosticAdder ).ToReadOnlyList();
            }

            foreach ( var targetDeclaration in targets )
            {
                if ( targetDeclaration == null! )
                {
                    continue;
                }

                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                // Verify containment.
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

                // Verify eligibility.
                var eligibility = aspectClass.GetEligibility( targetDeclaration );

                if ( filteredEligibility != EligibleScenarios.None && !eligibility.IncludesAny( filteredEligibility ) )
                {
                    continue;
                }

                var canBeInherited = ((IDeclarationImpl) targetDeclaration).CanBeInherited;
                var requiredEligibility = canBeInherited ? EligibleScenarios.Default | EligibleScenarios.Inheritance : EligibleScenarios.Default;

                if ( !eligibility.IncludesAny( requiredEligibility ) )
                {
                    var reason = aspectClass.GetIneligibilityJustification( requiredEligibility, new DescribedObject<IDeclaration>( targetDeclaration ) )!;

                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, reason) ) );

                    continue;
                }

                TResult? aspectInstance;

                if ( invoker != null && executionContext != null )
                {
                    aspectInstance = invoker.Invoke( () => createResult( targetDeclaration ), executionContext );
                }
                else
                {
                    aspectInstance = createResult( targetDeclaration );
                }

                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if ( aspectInstance != null )
                {
                    yield return aspectInstance;
                }
            }
        }

        private IEnumerable<TResult> SelectAndValidateValidatorOrConfiguratorTargets<TResult>(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            DiagnosticDefinition<(FormattableString Predecessor, IDeclaration Child, IDeclaration Parent)> diagnosticDefinition,
            Func<T, IEnumerable<TResult?>?> createResult )
            where TResult : class
        {
            IReadOnlyList<T> targets;

            if ( invoker != null && executionContext != null )
            {
#if DEBUG
                if ( !ReferenceEquals( executionContext.Compilation, compilation ) )
                {
                    throw new AssertionFailedException( "Execution context mismatch." );
                }
#endif
                targets = invoker.Invoke( () => this._selector( compilation, diagnosticAdder ).ToReadOnlyList(), executionContext );
            }
            else
            {
                targets = this._selector( compilation, diagnosticAdder ).ToReadOnlyList();
            }

            foreach ( var targetDeclaration in targets )
            {
                if ( targetDeclaration == null! )
                {
                    continue;
                }

                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                var containingTypeOrCompilation = (IDeclaration?) this._containingDeclaration.GetTarget( compilation ).AssertNotNull().GetTopmostNamedType()
                                                  ?? compilation;

                if ( (!targetDeclaration.IsContainedIn( containingTypeOrCompilation ) || targetDeclaration.DeclaringAssembly.IsExternal)
                     && containingTypeOrCompilation.DeclarationKind != DeclarationKind.Compilation )
                {
                    diagnosticAdder.Report(
                        diagnosticDefinition.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), targetDeclaration, containingTypeOrCompilation) ) );

                    continue;
                }

                var results = createResult( targetDeclaration );

                if ( results != null )
                {
                    foreach ( var result in results )
                    {
                        if ( result != null )
                        {
                            yield return result;
                        }
                    }
                }
            }
        }

        public void RequireAspect<TAspect>()
            where TAspect : class, IAspect<T>, new()
        {
            var aspectClass = this.GetAspectClass<TAspect>();
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterAspectSource(
                new ProgrammaticAspectSource(
                    aspectClass,
                    getRequirements: ( compilation, diagnosticAdder ) => this.SelectAndValidateAspectTargets(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnosticAdder ),
                        compilation,
                        diagnosticAdder,
                        aspectClass,
                        EligibleScenarios.None,
                        t => new AspectRequirement(
                            t.ToTypedRef<IDeclaration>(),
                            this._parent.AspectPredecessor.Instance ) ) ) );
        }
    }
}