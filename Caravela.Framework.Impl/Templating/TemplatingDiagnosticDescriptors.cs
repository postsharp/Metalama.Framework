﻿using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    public static partial class TemplatingDiagnosticDescriptors
    {
        // Templating errors (alex).
        // Reserved range 100-199

        private const string templateCategory = "Template";

        public static readonly DiagnosticDescriptor CannotReferenceRuntimeExpressionFromBuildTimeExpression
             = new DiagnosticDescriptor( "CR0100", "Cannot reference a run-time expression from a compile-time expression",
                 "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                 templateCategory, DiagnosticSeverity.Error, true );

        public static readonly DiagnosticDescriptor LanguageFeatureIsNotSupported
             = new DiagnosticDescriptor( "CR0101", "The C# language feature is not supported.",
                 "This C# language feature is not supported by the template compiler - {0}.",
                 templateCategory, DiagnosticSeverity.Error, true );

        public static readonly DiagnosticDescriptor ReturnTypeDoesNotMatch
             = new DiagnosticDescriptor( "CR0102", "The value returned by the template does not match the target's return type.",
                 "The template {0} cannot be applied to the target {1} - the value returned by the template does not match the target's return type.",
                 templateCategory, DiagnosticSeverity.Error, true );
    }
}
