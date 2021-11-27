// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Workspaces
{
    public sealed class DiagnosticModel
    {
        private readonly Diagnostic _diagnostic;

        public ICompilation Compilation { get; }

        public DiagnosticModel( Diagnostic diagnostic, ICompilation compilation )
        {
            this._diagnostic = diagnostic;
            this.Compilation = compilation;
        }

        public string Id => this._diagnostic.Id;

        public string Message => this._diagnostic.GetMessage();

        public string? FilePath => this._diagnostic.Location.SourceTree?.FilePath;

        public int? Line => this._diagnostic.Location.GetLineSpan().StartLinePosition.Line;

        [Memo]
        public IDeclaration? Declaration => this.GetDeclaration();

        public Severity Severity => this._diagnostic.Severity.ToOurSeverity();

        private IDeclaration? GetDeclaration()
        {
            // Find the node reporting the error.
            var sourceTree = this._diagnostic.Location.SourceTree;

            if ( sourceTree == null )
            {
                return null;
            }

            var syntaxRoot = sourceTree.GetRoot();
            var node = syntaxRoot.FindNode( this._diagnostic.Location.SourceSpan );

            // Map to a symbol.
            var compilation = this.Compilation.GetRoslynCompilation();
            var semanticModel = compilation.GetSemanticModel( sourceTree );

            for ( var n = node; n != null; n = n.Parent )
            {
                var symbol = semanticModel.GetDeclaredSymbol( n );

                if ( symbol != null )
                {
                    if ( this.Compilation.TryGetDeclaration( symbol, out var declaration ) )
                    {
                        return declaration;
                    }
                }
            }

            return null;
        }
    }
}