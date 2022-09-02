// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
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

        private UserCodeMemberInfo? _invokedMember;

        private IExecutionScenario? _executionScenario;

        public static UserCodeExecutionContext Current => (UserCodeExecutionContext) MetalamaExecutionContext.Current ?? throw new InvalidOperationException();

        public static UserCodeExecutionContext? CurrentInternal => (UserCodeExecutionContext?) MetalamaExecutionContext.CurrentOrNull;

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

        public IExecutionScenario ExecutionScenario
            => this._executionScenario ??= this.ServiceProvider.GetRequiredService<AspectPipelineDescription>().ExecutionScenario;

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
    }
}