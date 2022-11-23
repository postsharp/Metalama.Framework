// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Exposes objects required by <see cref="CodeActionBuilder"/>.
    /// </summary>
    internal sealed class CodeActionContext : ISdkCodeActionContext
    {
        private readonly HashSet<string> _changedSyntaxTrees = new();

        public PartialCompilation Compilation { get; private set; }

        public CompilationContext CompilationContext { get; }

        IPartialCompilation ISdkCodeActionContext.Compilation => this.Compilation;

        public ProjectServiceProvider ServiceProvider => this.CompilationContext.ServiceProvider;

        IServiceProvider<IProjectService> ICodeActionContext.ServiceProvider => this.ServiceProvider.Underlying;

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public bool IsComputingPreview { get; set; }

        public CancellationToken CancellationToken { get; }

        public CodeActionContext(
            PartialCompilation compilation,
            CompilationContext compilationContext,
            AspectPipelineConfiguration pipelineConfiguration,
            bool isComputingPreview,
            CancellationToken cancellationToken )
        {
            this.Compilation = compilation;
            this.CompilationContext = compilationContext;
            this.PipelineConfiguration = pipelineConfiguration ?? throw new ArgumentNullException();
            this.IsComputingPreview = isComputingPreview;
            this.CancellationToken = cancellationToken;
        }

        public void UpdateTree( SyntaxTree transformedTree, SyntaxTree originalTree )
        {
            this.Compilation = this.Compilation.Update( new[] { SyntaxTreeTransformation.ReplaceTree( originalTree, transformedTree ) } );
            this._changedSyntaxTrees.Add( originalTree.FilePath );
        }

        public void UpdateTree( SyntaxNode transformedRoot, SyntaxTree originalTree )
        {
            var transformedTree = CSharpSyntaxTree.Create(
                (CSharpSyntaxNode) transformedRoot,
                (CSharpParseOptions?) originalTree.Options,
                originalTree.FilePath,
                originalTree.Encoding );

            this.UpdateTree( transformedTree, originalTree );
        }

        void ISdkCodeActionContext.ApplyModifications( IPartialCompilation compilation ) => this.ApplyModifications( (PartialCompilation) compilation );

        public void ApplyModifications( PartialCompilation compilation )
        {
            this.Compilation = this.Compilation.Update( compilation.ModifiedSyntaxTrees.Values.Where( x => x.OldTree != null ).ToList() );

            foreach ( var modifiedPath in compilation.ModifiedSyntaxTrees.Keys )
            {
                this._changedSyntaxTrees.Add( modifiedPath );
            }
        }

        internal CodeActionResult ToCodeActionResult()
            => CodeActionResult.Success( this._changedSyntaxTrees.SelectImmutableArray( x => this.Compilation.SyntaxTrees[x] ) );
    }
}