// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    
    internal class UserCodeFixProvider
    {
        private readonly CodeFixRunner _codeFixRunner;

        internal UserCodeFixProvider( DesignTimeAspectPipelineFactory designTimeAspectPipelineFactory, IProjectOptions projectOptions )
        {
            this._codeFixRunner = new CodeFixRunner( designTimeAspectPipelineFactory, projectOptions );
        }

        public UserCodeFixProvider( IProjectOptions projectOptions) : this( DesignTimeAspectPipelineFactory.Instance, projectOptions ) { }

        public ImmutableArray<AssignedCodeFix> ProvideCodeFixes(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken )
        {
            var codeFixesBuilder = ImmutableArray.CreateBuilder<AssignedCodeFix>();

            foreach ( var diagnostic in diagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( diagnostic.Properties.TryGetValue( CodeFixTitles.DiagnosticPropertyKey, out var codeFixTitles ) &&
                     !string.IsNullOrEmpty( codeFixTitles ) )
                {
                    var splitTitles = codeFixTitles!.Split( CodeFixTitles.Separator );

                    foreach (var codeFixTitle in splitTitles)
                    {
                        // TODO: We may support hierarchical code fixes by allowing a separator in the title given by the user, i.e. '|'.
                        // The creation of the tree structure would then be done here.

                        var title = codeFixTitle;

                        var codeAction = CodeAction.Create(
                            codeFixTitle,
                            ct => this._codeFixRunner.ExecuteCodeFixAsync( document, diagnostic, title, cancellationToken ) );

                        codeFixesBuilder.Add( new AssignedCodeFix( codeAction, ImmutableArray.Create( diagnostic ) ) );
                    }
                }
            }

            return codeFixesBuilder.ToImmutable();
        }

 
    }
}