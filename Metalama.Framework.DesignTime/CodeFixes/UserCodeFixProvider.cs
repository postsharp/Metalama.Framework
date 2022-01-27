// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes
{
    /// <summary>
    /// A service used by <see cref="CentralCodeFixProvider"/> to handle user-defined code fixes.
    /// </summary>
    internal class UserCodeFixProvider
    {
        private readonly CodeFixRunner _codeFixRunner;

        internal UserCodeFixProvider( DesignTimeAspectPipelineFactory designTimeAspectPipelineFactory, IProjectOptions projectOptions )
        {
            this._codeFixRunner = new DesignTimeCodeFixRunner( designTimeAspectPipelineFactory, projectOptions );
        }

        public UserCodeFixProvider( IProjectOptions projectOptions ) : this( DesignTimeAspectPipelineFactory.Instance, projectOptions ) { }

        public ImmutableArray<CodeFixModel> ProvideCodeFixes(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken )
        {
            var codeFixesBuilder = ImmutableArray.CreateBuilder<CodeFixModel>();

            foreach ( var diagnostic in diagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( diagnostic.Properties.TryGetValue( CodeFixTitles.DiagnosticPropertyKey, out var codeFixTitles ) &&
                     !string.IsNullOrEmpty( codeFixTitles ) )
                {
                    var splitTitles = codeFixTitles!.Split( CodeFixTitles.Separator );

                    foreach ( var codeFixTitle in splitTitles )
                    {
                        // TODO: We may support hierarchical code fixes by allowing a separator in the title given by the user, i.e. '|'.
                        // The creation of the tree structure would then be done here.

                        var title = codeFixTitle;

                        var codeAction = new CodeActionModel(
                            codeFixTitle,
                            ct => this._codeFixRunner.ExecuteCodeFixAsync( document, diagnostic, title, ct ) );

                        codeFixesBuilder.Add( new CodeFixModel( codeAction, ImmutableArray.Create( diagnostic ) ) );
                    }
                }
            }

            return codeFixesBuilder.ToImmutable();
        }
    }
}