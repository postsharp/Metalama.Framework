// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.CodeFixes;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    public class UserCodeFix
    {
        private readonly Action<ICodeFixProviderContext> _provider;

        public IDiagnosticDefinition DiagnosticDefinition { get; }

        public Location Location { get; }

        internal UserCodeFix( IDiagnosticDefinition diagnosticDefinition, Location location, Action<ICodeFixProviderContext> provider )
        {
            this.DiagnosticDefinition = diagnosticDefinition;
            this.Location = location;
            this._provider = provider;
        }

        internal ImmutableArray<CodeAction> CreateCodeActions(
            Document document,
            CompilationModel compilationModel,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken )
        {
            var context = new CodeFixProviderContext( document, compilationModel, serviceProvider, cancellationToken );
            this._provider( context );

            return context.GetResult();
        }
    }
}