// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;

namespace Metalama.Framework.Engine.Templating
{
    public static partial class TemplatingCodeValidator
    {
        internal static bool Validate(
            Compilation compilation,
            IDiagnosticAdder diagnosticAdder,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken )
        {
            // Validate run-time code against templating rules.
            var hasError = false;

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                if ( !Validate( serviceProvider, semanticModel, diagnosticAdder.Report, false, false, cancellationToken ) )
                {
                    hasError = true;
                }
            }

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
                Visitor visitor = new( semanticModel, reportDiagnostic, serviceProvider, reportCompileTimeTreeOutdatedError, isDesignTime, cancellationToken );
                visitor.Visit( semanticModel.SyntaxTree.GetRoot() );

                return !visitor.HasError;
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
                    handler.ReportException( e, reportDiagnostic );

                    // We return successfully because we want the compilation to continue regardless.
                    return true;
                }
            }
        }
    }
}