// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.CodeFixes;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal class CodeFixProviderContext : ICodeFixProviderContext
    {
        private readonly ImmutableArray<CodeAction>.Builder _codeActions = ImmutableArray.CreateBuilder<CodeAction>();

        public Document OriginalDocument { get; }

        public CompilationModel OriginalCompilationModel { get; }

        public IServiceProvider ServiceProvider { get; }

        public CancellationToken CancellationToken { get; }

        public ImmutableArray<CodeAction> GetResult() => this._codeActions.ToImmutable();

        public CodeFixProviderContext(
            Document originalDocument,
            CompilationModel originalCompilationModel,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken )
        {
            this.OriginalDocument = originalDocument;
            this.OriginalCompilationModel = originalCompilationModel;
            this.ServiceProvider = serviceProvider;
            this.CancellationToken = cancellationToken;
        }

        public void AddCodeFix( string name, BuildCodeFixAsyncAction action )
            => this._codeActions.Add( CodeAction.Create( name, cancellationToken => this.ExecuteCodeFix( action, cancellationToken ) ) );

        private async Task<Solution> ExecuteCodeFix( BuildCodeFixAsyncAction action, CancellationToken cancellationToken )
        {
            var codeFixBuilder = new CodeFixBuilder( this, cancellationToken );

            // TODO: use user code invoker
            await action( codeFixBuilder );

            return await codeFixBuilder.GetResultingSolutionAsync();
        }
    }
}