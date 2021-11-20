// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
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
            var project = document.Project;
            var compilation = await project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return default;
            }

            var syntaxTree = await document.GetSyntaxTreeAsync( cancellationToken );

            if ( syntaxTree == null )
            {
                return default;
            }

            // Get the pipeline for the compilation.
            if ( !this._designTimeAspectPipelineFactory.TryGetPipeline( compilation, out var designTimePipeline ) )
            {
                // We cannot create the pipeline because we don't have all options.
                // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

                return default;
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
                return default;
            }

            // Execute the compile-time pipeline with the design-time project configuration.
            var codeFixPipeline = new CodeFixPipeline(
                designTimeConfiguration.ServiceProvider,
                false,
                this._designTimeAspectPipelineFactory.Domain,
                syntaxTree,
                span );

            if ( !codeFixPipeline.TryExecute(
                partialCompilation,
                designTimeConfiguration,
                cancellationToken,
                out var userCodeFixes,
                out var compilationModel ) )
            {
                return default;
            }

            var codeFixesBuilder = ImmutableArray.CreateBuilder<AssignedCodeFix>();

            foreach ( var codeFix in userCodeFixes )
            {
                // Get the diagnostics to which that diagnostic apply. We don't need to check the syntax tree because the pipeline has filtered anyway.
                var applicableDiagnostics = diagnostics
                    .Where( d => d.Id == codeFix.DiagnosticDefinition.Id && d.Location.SourceSpan.Equals( codeFix.Location.SourceSpan ) )
                    .ToImmutableArray();

                foreach ( var codeAction in codeFix.CreateCodeActions(
                    document,
                    compilationModel,
                    designTimeConfiguration.ServiceProvider,
                    cancellationToken ) )
                {
                    codeFixesBuilder.Add( new AssignedCodeFix( codeAction, applicableDiagnostics ) );
                }
            }

            return codeFixesBuilder.ToImmutable();
        }
    }
}