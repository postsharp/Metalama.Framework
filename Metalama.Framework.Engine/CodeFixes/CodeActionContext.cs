// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        IPartialCompilation ISdkCodeActionContext.Compilation => this.Compilation;

        public ServiceProvider ServiceProvider => this.PipelineConfiguration.ServiceProvider;

        IServiceProvider ICodeActionContext.ServiceProvider => this.ServiceProvider;

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public CancellationToken CancellationToken { get; }

        public CodeActionContext(
            PartialCompilation compilation,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
        {
            this.Compilation = compilation;
            this.PipelineConfiguration = pipelineConfiguration;
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

        internal CodeActionResult ToCodeActionResult() => new( this._changedSyntaxTrees.Select( x => this.Compilation.SyntaxTrees[x] ).ToImmutableArray() );
    }
}