// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Metalama.Framework.Engine.Utilities.UserCode
{
    /// <summary>
    /// Represents the context of execution of compile-time user code, when this code does not have another
    /// "cleaner" way to get the context. Specifically, this is used to in the transformed expression of <c>typeof</c>.
    /// The current class is a service that must be registered and then disposed.
    /// </summary>
    public class UserCodeExecutionContext : IExecutionContextInternal
    {
        private readonly IDiagnosticAdder? _diagnosticAdder;
        private readonly bool _throwOnUnsupportedDependencies;
        private readonly IDependencyCollector? _dependencyCollector;
        private readonly INamedType? _targetType;
        private readonly ISyntaxBuilderImpl? _syntaxBuilder;
        private readonly CompilationContext? _compilationServices;
        private bool _collectDependencyDisabled;
        private UserCodeMemberInfo? _invokedMember;

        internal static UserCodeExecutionContext Current
            => (UserCodeExecutionContext) MetalamaExecutionContext.Current ?? throw new InvalidOperationException();

        internal static UserCodeExecutionContext? CurrentOrNull => (UserCodeExecutionContext?) MetalamaExecutionContext.CurrentOrNull;

        internal static Type ResolveCompileTimeTypeOf( string id, IReadOnlyDictionary<string, IType>? substitutions = null )
        {
            if ( Current._compilationServices == null )
            {
                throw new InvalidOperationException( "Using typeof for run-time types is not possible here." );
            }

            return Current._compilationServices.CompileTimeTypeFactory
                        .Get( new SerializableTypeId( id ), substitutions );
        }

        IDisposable IExecutionContext.WithoutDependencyCollection() => this.WithoutDependencyCollection();

        internal DisposeAction WithoutDependencyCollection()
        {
            if ( this._dependencyCollector == null )
            {
                return default;
            }
            else
            {
                var previousValue = this._collectDependencyDisabled;
                this._collectDependencyDisabled = true;

                return new DisposeAction( () => this._collectDependencyDisabled = previousValue );
            }
        }

        internal static DisposeAction WithContext( UserCodeExecutionContext? context )
        {
            if ( context == null )
            {
                return default;
            }

            var oldContext = MetalamaExecutionContext.CurrentOrNull;
            MetalamaExecutionContext.CurrentOrNull = context;
            var oldCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = MetalamaStringFormatter.Instance;

            return new DisposeAction(
                () =>
                {
                    MetalamaExecutionContext.CurrentOrNull = oldContext;
                    CultureInfo.CurrentCulture = oldCulture;
                } );
        }

        [PublicAPI]
        public static DisposeAction WithContext( ProjectServiceProvider serviceProvider, CompilationModel compilation )
            => WithContext( new UserCodeExecutionContext( serviceProvider, compilationModel: compilation ) );

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCodeExecutionContext"/> class that can be used
        /// to invoke user code using <see cref="UserCodeInvoker.Invoke"/> but not <see cref="UserCodeInvoker.TryInvoke{T}"/>.
        /// </summary>
        internal UserCodeExecutionContext(
            ProjectServiceProvider serviceProvider,
            AspectLayerId? aspectAspectLayerId = null,
            CompilationModel? compilationModel = null,
            IDeclaration? targetDeclaration = null,
            ISyntaxBuilderImpl? syntaxBuilder = null,
            MetaApi? metaApi = null )
        {
            this.ServiceProvider = serviceProvider;
            this.AspectLayerId = aspectAspectLayerId;
            this.Compilation = compilationModel;
            this._compilationServices = compilationModel?.CompilationContext;
            this.TargetDeclaration = targetDeclaration;
            this._dependencyCollector = serviceProvider.GetService<IDependencyCollector>();
            this._targetType = targetDeclaration?.GetTopmostNamedType();
            this._syntaxBuilder = GetSyntaxBuilder( compilationModel, syntaxBuilder );
            this.MetaApi = metaApi;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCodeExecutionContext"/> class that can be used
        /// to invoke user code using <see cref="UserCodeInvoker.TryInvoke{T}"/>.
        /// </summary>
        internal UserCodeExecutionContext(
            ProjectServiceProvider serviceProvider,
            IDiagnosticAdder diagnostics,
            UserCodeMemberInfo invokedMember,
            AspectLayerId? aspectAspectLayerId = null,
            CompilationModel? compilationModel = null,
            IDeclaration? targetDeclaration = null,
            bool throwOnUnsupportedDependencies = false,
            ISyntaxBuilderImpl? syntaxBuilder = null,
            MetaApi? metaApi = null,
            CompilationContext? compilationServices = null )
        {
            this.ServiceProvider = serviceProvider;
            this.AspectLayerId = aspectAspectLayerId;
            this.Compilation = compilationModel;
            this._diagnosticAdder = diagnostics;
            this._throwOnUnsupportedDependencies = throwOnUnsupportedDependencies;
            this.InvokedMember = invokedMember;
            this.TargetDeclaration = targetDeclaration;
            this._dependencyCollector = serviceProvider.GetService<IDependencyCollector>();
            this._targetType = targetDeclaration?.GetTopmostNamedType();

            this._compilationServices = compilationServices ?? compilationModel?.CompilationContext;

            this._syntaxBuilder = GetSyntaxBuilder( compilationModel, syntaxBuilder );
            this.MetaApi = metaApi;
        }

        internal UserCodeExecutionContext( UserCodeExecutionContext prototype )
        {
            this.ServiceProvider = prototype.ServiceProvider;
            this.AspectLayerId = prototype.AspectLayerId;
            this.Compilation = prototype.Compilation;
            this._diagnosticAdder = prototype._diagnosticAdder;
            this._throwOnUnsupportedDependencies = prototype._throwOnUnsupportedDependencies;
            this._invokedMember = prototype._invokedMember;
            this.TargetDeclaration = prototype.TargetDeclaration;
            this._dependencyCollector = prototype._dependencyCollector;
            this._targetType = prototype._targetType;
            this._compilationServices = prototype._compilationServices;
            this._syntaxBuilder = prototype._syntaxBuilder;
            this.MetaApi = prototype.MetaApi;
        }

        private static ISyntaxBuilderImpl? GetSyntaxBuilder(
            CompilationModel? compilationModel,
            ISyntaxBuilderImpl? syntaxBuilderImpl )
            => syntaxBuilderImpl ?? (compilationModel == null ? null : new SyntaxBuilderImpl( compilationModel ));

        private CompilationContext CompilationContext => this._compilationServices ?? throw new InvalidOperationException( "Compilation context is currently not available" );

        internal IDiagnosticAdder Diagnostics
            => this._diagnosticAdder ?? throw new InvalidOperationException( "Cannot report diagnostics in a context without diagnostics adder." );

        // This property is intentionally writable because it allows us to reuse the same context for several calls, when performance
        // is critical. This feature is used by validators.
        internal UserCodeMemberInfo InvokedMember
        {
            get => this._invokedMember ?? throw new InvalidOperationException( "Cannot report diagnostics in a context without invoked member." );
            set => this._invokedMember = value;
        }

        internal IDeclaration? TargetDeclaration { get; }

        internal ProjectServiceProvider ServiceProvider { get; }

        IServiceProvider<IProjectService> IExecutionContext.ServiceProvider => this.ServiceProvider.Underlying;

        public IFormatProvider FormatProvider => MetalamaStringFormatter.Instance;

        internal AspectLayerId? AspectLayerId { get; }

        internal CompilationModel? Compilation { get; }

        ISyntaxBuilderImpl? IExecutionContextInternal.SyntaxBuilder => this._syntaxBuilder;

        IMetaApi? IExecutionContextInternal.MetaApi => this.MetaApi;

        private protected MetaApi? MetaApi { get; }

        [Memo]
        public IExecutionScenario ExecutionScenario => this.ServiceProvider.GetRequiredService<ExecutionScenario>();

        ICompilation IExecutionContext.Compilation
            => this.Compilation ?? throw new InvalidOperationException( "There is no compilation in the current execution context" );

        internal UserCodeExecutionContext WithInvokedMember( UserCodeMemberInfo invokedMember )
            => new(
                this.ServiceProvider,
                this.Diagnostics,
                invokedMember,
                this.AspectLayerId,
                this.Compilation,
                this.TargetDeclaration,
                this._throwOnUnsupportedDependencies,
                this._syntaxBuilder,
                this.MetaApi );

        internal UserCodeExecutionContext WithCompilationAndDiagnosticAdder( CompilationModel compilation, IDiagnosticAdder diagnostics )
        {
            if ( ReferenceEquals( this.Compilation, compilation ) && diagnostics == this.Diagnostics )
            {
                return this;
            }

            return new UserCodeExecutionContext(
                this.ServiceProvider,
                diagnostics,
                this.InvokedMember,
                this.AspectLayerId,
                compilation,
                this.TargetDeclaration,
                this._throwOnUnsupportedDependencies,
                new SyntaxBuilderImpl( compilation ),
                this.MetaApi );
        }

        internal void AddDependency( IDeclaration declaration )
        {
            // Prevent infinite recursion while getting the declaring type.
            // We assume that there is one instance of this class per execution context and that it is single-threaded.

            if ( this._collectDependencyDisabled )
            {
                return;
            }

            this._collectDependencyDisabled = true;

            try
            {
                if ( this._dependencyCollector != null && this._targetType != null )
                {
                    var declaringType = declaration.GetTopmostNamedType();

                    if ( declaringType != null && declaringType != this._targetType )
                    {
                        this._dependencyCollector.AddDependency( declaringType.GetSymbol(), this._targetType.GetSymbol() );
                    }
                }
            }
            finally
            {
                this._collectDependencyDisabled = false;
            }
        }

        internal void OnUnsupportedDependency( string api )
        {
            if ( this._throwOnUnsupportedDependencies && this._dependencyCollector != null && !this._collectDependencyDisabled )
            {
                throw new InvalidOperationException(
                    $"'The '{api}' API is not supported in the BuildAspect context at design time. " +
                    $"It is only supported in the context of a adding new aspects ({nameof(IAspectReceiverSelector<IDeclaration>)}.{nameof(IAspectReceiverSelector<IDeclaration>.With)})'."
                    +
                    $"You can use {nameof(MetalamaExecutionContext)}.{nameof(MetalamaExecutionContext.Current)}.{nameof(IExecutionContext.ExecutionScenario)}.{nameof(IExecutionScenario.IsDesignTime)} to run your code at design time only." );
            }
        }
    }
}