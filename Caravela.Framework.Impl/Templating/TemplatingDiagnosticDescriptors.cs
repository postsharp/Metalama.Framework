// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating
{
#pragma warning disable SA1118 // Allow multi-line parameters.

    internal static class TemplatingDiagnosticDescriptors
    {
        // Reserved range 100-199

        private const string _category = "Caravela.Template";

        internal static readonly StrongDiagnosticDescriptor<(string Expression, string ParentExpression)>
            CannotReferenceRuntimeExpressionFromBuildTimeExpression
                = new(
                    "CR0100",
                    "Cannot reference a run-time expression from a compile-time expression",
                    "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                    _category,
                    DiagnosticSeverity.Error );

        public static readonly StrongDiagnosticDescriptor<string> LanguageFeatureIsNotSupported
            = new(
                "CR0101",
                "The C# language feature is not supported.",
                "The {0} language feature is not supported by the template compiler .",
                _category,
                DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<string> LocalVariableAmbiguousCoercion
            = new(
                "CR0103",
                "The local variable is both coerced to be run-time and compile-time",
                "The local variable '{0}' is both coerced to be run-time and compile-time.",
                _category,
                DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<(string Expression, string ActualScope, string ExpectedScope, string Context)> ScopeMismatch
            = new(
                "CR0104",
                "The expression is expected to be of a different scope (run-time or compile-time)",
                "The expression '{0}' is {1} but it is expected to be {2} because the expression appears in {3}.",
                _category,
                DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<string> SplitVariables
            = new(
                "CR0105",
                "Build-time and run-time local variables cannot be mixed in the same declaration. Split them into different declarations; "
                + "one for run-time variables, and one for compile-time variables",
                "Local variables {0} cannot be declared in the same declaration. "
                + "Split them into different declarations; one for run-time variables, and one for compile-time variables",
                _category,
                DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<string> UnsupportedContextForProceed
            = new(
                "CR0106",
                "The proceed() method can only be invoked from a local variable assignment or a return statement.",
                "The proceed() method can only be invoked from a local variable assignment or a return statement.",
                _category,
                DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<(string Expression, ITypeSymbol ExpressionType)> CannotConvertBuildTime
            = new(
                "CR0107",
                "Cannot convert an expression into compile-time code because the expression is of an unsupported type",
                "The expression '{0}' of type '{1}' cannot be compiled into compile-time code because it is of an unsupported type.",
                _category,
                DiagnosticSeverity.Error );

        public static Diagnostic CreateLanguageFeatureIsNotSupported( SyntaxNode node )
        {
            return LanguageFeatureIsNotSupported.CreateDiagnostic( node.GetLocation(), node.Kind().ToString() );
        }
    }
}