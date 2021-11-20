// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal class UserCodeFixProvider
    {
        private readonly DesignTimeAspectPipelineFactory _designTimeAspectPipelineFactory;

        internal UserCodeFixProvider( DesignTimeAspectPipelineFactory designTimeAspectPipelineFactory )
        {
            this._designTimeAspectPipelineFactory = designTimeAspectPipelineFactory;
        }

        public UserCodeFixProvider() : this( DesignTimeAspectPipelineFactory.Instance ) { }

        public async Task<ImmutableArray<AssignedCodeFix>> ProvideCodeFixesAsync(
            Document document,
            TextSpan span,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken )
        {
            var codeFixesBuilder = ImmutableArray.CreateBuilder<AssignedCodeFix>();

            foreach ( var diagnostic in diagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( diagnostic.Properties.TryGetValue( DiagnosticDescriptorExtensions.CodeFixesDiagnosticPropertyKey, out var codeFixTitles ) &&
                     !string.IsNullOrEmpty( codeFixTitles ) )
                {
                    var splitTitles = codeFixTitles!.Split( '\n' );

                    for ( var i = 0; i < splitTitles.Length; i++ )
                    {
                        var codeFixId = i;
                        var codeFixTitle = splitTitles[i];
                        
                        // TODO: We may support hierarchical code fixes by allowing a separator in the title given by the user, i.e. '|'.
                        // The creation of the tree structure would then be done here.

                        var codeAction = CodeAction.Create(
                            codeFixTitle,
                            ct => this.ExecuteCodeFixAsync( document, span, diagnostic, codeFixId, codeFixTitle, cancellationToken ) );

                        codeFixesBuilder.Add( new AssignedCodeFix( codeAction, ImmutableArray.Create( diagnostic ) ) );
                    }
                }
            }

            return codeFixesBuilder.ToImmutable();
        }

        private async Task<Solution> ExecuteCodeFixAsync(
            Document document,
            TextSpan span,
            Diagnostic diagnostic,
            int codeFixId,
            string codeFixTitle,
            CancellationToken cancellationToken )
        {
            var project = document.Project;
            var compilation = await project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return project.Solution;
            }

            var syntaxTree = await document.GetSyntaxTreeAsync( cancellationToken );

            if ( syntaxTree == null )
            {
                return project.Solution;
            }

            // Get the pipeline for the compilation.
            if ( !this._designTimeAspectPipelineFactory.TryGetPipeline( compilation, out var designTimePipeline ) )
            {
                // We cannot create the pipeline because we don't have all options.
                // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

                return project.Solution;
            }

            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = compilation.SyntaxTrees.Where( CompilationChangeTracker.IsGeneratedFile );
            var sourceCompilation = compilation.RemoveSyntaxTrees( generatedFiles );

            var partialCompilation = PartialCompilation.CreatePartial( sourceCompilation, syntaxTree );

            // Get the pipeline configuration from the design-time pipeline.
            if ( !designTimePipeline.TryGetConfiguration(
                partialCompilation,
                NullDiagnosticAdder.Instance,
                true,
                cancellationToken,
                out var designTimeConfiguration ) )
            {
                return project.Solution;
            }

            // Execute the compile-time pipeline with the design-time project configuration.
            var codeFixPipeline = new CodeFixPipeline(
                designTimeConfiguration.ServiceProvider,
                false,
                this._designTimeAspectPipelineFactory.Domain,
                syntaxTree,
                span,
                diagnostic );

            if ( !codeFixPipeline.TryExecute(
                partialCompilation,
                designTimeConfiguration,
                cancellationToken,
                out var userCodeFixes,
                out var compilationModel ) )
            {
                return project.Solution;
            }

            if ( codeFixId >= userCodeFixes.Length )
            {
                // There was some mismatch in the generation of code fixes. 
                return project.Solution;
            }

            var codeFix = userCodeFixes[codeFixId];

            if ( codeFix.CodeFix.Title != codeFixTitle )
            {
                // There was some mismatch in the generation of code fixes. 
                return project.Solution;
            }

            var context = new CodeFixContext( document, compilationModel, designTimeConfiguration.ServiceProvider );
            var codeFixBuilder = new CodeFixBuilder( context, cancellationToken );

            // TODO: use user code invoker
            await codeFix.CodeFix.Action( codeFixBuilder );

            return await codeFixBuilder.GetResultingSolutionAsync();
        }
    }
}