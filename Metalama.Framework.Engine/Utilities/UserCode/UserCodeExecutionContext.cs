// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Project;
using System;
using System.Globalization;

namespace Metalama.Framework.Engine.Utilities.UserCode
{
    /// <summary>
    /// Represents the context of execution of compile-time user code, when this code does not have another
    /// "cleaner" way to get the context. Specifically, this is used to in the transformed expression of <c>typeof</c>.
    /// The current class is a service that must be registered and then disposed.
    /// </summary>
    internal class UserCodeExecutionContext : IExecutionContext
    {
        private readonly IDiagnosticAdder? _diagnosticAdder;
        private readonly IDependencyCollector? _dependencyCollector;
        private readonly INamedType? _targetType;
        private UserCodeMemberInfo? _invokedMember;
        private bool _collectDependencyDisabled;

        public static UserCodeExecutionContext Current => (UserCodeExecutionContext) MetalamaExecutionContext.Current ?? throw new InvalidOperationException();

        public static UserCodeExecutionContext? CurrentInternal => (UserCodeExecutionContext?) MetalamaExecutionContext.CurrentOrNull;

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
            CultureInfo.CurrentCulture = UserMessageFormatter.Instance;

            return new DisposeAction(
                () =>
                {
                    MetalamaExecutionContext.CurrentOrNull = oldContext;
                    CultureInfo.CurrentCulture = oldCulture;
                } );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCodeExecutionContext"/> class that can be used
        /// to invoke user code using <see cref="UserCodeInvoker.Invoke"/> but not <see cref="UserCodeInvoker.TryInvoke{T}"/>.
        /// </summary>
        public UserCodeExecutionContext(
            IServiceProvider serviceProvider,
            AspectLayerId? aspectAspectLayerId = null,
            CompilationModel? compilationModel = null,
            IDeclaration? targetDeclaration = null )
        {
            this.ServiceProvider = serviceProvider;
            this.AspectLayerId = aspectAspectLayerId;
            this.Compilation = compilationModel;
            this.TargetDeclaration = targetDeclaration;
            this._dependencyCollector = serviceProvider.GetService<IDependencyCollector>();
            this._targetType = targetDeclaration?.GetTopNamedType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCodeExecutionContext"/> class that can be used
        /// to invoke user code using <see cref="UserCodeInvoker.TryInvoke{T}"/>.
        /// </summary>
        public UserCodeExecutionContext(
            IServiceProvider serviceProvider,
            IDiagnosticAdder diagnostics,
            UserCodeMemberInfo invokedMember,
            AspectLayerId? aspectAspectLayerId = null,
            ICompilation? compilationModel = null,
            IDeclaration? targetDeclaration = null )
        {
            this.ServiceProvider = serviceProvider;
            this.AspectLayerId = aspectAspectLayerId;
            this.Compilation = compilationModel;
            this._diagnosticAdder = diagnostics;
            this.InvokedMember = invokedMember;
            this.TargetDeclaration = targetDeclaration;
            this._dependencyCollector = serviceProvider.GetService<IDependencyCollector>();
            this._targetType = targetDeclaration?.GetTopNamedType();
        }

        public IDiagnosticAdder Diagnostics
            => this._diagnosticAdder ?? throw new InvalidOperationException( "Cannot report diagnostics in a context without diagnostics adder." );

        // This property is intentionally writable because it allows us to reuse the same context for several calls, when performance
        // is critical. This feature is used by validators.
        public UserCodeMemberInfo InvokedMember
        {
            get => this._invokedMember ?? throw new InvalidOperationException( "Cannot report diagnostics in a context without invoked member." );
            set => this._invokedMember = value;
        }

        public IDeclaration? TargetDeclaration { get; }

        public IServiceProvider ServiceProvider { get; }

        public IFormatProvider FormatProvider => UserMessageFormatter.Instance;

        internal AspectLayerId? AspectLayerId { get; }

        public ICompilation? Compilation { get; }

        [Memo]
        public IExecutionScenario ExecutionScenario => this.ServiceProvider.GetRequiredService<ExecutionScenario>();

        ICompilation IExecutionContext.Compilation
            => this.Compilation ?? throw new InvalidOperationException( "There is no compilation in the current execution context" );

        public UserCodeExecutionContext WithInvokedMember( UserCodeMemberInfo invokedMember )
            => new(
                this.ServiceProvider,
                this.Diagnostics,
                invokedMember,
                this.AspectLayerId,
                this.Compilation,
                this.TargetDeclaration );

        public UserCodeExecutionContext WithDiagnosticAdder( IDiagnosticAdder diagnostics )
            => new(
                this.ServiceProvider,
                diagnostics,
                this.InvokedMember,
                this.AspectLayerId,
                this.Compilation,
                this.TargetDeclaration );

        public void AddDependency( IDeclaration declaration )
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
                    var declaringType = declaration.GetTopNamedType();

                    if ( declaringType != null && declaringType != this._targetType && !this._targetType.IsSubclassOf( declaringType ) )
                    {
                        this._dependencyCollector.AddDependency( this._targetType.GetSymbol(), declaringType.GetSymbol() );
                    }
                }
            }
            finally
            {
                this._collectDependencyDisabled = false;
            }
        }
    }
}