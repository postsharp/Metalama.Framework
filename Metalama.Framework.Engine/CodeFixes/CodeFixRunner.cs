// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using ProjectServiceProvider = Metalama.Framework.Engine.Services.ProjectServiceProvider;

namespace Metalama.Framework.Engine.CodeFixes
{
    // This class needs to be public because it is used by the public test runner.

    /// <summary>
    /// Executes code fixes.
    /// </summary>
    public abstract class CodeFixRunner
    {
        private readonly UserCodeInvoker _userCodeInvoker;

        protected CodeFixRunner( ProjectServiceProvider serviceProvider )
        {
            this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
        }

        private protected abstract
            ValueTask<(bool Success, AspectPipelineConfiguration? Configuration, ProjectServiceProvider? ServiceProvider, CompileTimeDomain? Domain)>
            GetConfigurationAsync(
                PartialCompilation compilation,
                TestableCancellationToken cancellationToken );

        public async Task<CodeActionResult> ExecuteCodeFixAsync(
            Document document,
            Diagnostic diagnostic,
            string codeFixTitle,
            bool isComputingPreview,
            TestableCancellationToken cancellationToken = default )
        {
            var project = document.Project;
            var compilation = await project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return CodeActionResult.Empty;
            }

            var syntaxTree = await document.GetSyntaxTreeAsync( cancellationToken );

            if ( syntaxTree == null )
            {
                return CodeActionResult.Empty;
            }

            return await this.ExecuteCodeFixAsync(
                compilation,
                syntaxTree,
                diagnostic.Id,
                diagnostic.Location.SourceSpan,
                codeFixTitle,
                isComputingPreview,
                cancellationToken );
        }

        public async Task<CodeActionResult> ExecuteCodeFixAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            string diagnosticId,
            TextSpan diagnosticSpan,
            string codeFixTitle,
            bool isComputingPreview,
            TestableCancellationToken cancellationToken )
        {
            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = compilation.SyntaxTrees.Where( SourceGeneratorHelper.IsGeneratedFile );
            var sourceCompilation = compilation.RemoveSyntaxTrees( generatedFiles );

            var partialCompilation = PartialCompilation.CreatePartial( sourceCompilation, syntaxTree );

            // Get the pipeline configuration.
            var configuration = await this.GetConfigurationAsync( partialCompilation, cancellationToken );

            if ( !configuration.Success )
            {
                // We cannot get the pipeline configuration.

                return CodeActionResult.Empty;
            }

            // Execute the compile-time pipeline with the design-time project configuration.
            var codeFixPipeline = new CodeFixPipeline(
                configuration.ServiceProvider!.Value,
                configuration.Domain!,
                diagnosticId,
                syntaxTree.FilePath,
                diagnosticSpan );

            var compilationServices = configuration.ServiceProvider!.Value.GetRequiredService<CompilationServicesFactory>()
                .GetInstance( partialCompilation.Compilation );

            var designTimeConfiguration = configuration.Configuration;

            var pipelineResult = await codeFixPipeline.ExecuteAsync(
                partialCompilation,
                designTimeConfiguration,
                cancellationToken );

            if ( !pipelineResult.IsSuccessful )
            {
                return CodeActionResult.Error( pipelineResult.Diagnostics );
            }

            var userCodeFixes = pipelineResult.Value.CodeFixes;

            if ( userCodeFixes.IsDefaultOrEmpty )
            {
                // The pipeline did not generate any code fix, which is unexpected.
                return CodeActionResult.Error( GeneralDiagnosticDescriptors.CannotFindCodeFix.CreateRoslynDiagnostic( null, codeFixTitle ) );
            }

            var codeFix = userCodeFixes.FirstOrDefault( f => f.CodeFix.Title == codeFixTitle );

            if ( codeFix == null )
            {
                // There was some mismatch in the generation of code fixes.
                // Note that we are not matching the code fixes by position in the array because there may be several diagnostics
                // of the same id on the same span, and we would not be able to differentiate these instances and therefore the ids.
                // In theory, the aspect author could provide different code fixes with the same title for the same diagnostic,
                // but in this case this would also be confusing for the end user.
                return CodeActionResult.Error( GeneralDiagnosticDescriptors.CannotFindCodeFix.CreateRoslynDiagnostic( null, codeFixTitle ) );
            }
            else if ( !codeFix.IsLicensed && !isComputingPreview )
            {
                return CodeActionResult.Error(
                    LicensingDiagnosticDescriptors.CodeActionNotAvailable.CreateRoslynDiagnostic( null, (codeFixTitle, codeFix.Creator) ) );
            }
            else
            {
                var diagnostics = new DiagnosticBag();

                var context = new CodeActionContext(
                    partialCompilation,
                    compilationServices,
                    pipelineResult.Value.Configuration,
                    isComputingPreview,
                    cancellationToken );

                var codeFixBuilder = new CodeActionBuilder( context );

                var userCodeExecutionContext = new UserCodeExecutionContext(
                    configuration.ServiceProvider!.Value,
                    diagnostics,
                    UserCodeMemberInfo.FromDelegate( codeFix.CodeFix.CodeAction ) );

                await this._userCodeInvoker.InvokeAsync( () => codeFix.CodeFix.CodeAction( codeFixBuilder ), userCodeExecutionContext );

                if ( diagnostics.HasError )
                {
                    return CodeActionResult.Error( diagnostics );
                }
                else
                {
                    return context.ToCodeActionResult();
                }
            }
        }
    }
}