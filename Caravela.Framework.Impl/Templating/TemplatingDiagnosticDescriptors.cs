// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating
{
    internal static class TemplatingDiagnosticDescriptors
    {
        // Templating errors (alex).
        // Reserved range 100-199

        private const string _templateCategory = "Template";

        internal static readonly DiagnosticDescriptor CannotReferenceRuntimeExpressionFromBuildTimeExpression
            = new DiagnosticDescriptor(
                "CR0100",
                "Cannot reference a run-time expression from a compile-time expression",
                "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );

        public static readonly DiagnosticDescriptor LanguageFeatureIsNotSupported
            = new DiagnosticDescriptor(
                "CR0101",
                "The C# language feature is not supported.",
                "The {0} language feature is not supported by the template compiler .",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );

        internal static readonly DiagnosticDescriptor ReturnTypeDoesNotMatch
            = new DiagnosticDescriptor(
                "CR0102",
                "The value returned by the template does not match the target's return type.",
                "The template {0} cannot be applied to the target {1} - the value returned by the template does not match the target's return type.",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );

        internal static readonly DiagnosticDescriptor LocalVariableAmbiguousCoercion
            = new DiagnosticDescriptor(
                "CR0103",
                "The local variable is both coerced to be run-time and compile-time",
                "The local variable '{0}' is both coerced to be run-time and compile-time.",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );

        internal static readonly DiagnosticDescriptor ScopeMismatch
            = new DiagnosticDescriptor(
                "CR0104",
                "The expression is expected to be of a different scope (run-time or compile-time)",
                "The expression '{0}' is {1} but it is expected to be {2} because the expression appears in {3}.",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );
        
        internal static readonly DiagnosticDescriptor SplitVariables
            = new DiagnosticDescriptor(
                "CR0105",
                "Build-time and run-time local variables cannot be mixed in the same declaration. Split them into different declarations; one for run-time variables, and one for compile-time variables",
                "Local variables {0} cannot be declared in the same declaration. Split them into different declarations; one for run-time variables, and one for compile-time variables",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );

        internal static readonly DiagnosticDescriptor UnsupportedContextForProceed
            = new DiagnosticDescriptor(
                "CR0106",
                "The proceed() method can only be invoked from a local variable assignment or a return statement.",
                "The proceed() method can only be invoked from a local variable assignment or a return statement.",
                _templateCategory,
                DiagnosticSeverity.Error,
                true );
    }
}