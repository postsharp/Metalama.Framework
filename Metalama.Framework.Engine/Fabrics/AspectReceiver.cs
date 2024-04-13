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
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Options;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly Func<CompilationModel, Func<T, OutboundActionCollector, CancellationToken, Task>, OutboundActionCollector, CancellationToken, Task> _adder;
        private readonly IConcurrentTaskRunner _concurrentTaskRunner;

        public AspectReceiver(
            ISdkRef<IDeclaration> containingDeclaration,
            IAspectReceiverParent parent,
            CompilationModelVersion compilationModelVersion,
            Func<CompilationModel, Func<T, OutboundActionCollector, CancellationToken, Task>, OutboundActionCollector, CancellationToken, Task> addTargets )
        {
            this._concurrentTaskRunner = parent.ServiceProvider.GetRequiredService<IConcurrentTaskRunner>();
            this._containingDeclaration = containingDeclaration;
            this._parent = parent;
            this._compilationModelVersion = compilationModelVersion;
            this._adder = addTargets;
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

        private Task SelectReferenceValidatorInstancesAsync(
            IDeclaration validatedDeclaration,
            ValidatorDriver driver,
            ValidatorImplementation implementation,
            ReferenceKinds referenceKinds,
            bool includeDerivedTypes,
            OutboundActionCollector collector )
        {
            var description = MetalamaStringFormatter.Format(
                $"reference validator for {validatedDeclaration.DeclarationKind} '{validatedDeclaration.ToDisplayString()}' provided by {this._parent.DiagnosticSourceDescription}" );

            collector.AddValidator(
                new ReferenceValidatorInstance(
                    validatedDeclaration,
                    driver,
                    implementation,
                    referenceKinds,
                    includeDerivedTypes,
                    description ) );

            if ( validatedDeclaration is Method validatedMethod )
            {
                switch ( validatedMethod.MethodKind )
                {
                    case MethodKind.PropertyGet:
                        collector.AddValidator(
                            new ReferenceValidatorInstance(
                                validatedMethod.DeclaringMember!,
                                driver,
                                implementation,
                                ReferenceKinds.Default,
                                includeDerivedTypes,
                                description ) );

                        break;

                    // TODO: The validator doesn't distinguish event add and event remove.
                    case MethodKind.PropertySet:
                    case MethodKind.EventAdd:
                    case MethodKind.EventRemove:
                        collector.AddValidator(
                            new ReferenceValidatorInstance(
                                validatedMethod.DeclaringMember!,
                                driver,
                                implementation,
                                ReferenceKinds.Assignment,
                                includeDerivedTypes,
                                description ) );

                        break;
                }
            }

            return Task.CompletedTask;
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
                    ( source, compilation, collector, cancellationToken ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        ( item, collector ) => this.SelectReferenceValidatorInstancesAsync(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( source.Predecessor.Instance ),
                            referenceKinds,
                            includeDerivedTypes,
                            collector ),
                        collector,
                        cancellationToken ) ) );
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
                    ( source, compilation, collector, cancellationToken ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        ( item, c ) => this.SelectReferenceValidatorInstancesAsync(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( validator ),
                            validator.ValidatedReferenceKinds,
                            validator.IncludeDerivedTypes,
                            c ),
                        collector,
                        cancellationToken ) ) );
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
                    ( source, compilation, collector, cancellationToken ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        ( item, c ) =>
                        {
                            var validator = userCodeInvoker.Invoke( () => getValidator( item ), executionContext );

                            return this.SelectReferenceValidatorInstancesAsync(
                                item,
                                source.Driver,
                                ValidatorImplementation.Create( validator ),
                                validator.ValidatedReferenceKinds,
                                validator.IncludeDerivedTypes,
                                c );
                        },
                        collector,
                        cancellationToken ) ) );
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
                    ( source, compilation, collector, cancellationToken ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        ( item, c ) =>
                        {
                            c.AddValidator(
                                new DeclarationValidatorInstance(
                                    item,
                                    (ValidatorDriver<DeclarationValidationContext>) source.Driver,
                                    ValidatorImplementation.Create( source.Predecessor.Instance ),
                                    this._parent.DiagnosticSourceDescription ) );

                            return Task.CompletedTask;
                        },
                        collector,
                        cancellationToken ) ) );
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
            => new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Final, this._adder );

        public IValidatorReceiver<T> BeforeAnyAspect()
            => new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Initial, this._adder );

        public IAspectReceiver<TMember> SelectMany<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => new AspectReceiver<TMember>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( compilation, action, collector, cancellationToken ) => this._adder(
                    compilation,
                    ( declaration, collector, cancellationToken ) =>
                    {
                        var children = selector( declaration );

                        return this._concurrentTaskRunner.RunInParallelAsync(
                            children,
                            child => action( child, collector, cancellationToken ),
                            cancellationToken );
                    },
                    collector,
                    cancellationToken ) );

        public IAspectReceiver<TMember> Select<TMember>( Func<T, TMember> selector )
            where TMember : class, IDeclaration
            => new AspectReceiver<TMember>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( compilation, action, collector, cancellationToken ) => this._adder(
                    compilation,
                    ( declaration, collector, cancellationToken ) => action( selector( declaration ), collector, cancellationToken ),
                    collector,
                    cancellationToken ) );

        public IAspectReceiver<T> Where( Func<T, bool> predicate )
            => new AspectReceiver<T>(
                this._containingDeclaration,
                this._parent,
                this._compilationModelVersion,
                ( compilation, action, collector, cancellationToken ) => this._adder(
                    compilation,
                    ( declaration, collector, cancellationToken ) =>
                    {
                        if ( predicate( declaration ) )
                        {
                            return action( declaration, collector, cancellationToken );
                        }
                        else
                        {
                            return Task.CompletedTask;
                        }
                    },
                    collector,
                    cancellationToken ) );

        public void SetOptions<TOptions>( Func<T, TOptions> func )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( compilation, collector, cancellationToken )
                        => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                            compilation,
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            ( declaration, collector ) =>
                            {
                                if ( userCodeInvoker.TryInvoke(
                                        () => func( declaration ),
                                        executionContext,
                                        out var options ) )
                                {
                                    collector.AddOptions(
                                        new HierarchicalOptionsInstance(
                                            declaration,
                                            options ) );
                                }

                                return Task.CompletedTask;
                            },
                            collector,
                            cancellationToken ) ) );
        }

        public void SetOptions<TOptions>( TOptions options )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( compilation, collector, cancellationToken )
                        => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                            compilation,
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            ( declaration, collector ) =>
                            {
                                collector.AddOptions(
                                    new HierarchicalOptionsInstance(
                                        declaration,
                                        options ) );

                                return Task.CompletedTask;
                            },
                            collector,
                            cancellationToken ) ) );
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
                    ( compilation, collector, cancellationToken ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        aspectClass,
                        eligibility,
                        ( t, collector ) =>
                        {
                            if ( userCodeInvoker.TryInvoke(
                                    () => createAspect( t ),
                                    executionContext,
                                    out var aspect ) )
                            {
                                collector.AddAspectInstance(
                                    new AspectInstance(
                                        aspect,
                                        t,
                                        aspectClass,
                                        this._parent.AspectPredecessor ) );
                            }

                            return Task.CompletedTask;
                        },
                        collector,
                        cancellationToken ) ) );
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
                    ( compilation, collector, cancellationToken ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        aspectClass,
                        eligibility,
                        ( t, collector2 ) =>
                        {
                            if ( userCodeInvoker.TryInvoke(
                                    () => new TAspect(),
                                    executionContext,
                                    out var aspect ) )
                            {
                                collector2.AddAspectInstance(
                                    new AspectInstance(
                                        aspect,
                                        t,
                                        aspectClass,
                                        this._parent.AspectPredecessor ) );
                            }

                            return Task.CompletedTask;
                        },
                        collector,
                        cancellationToken ) ) );
        }

        private Task SelectAndValidateAspectTargetsAsync(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            CompilationModel compilation,
            AspectClass aspectClass,
            EligibleScenarios filteredEligibility,
            Func<T, OutboundActionCollector, Task> addResult,
            OutboundActionCollector collector,
            CancellationToken cancellationToken )
        {
            if ( invoker != null && executionContext != null )
            {
#if DEBUG
                if ( !ReferenceEquals( executionContext.Compilation, compilation ) )
                {
                    throw new AssertionFailedException( "Execution context mismatch." );
                }
#endif

                return invoker.InvokeAsync( () => this._adder( compilation, ProcessTarget, collector, cancellationToken ), executionContext );
            }
            else
            {
                return this._adder( compilation, ProcessTarget, collector, cancellationToken );
            }

            Task ProcessTarget( T targetDeclaration, OutboundActionCollector collector, CancellationToken cancellationToken )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( targetDeclaration == null! )
                {
                    return Task.CompletedTask;
                }

                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                // Verify containment.
                var containingDeclaration = this._containingDeclaration.GetTarget( compilation ).AssertNotNull();

                if ( !(targetDeclaration.IsContainedIn( containingDeclaration )
                       || (containingDeclaration is IParameter p && p.DeclaringMember.Equals( targetDeclaration ))
                       || (containingDeclaration is IMember m && m.DeclaringType.Equals( targetDeclaration )))
                     || targetDeclaration.DeclaringAssembly.IsExternal )
                {
                    collector.Report(
                        GeneralDiagnosticDescriptors.CanAddChildAspectOnlyUnderParent.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, containingDeclaration) ) );

                    return Task.CompletedTask;
                }

                // Verify eligibility.
                var eligibility = aspectClass.GetEligibility( targetDeclaration );

                if ( filteredEligibility != EligibleScenarios.None && !eligibility.IncludesAny( filteredEligibility ) )
                {
                    return Task.CompletedTask;
                }

                var canBeInherited = ((IDeclarationImpl) targetDeclaration).CanBeInherited;
                var requiredEligibility = canBeInherited ? EligibleScenarios.Default | EligibleScenarios.Inheritance : EligibleScenarios.Default;

                if ( !eligibility.IncludesAny( requiredEligibility ) )
                {
                    var reason = aspectClass.GetIneligibilityJustification( requiredEligibility, new DescribedObject<IDeclaration>( targetDeclaration ) )!;

                    collector.Report(
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, reason) ) );

                    return Task.CompletedTask;
                }

                if ( invoker != null && executionContext != null )
                {
                    return invoker.InvokeAsync( () => addResult( targetDeclaration, collector ), executionContext );
                }
                else
                {
                    return addResult( targetDeclaration, collector );
                }
            }
        }

        private async Task SelectAndValidateValidatorOrConfiguratorTargetsAsync(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            CompilationModel compilation,
            DiagnosticDefinition<(FormattableString Predecessor, IDeclaration Child, IDeclaration Parent)> diagnosticDefinition,
            Func<T, OutboundActionCollector, Task> addAction,
            OutboundActionCollector collector,
            CancellationToken cancellationToken )
        {
            if ( invoker != null && executionContext != null )
            {
#if DEBUG
                if ( !ReferenceEquals( executionContext.Compilation, compilation ) )
                {
                    throw new AssertionFailedException( "Execution context mismatch." );
                }
#endif
                await invoker.InvokeAsync( () => this._adder( compilation, ProcessTarget, collector, cancellationToken ), executionContext );
            }
            else
            {
                await this._adder( compilation, ProcessTarget, collector, cancellationToken );
            }

            Task ProcessTarget( T targetDeclaration, OutboundActionCollector collector, CancellationToken cancellationToken )
            {
                if ( targetDeclaration == null! )
                {
                    return Task.CompletedTask;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                var containingTypeOrCompilation = (IDeclaration?) this._containingDeclaration.GetTarget( compilation ).AssertNotNull().GetTopmostNamedType()
                                                  ?? compilation;

                if ( (!targetDeclaration.IsContainedIn( containingTypeOrCompilation ) || targetDeclaration.DeclaringAssembly.IsExternal)
                     && containingTypeOrCompilation.DeclarationKind != DeclarationKind.Compilation )
                {
                    collector.Report(
                        diagnosticDefinition.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), targetDeclaration, containingTypeOrCompilation) ) );
                }

                return addAction( targetDeclaration, collector );
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
                    ( compilation, collector, cancellationToken ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, collector ),
                        compilation,
                        aspectClass,
                        EligibleScenarios.None,
                        ( declaration, collector ) =>
                        {
                            collector.AddAspectRequirement(
                                new AspectRequirement(
                                    declaration.ToTypedRef<IDeclaration>(),
                                    this._parent.AspectPredecessor.Instance ) );

                            return Task.CompletedTask;
                        },
                        collector,
                        cancellationToken ) ) );
        }
    }
}