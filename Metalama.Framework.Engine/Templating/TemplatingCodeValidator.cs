// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Templating
{
    public static partial class TemplatingCodeValidator
    {
        internal static async Task<bool> ValidateAsync(
            ProjectServiceProvider serviceProvider,
            CompilationContext compilationContext,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            var taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
            var semanticModelProvider = compilationContext.SemanticModelProvider;

            var hasError = false;

            void ValidateSyntaxTree( SyntaxTree syntaxTree )
            {
                var semanticModel = semanticModelProvider.GetSemanticModel( syntaxTree );

                if ( !ValidateCore( serviceProvider, semanticModel, compilationContext, diagnosticAdder.Report, false, false, cancellationToken ) )
                {
                    hasError = true;
                }
            }

            await taskScheduler.RunInParallelAsync( compilationContext.Compilation.SyntaxTrees, ValidateSyntaxTree, cancellationToken );

            return !hasError;
        }

        public static bool Validate(
            ProjectServiceProvider serviceProvider,
            SemanticModel semanticModel,
            Action<Diagnostic> reportDiagnostic,
            bool reportCompileTimeTreeOutdatedError,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            var compilationContext = serviceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( semanticModel.Compilation );

            return Validate(
                serviceProvider,
                compilationContext,
                semanticModel,
                reportDiagnostic,
                reportCompileTimeTreeOutdatedError,
                isDesignTime,
                cancellationToken );
        }

        public static bool Validate(
            ProjectServiceProvider serviceProvider,
            CompilationContext compilationContext,
            SemanticModel semanticModel,
            Action<Diagnostic> reportDiagnostic,
            bool reportCompileTimeTreeOutdatedError,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            try
            {
                return ValidateCore(
                    serviceProvider,
                    semanticModel,
                    compilationContext,
                    reportDiagnostic,
                    reportCompileTimeTreeOutdatedError,
                    isDesignTime,
                    cancellationToken );
            }
            catch ( Exception e )
            {
                var handler = serviceProvider.Global.GetService<ICompileTimeExceptionHandler>();

                if ( handler == null )
                {
                    throw;
                }
                else
                {
                    // It is important to swallow the exception here because this validator is executed on the whole code, even without
                    // aspect, so an exception in this code would have a large impact without any workaround. However, this code has no
                    // other use than reporting diagnostics, so skipping it is safer than failing the compilation. 
                    handler.ReportException( e, reportDiagnostic, true, out _ );

                    // We return successfully because we want the compilation to continue regardless.
                    return true;
                }
            }
        }

        private static bool ValidateCore(
            ProjectServiceProvider serviceProvider,
            SemanticModel semanticModel,
            CompilationContext compilationContext,
            Action<Diagnostic> reportDiagnostic,
            bool reportCompileTimeTreeOutdatedError,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            Visitor visitor = new(
                serviceProvider,
                semanticModel,
                compilationContext,
                reportDiagnostic,
                reportCompileTimeTreeOutdatedError,
                isDesignTime,
                cancellationToken );

            visitor.Visit( semanticModel.SyntaxTree.GetRoot() );

            return !visitor.HasError;
        }
    }
}