using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl
{
    internal static partial class TemplatingDiagnosticDescriptors
    {
        // Templating errors (alex).
        // Reserved range 100-199
        
        private const string templateCategory = "Template";

       public static readonly DiagnosticDescriptor CannotReferenceRuntimeExpressionFromBuildTimeExpression
            = new DiagnosticDescriptor( "CR0100", "Cannot reference a run-time expression from a compile-time expression",
                "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                templateCategory, DiagnosticSeverity.Error, true );
    }
}
