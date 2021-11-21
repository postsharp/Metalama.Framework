using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    // This class needs to be public because it is used by the public test runner.
    public sealed class CodeFixRunner
    {
        private readonly DesignTimeAspectPipelineFactory _designTimeAspectPipelineFactory;
        private readonly IProjectOptions _projectOptions;

        // Constructor for the test framework.
        public CodeFixRunner( CompileTimeDomain domain, IProjectOptions projectOptions )
        {
            this._designTimeAspectPipelineFactory = new TestDesignTimeAspectPipelineFactory( domain, projectOptions );
            this._projectOptions = projectOptions;
        }
        
        internal CodeFixRunner( DesignTimeAspectPipelineFactory designTimeAspectPipelineFactory, IProjectOptions projectOptions )
        {
            this._designTimeAspectPipelineFactory = designTimeAspectPipelineFactory;
            this._projectOptions = projectOptions;
        }

        public async Task<Solution> ExecuteCodeFixAsync(
            Document document,
            Diagnostic diagnostic,
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

            if ( userCodeFixes.IsDefaultOrEmpty )
            {
                // The pipeline did not generate any code fix, which is unexpected.
                return project.Solution;
            }

            var codeFix = userCodeFixes.FirstOrDefault( f => f.CodeFix.Title == codeFixTitle );

            if ( codeFix == null )
            {
                // There was some mismatch in the generation of code fixes.
                // Note that we are not matching the code fixes by position in the array because there may be several diagnostics
                // of the same id on the same span, and we would not be able to differentiate these instances and therefore the ids.
                // In theory, the aspect author could provide different code fixes with the same title for the same diagnostic,
                // but in this case this would also be confusing for the end user.
                return project.Solution;
            }

            var context = new CodeFixContext( document, compilationModel, this._designTimeAspectPipelineFactory,this._projectOptions,designTimeConfiguration );
            var codeFixBuilder = new CodeFixBuilder( context, cancellationToken );

            // TODO: use user code invoker
            await codeFix.CodeFix.Action( codeFixBuilder );

            return await codeFixBuilder.GetResultingSolutionAsync();
        }        
    }
}