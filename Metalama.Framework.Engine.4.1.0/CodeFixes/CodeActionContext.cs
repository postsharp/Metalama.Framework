// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            this.Compilation = this.Compilation.Update( new[] { new SyntaxTreeModification( transformedTree, originalTree ) } );
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
            this.Compilation = this.Compilation.Update(
                compilation.ModifiedSyntaxTrees.Values.Where( x => x.OldTree != null ).ToList(),
                compilation.ModifiedSyntaxTrees.Values.Where( x => x.OldTree == null ).Select( x => x.NewTree ).ToList() );

            foreach ( var modifiedPath in compilation.ModifiedSyntaxTrees.Keys )
            {
                this._changedSyntaxTrees.Add( modifiedPath );
            }
        }

        internal CodeActionResult ToCodeActionResult() => new( this._changedSyntaxTrees.Select( x => this.Compilation.SyntaxTrees[x] ).ToImmutableArray() );
    }
}