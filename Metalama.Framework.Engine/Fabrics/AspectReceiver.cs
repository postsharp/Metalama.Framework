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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        private readonly Func<Func<T, DeclarationSelectionContext, Task>, DeclarationSelectionContext, Task> _adder;
        private readonly IConcurrentTaskRunner _concurrentTaskRunner;

        // We track the number of children to know if we must cache results.
        private int _childrenCount;

        internal AspectReceiver(
            ISdkRef<IDeclaration> containingDeclaration,
            IAspectReceiverParent parent,
            CompilationModelVersion compilationModelVersion,
            Func<Func<T, DeclarationSelectionContext, Task>, DeclarationSelectionContext, Task> addTargets )
        {
            this._concurrentTaskRunner = parent.ServiceProvider.GetRequiredService<IConcurrentTaskRunner>();
            this._containingDeclaration = containingDeclaration;
            this._parent = parent;
            this._compilationModelVersion = compilationModelVersion;
            this._adder = addTargets;
        }

        protected virtual bool ShouldCache => this._childrenCount > 1;

        private AspectReceiver<TChild> AddChild<TChild>( AspectReceiver<TChild> child )
            where TChild : class, IDeclaration
        {
            this._childrenCount++;

            return child;
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
            this._childrenCount++;

            this._parent.LicenseVerifier?.VerifyCanAddChildAspect( this._parent.AspectPredecessor );

            this._parent.AddAspectSource( aspectSource );
        }

        private void RegisterValidatorSource( ProgrammaticValidatorSource validatorSource )
        {
            this._childrenCount++;

            this._parent.LicenseVerifier?.VerifyCanAddValidator( this._parent.AspectPredecessor );

            this._parent.AddValidatorSource( validatorSource );
        }

        private void RegisterOptionsSource( IHierarchicalOptionsSource hierarchicalOptionsSource )
        {
            this._childrenCount++;

            this._parent.AddOptionsSource( hierarchicalOptionsSource );
        }

        private Task SelectReferenceValidatorInstancesAsync(
            IDeclaration validatedDeclaration,
            ValidatorDriver driver,
            ValidatorImplementation implementation,
            ReferenceKinds referenceKinds,
            bool includeDerivedTypes,
            OutboundActionCollectionContext context )
        {
            var description = MetalamaStringFormatter.Format(
                $"reference validator for {validatedDeclaration.DeclarationKind} '{validatedDeclaration.ToDisplayString()}' provided by {this._parent.DiagnosticSourceDescription}" );

            context.Collector.AddValidator(
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
                        context.Collector.AddValidator(
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
                        context.Collector.AddValidator(
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
                    ( source, context ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        context,
                        ( item, context2 ) => this.SelectReferenceValidatorInstancesAsync(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( source.Predecessor.Instance ),
                            referenceKinds,
                            includeDerivedTypes,
                            context2 ) ) ) );
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
                    ( source, context ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        context,
                        ( item, c ) => this.SelectReferenceValidatorInstancesAsync(
                            item,
                            source.Driver,
                            ValidatorImplementation.Create( validator ),
                            validator.ValidatedReferenceKinds,
                            validator.IncludeDerivedTypes,
                            c ) ) ) );
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
                    ( source, context ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        context,
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
                    ( source, context ) => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                        context,
                        ( item, context2 ) =>
                        {
                            context2.Collector.AddValidator(
                                new DeclarationValidatorInstance(
                                    item,
                                    (ValidatorDriver<DeclarationValidationContext>) source.Driver,
                                    ValidatorImplementation.Create( source.Predecessor.Instance ),
                                    this._parent.DiagnosticSourceDescription ) );

                            return Task.CompletedTask;
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
            => this.AddChild( new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Final, this._adder ) );

        public IValidatorReceiver<T> BeforeAnyAspect()
            => this.AddChild( new AspectReceiver<T>( this._containingDeclaration, this._parent, CompilationModelVersion.Initial, this._adder ) );

        public IAspectReceiver<TMember> SelectMany<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => this.AddChild(
                new AspectReceiver<TMember>(
                    this._containingDeclaration,
                    this._parent,
                    this._compilationModelVersion,
                    ( action, context ) => this.InvokeAdderAsync(
                        context,
                        ( declaration, context2 ) =>
                        {
                            var children = selector( declaration );

                            return this._concurrentTaskRunner.RunInParallelAsync(
                                children,
                                child => action( child, context ),
                                context2.CancellationToken );
                        } ) ) );

        public IAspectReceiver<TMember> Select<TMember>( Func<T, TMember> selector )
            where TMember : class, IDeclaration
            => this.AddChild(
                new AspectReceiver<TMember>(
                    this._containingDeclaration,
                    this._parent,
                    this._compilationModelVersion,
                    ( action, context ) => this.InvokeAdderAsync(
                        context,
                        ( declaration, context2 ) => action( selector( declaration ), context2 ) ) ) );

        public IAspectReceiver<T> Where( Func<T, bool> predicate )
            => this.AddChild(
                new AspectReceiver<T>(
                    this._containingDeclaration,
                    this._parent,
                    this._compilationModelVersion,
                    ( action, context ) => this.InvokeAdderAsync(
                        context,
                        ( declaration, context2 ) =>
                        {
                            if ( predicate( declaration ) )
                            {
                                return action( declaration, context2 );
                            }
                            else
                            {
                                return Task.CompletedTask;
                            }
                        } ) ) );

        public void SetOptions<TOptions>( Func<T, TOptions> func )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( context )
                        => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            context,
                            ( declaration, context2 ) =>
                            {
                                if ( userCodeInvoker.TryInvoke(
                                        () => func( declaration ),
                                        executionContext,
                                        out var options ) )
                                {
                                    context2.Collector.AddOptions(
                                        new HierarchicalOptionsInstance(
                                            declaration,
                                            options ) );
                                }

                                return Task.CompletedTask;
                            } ) ) );
        }

        public void SetOptions<TOptions>( TOptions options )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<T>, new()
        {
            var userCodeInvoker = this._parent.ServiceProvider.GetRequiredService<UserCodeInvoker>();
            var executionContext = UserCodeExecutionContext.Current;

            this.RegisterOptionsSource(
                new ProgrammaticHierarchicalOptionsSource(
                    ( context )
                        => this.SelectAndValidateValidatorOrConfiguratorTargetsAsync(
                            userCodeInvoker,
                            executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                            GeneralDiagnosticDescriptors.CanAddValidatorOnlyUnderParent,
                            context,
                            ( declaration, context2 ) =>
                            {
                                context2.Collector.AddOptions(
                                    new HierarchicalOptionsInstance(
                                        declaration,
                                        options ) );

                                return Task.CompletedTask;
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
                    ( context ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        aspectClass,
                        eligibility,
                        context,
                        ( t, context2 ) =>
                        {
                            if ( userCodeInvoker.TryInvoke(
                                    () => createAspect( t ),
                                    executionContext,
                                    out var aspect ) )
                            {
                                context2.Collector.AddAspectInstance(
                                    new AspectInstance(
                                        aspect,
                                        t,
                                        aspectClass,
                                        this._parent.AspectPredecessor ) );
                            }

                            return Task.CompletedTask;
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
                    ( context ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        aspectClass,
                        eligibility,
                        context,
                        ( t, context2 ) =>
                        {
                            if ( userCodeInvoker.TryInvoke(
                                    () => new TAspect(),
                                    executionContext,
                                    out var aspect ) )
                            {
                                context2.Collector.AddAspectInstance(
                                    new AspectInstance(
                                        aspect,
                                        t,
                                        aspectClass,
                                        this._parent.AspectPredecessor ) );
                            }

                            return Task.CompletedTask;
                        } ) ) );
        }

        private async Task InvokeAdderAsync(
            DeclarationSelectionContext selectionContext,
            Func<T, DeclarationSelectionContext, Task> processTarget,
            UserCodeInvoker? invoker = null,
            UserCodeExecutionContext? executionContext = null )
        {
            ConcurrentBag<T> cached = null;
            var processTargetWithCachingIfNecessary = processTarget;

            if ( this.ShouldCache )
            {
                // GetFromCacheAsync uses a semaphore to control exclusivity.  AddToCache must be called is the method returns null.
                cached = await selectionContext.GetFromCacheAsync<ConcurrentBag<T>>( this, selectionContext.CancellationToken );

                if ( cached != null )
                {
                    await this._concurrentTaskRunner.RunInParallelAsync(
                        cached,
                        x => processTarget( x, selectionContext ),
                        selectionContext.CancellationToken );
                }
                else
                {
                    cached = new ConcurrentBag<T>();

                    processTargetWithCachingIfNecessary = ( a, ctx ) =>
                    {
                        cached.Add( a );

                        return processTarget( a, ctx );
                    };
                }
            }

            if ( invoker != null && executionContext != null )
            {
#if DEBUG
                if ( !ReferenceEquals( executionContext.Compilation, selectionContext.Compilation ) )
                {
                    throw new AssertionFailedException( "Execution context mismatch." );
                }
#endif

                await invoker.InvokeAsync( () => this._adder( processTargetWithCachingIfNecessary, selectionContext ), executionContext );
            }
            else
            {
                await this._adder( processTargetWithCachingIfNecessary, selectionContext );
            }

            if ( this.ShouldCache )
            {
                selectionContext.AddToCache( this, cached );
            }
        }

        private async Task SelectAndValidateAspectTargetsAsync(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            AspectClass aspectClass,
            EligibleScenarios filteredEligibility,
            OutboundActionCollectionContext context,
            Func<T, OutboundActionCollectionContext, Task> addResult )
        {
            var compilation = context.Compilation;

            await this.InvokeAdderAsync( context, ProcessTarget, invoker, executionContext );

            Task ProcessTarget( T targetDeclaration, DeclarationSelectionContext context2 )
            {
                context2.CancellationToken.ThrowIfCancellationRequested();

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
                    context.Collector.Report(
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

                    context.Collector.Report(
                        GeneralDiagnosticDescriptors.IneligibleChildAspect.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), aspectClass.ShortName, targetDeclaration, reason) ) );

                    return Task.CompletedTask;
                }

                if ( invoker != null && executionContext != null )
                {
                    return invoker.InvokeAsync( () => addResult( targetDeclaration, context ), executionContext );
                }
                else
                {
                    return addResult( targetDeclaration, context );
                }
            }
        }

        private async Task SelectAndValidateValidatorOrConfiguratorTargetsAsync(
            UserCodeInvoker? invoker,
            UserCodeExecutionContext? executionContext,
            DiagnosticDefinition<(FormattableString Predecessor, IDeclaration Child, IDeclaration Parent)> diagnosticDefinition,
            OutboundActionCollectionContext context,
            Func<T, OutboundActionCollectionContext, Task> addAction )
        {
            var compilation = context.Compilation;

            await this.InvokeAdderAsync( context, ProcessTarget, invoker, executionContext );

            Task ProcessTarget( T targetDeclaration, DeclarationSelectionContext context2 )
            {
                if ( targetDeclaration == null! )
                {
                    return Task.CompletedTask;
                }

                context2.CancellationToken.ThrowIfCancellationRequested();

                var predecessorInstance = (IAspectPredecessorImpl) this._parent.AspectPredecessor.Instance;

                var containingTypeOrCompilation = (IDeclaration?) this._containingDeclaration.GetTarget( compilation ).AssertNotNull().GetTopmostNamedType()
                                                  ?? compilation;

                if ( (!targetDeclaration.IsContainedIn( containingTypeOrCompilation ) || targetDeclaration.DeclaringAssembly.IsExternal)
                     && containingTypeOrCompilation.DeclarationKind != DeclarationKind.Compilation )
                {
                    context.Collector.Report(
                        diagnosticDefinition.CreateRoslynDiagnostic(
                            predecessorInstance.GetDiagnosticLocation( compilation.RoslynCompilation ),
                            (predecessorInstance.FormatPredecessor( compilation ), targetDeclaration, containingTypeOrCompilation) ) );
                }

                return addAction( targetDeclaration, context );
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
                    ( context ) => this.SelectAndValidateAspectTargetsAsync(
                        userCodeInvoker,
                        executionContext.WithCompilationAndDiagnosticAdder( context.Compilation, context.Collector ),
                        aspectClass,
                        EligibleScenarios.None,
                        context,
                        ( declaration, context2 ) =>
                        {
                            context2.Collector.AddAspectRequirement(
                                new AspectRequirement(
                                    declaration.ToTypedRef<IDeclaration>(),
                                    this._parent.AspectPredecessor.Instance ) );

                            return Task.CompletedTask;
                        } ) ) );
        }
    }
}