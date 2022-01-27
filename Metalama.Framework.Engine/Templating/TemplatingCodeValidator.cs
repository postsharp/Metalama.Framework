// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
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
            bool isCompileTimeTreeOutdated,
            bool isDesignTime,
            CancellationToken cancellationToken )
        {
            Visitor visitor = new( semanticModel, reportDiagnostic, serviceProvider, isCompileTimeTreeOutdated, isDesignTime, cancellationToken );
            visitor.Visit( semanticModel.SyntaxTree.GetRoot() );

            return !visitor.HasError;
        }
    }
}