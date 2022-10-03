// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Templating
{
    public static partial class TemplatingCodeValidator
    {
        internal static async Task<bool> ValidateAsync(
            Compilation compilation,
            IDiagnosticAdder diagnosticAdder,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken )
        {
            var taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();

            var hasError = false;

            void ValidateSyntaxTree( SyntaxTree syntaxTree )
            {
                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                if ( !ValidateCore( serviceProvider, semanticModel, diagnosticAdder.Report, false, false, cancellationToken ) )
                {
                    hasError = true;
                }
            }

            await taskScheduler.RunInParallelAsync( compilation.SyntaxTrees, ValidateSyntaxTree, cancellationToken );

            return !hasError;
        }

        public static bool Validate(
            IServiceProvider serviceProvider,
            SemanticModel semanticModel,
            Action<Diagnostic> reportDiagnostic,
            bool reportCompileTimeTreeOutdatedError,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            try
            {
                return ValidateCore( serviceProvider, semanticModel, reportDiagnostic, reportCompileTimeTreeOutdatedError, isDesignTime, cancellationToken );
            }
            catch ( Exception e )
            {
                var handler = serviceProvider.GetService<ICompileTimeExceptionHandler>();

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
            IServiceProvider serviceProvider,
            SemanticModel semanticModel,
            Action<Diagnostic> reportDiagnostic,
            bool reportCompileTimeTreeOutdatedError,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            Visitor visitor = new( semanticModel, reportDiagnostic, serviceProvider, reportCompileTimeTreeOutdatedError, isDesignTime, cancellationToken );
            visitor.Visit( semanticModel.SyntaxTree.GetRoot() );

            return !visitor.HasError;
        }
    }
}