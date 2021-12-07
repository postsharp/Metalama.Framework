// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using System;
using System.Globalization;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Represents the context of execution of compile-time user code, when this code does not have another
    /// "cleaner" way to get the context. Specifically, this is used to in the transformed expression of <c>typeof</c>.
    /// The current class is a service that must be registered and then disposed.
    /// </summary>
    internal class UserCodeExecutionContext : IExecutionContext
    {
        private IExecutionScenario? _executionScenario;

        public static UserCodeExecutionContext Current => (UserCodeExecutionContext) CaravelaExecutionContext.Current ?? throw new InvalidOperationException();

        public static UserCodeExecutionContext? CurrentInternal => (UserCodeExecutionContext?) CaravelaExecutionContext.CurrentOrNull;

        internal static DisposeAction WithContext( UserCodeExecutionContext context )
        {
            var oldContext = CaravelaExecutionContext.CurrentOrNull;
            CaravelaExecutionContext.CurrentOrNull = context;
            var oldCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = UserMessageFormatter.Instance;

            return new DisposeAction(
                () =>
                {
                    CaravelaExecutionContext.CurrentOrNull = oldContext;
                    CultureInfo.CurrentCulture = oldCulture;
                } );
        }

        public IDiagnosticAdder Diagnostics { get; }

        public UserCodeMemberInfo InvokedMember { get; }

        public IDeclaration? TargetDeclaration { get; }

        public IServiceProvider ServiceProvider { get; }

        public IFormatProvider FormatProvider => UserMessageFormatter.Instance;

        internal AspectLayerId? AspectLayerId { get; }

        public CompilationModel? Compilation { get; }

        public IExecutionScenario ExecutionScenario
            => this._executionScenario ??= this.ServiceProvider.GetService<AspectPipelineDescription>().ExecutionScenario;

        ICompilation IExecutionContext.Compilation
            => this.Compilation ?? throw new InvalidOperationException( "There is no compilation in the current execution context" );

        public UserCodeExecutionContext(
            IServiceProvider serviceProvider,
            IDiagnosticAdder diagnostics,
            UserCodeMemberInfo invokedMember,
            AspectLayerId? aspectAspectLayerId = null,
            CompilationModel? compilationModel = null,
            IDeclaration? targetDeclaration = null )
        {
            this.ServiceProvider = serviceProvider;
            this.AspectLayerId = aspectAspectLayerId;
            this.Compilation = compilationModel;
            this.Diagnostics = diagnostics;
            this.InvokedMember = invokedMember;
            this.TargetDeclaration = targetDeclaration;
        }

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