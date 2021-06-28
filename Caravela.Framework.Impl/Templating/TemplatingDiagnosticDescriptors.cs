// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using static Caravela.Framework.Diagnostics.Severity;

namespace Caravela.Framework.Impl.Templating
{
#pragma warning disable SA1118 // Allow multi-line parameters.

    internal static class TemplatingDiagnosticDescriptors
    {
        // Reserved range 100-199

        private const string _category = "Caravela.Template";

        internal static readonly DiagnosticDefinition<(string Expression, string ParentExpression)>
            CannotReferenceRuntimeExpressionFromBuildTimeExpression
                = new(
                    "CR0100",
                    "Cannot reference a run-time expression from a compile-time expression",
                    "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                    _category,
                    Error );

        public static readonly DiagnosticDefinition<string> LanguageFeatureIsNotSupported
            = new(
                "CR0101",
                "The C# language feature is not supported.",
                "'{0}' is not supported in a template.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string Expression, string ActualScope, string ExpectedScope, string Context)> ScopeMismatch
            = new(
                "CR0104",
                "The expression is expected to be of a different scope (run-time or compile-time)",
                "The expression '{0}' is {1} but it is expected to be {2} because the expression appears in {3}.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> SplitVariables
            = new(
                "CR0105",
                "Build-time and run-time local variables cannot be mixed in the same declaration. Split them into different declarations; "
                + "one for run-time variables, and one for compile-time variables",
                "Local variables {0} cannot be declared in the same declaration. "
                + "Split them into different declarations; one for run-time variables, and one for compile-time variables",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> UnsupportedContextForProceed
            = new(
                "CR0106",
                "The meta.Proceed() method can only be invoked from a local variable assignment or a return statement.",
                "The meta.Proceed() method can only be invoked from a local variable assignment or a return statement.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string Expression, ITypeSymbol ExpressionType)> CannotConvertBuildTime
            = new(
                "CR0107",
                "Cannot convert an expression into compile-time code because the expression is of an unsupported type",
                "The expression '{0}' of type '{1}' cannot be compiled into compile-time code because it is of an unsupported type.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string VariableName, string RunTimeCondition)> CannotSetCompileTimeVariableInRunTimeConditionalBlock
            = new(
                "CR0108",
                "Cannot set a compile-time variable in a block whose execution depends on a run-time condition",
                "Cannot set the compile-time variable '{0}' here because it is part of a block whose execution depends on the run-time condition '{1}'. " +
                "Move the assignment out of the run-time-conditional block.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> UndeclaredRunTimeIdentifier
            = new(
                "CR0109",
                "The run-time identifier was not declared",
                "The run-time identifier '{0}' was not defined.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string VariableName, string RunTimeCondition)> CannotHaveCompileTimeLoopInRunTimeConditionalBlock
            = new(
                "CR0110",
                "Cannot have a compile-time loop in a block whose execution depends on a run-time condition",
                "The compile-time loop '{0}' is not allowed here because it is a part of block whose execution depends on the run-time condition '{1}'. " +
                "Move the loop out of the run-time-conditional block.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(ISymbol TemplateSymbol, IDeclaration TargetDeclaration, string ExceptionType, string Exception)>
            ExceptionInTemplate
                = new(
                    "CR0112",
                    "An advice threw an exception",
                    "The advice '{0}' threw '{2}' when applied to '{1}':" + Environment.NewLine + "{3}",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(string AspectName, string AssemblyName)>
            CannotFindAspectInCompilation
                = new(
                    "CR0113",
                    "An aspect type defined in a reference assembly could not be found in the compilation",
                    "Cannot find in the current compilation the aspect type '{0}' defined in the aspect library '{1}'.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol Advice, string Expression, IDeclaration TargetDeclaration, DeclarationKind TargetKind)>
            CannotUseThisInStaticContext
                = new(
                    "CR0114",
                    "Cannot use 'meta.This' from a static context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because the target {3} is static.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol Advice, string Expression, IDeclaration TargetDeclaration, DeclarationKind TargetKind,
                string MissingKind)>
            MemberMemberNotAvailable
                = new(
                    "CR0115",
                    "Cannot use a meta member in the current context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because there is no '{4}' in the context of a {3}.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol DeclaringSymbol, ISymbol ReferencedSymbol)>
            CannotReferenceCompileTimeOnly
                = new(
                    "CR0117",
                    "Cannot reference a compile-time-only declaration in a non-compile-time-only declaration.",
                    "Cannot reference '{1}' in '{0}' because '{1}' is compile-time-only but '{0}' is not. " +
                    "Consider adding [CompileTimeOnly] to '{0}', or do not use '{1}' in '{0}'.'",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<ISymbol>
            CompileTimeTypeNeedsRebuild
                = new(
                    "CR0118",
                    "The compile-time type needs rebuild.",
                    "The compile-time type '{0}' has been modified since the last build. Caravela will stop analyzing this solution until the "
                    + "next build and you may get errors related to the absence of generated source. "
                    + "To resume analysis, finish the work on all compile-time logic, then build the project (even if the run-time code still has issues).",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol Declaration, string Namespace)>
            CompileTimeCodeNeedsNamespaceImport
                = new(
                    "CR0119",
                    "The declaration contains compile-time code but it does not import the proper namespaces.",
                    "The compile-time declaration '{0}' contains compile-time code but it does not explicitly import the '{1}' namespaces. "
                    + "This may cause an inconsistent design-time experience. Import this namespace explicitly.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol ReferencedDeclaration, ISymbol ReferencingDeclaration)>
            TemplateCannotReferenceTemplate
                = new(
                    "CR0220",
                    "A template cannot reference another template.",
                    "The declaration '{0}' cannot be referenced from '{1}' both declarations are templates, "
                    + "and templates cannot reference each other yet.",
                    _category,
                    Error );
        
        internal static readonly DiagnosticDefinition
            CannotUseThisInRunTimeContext
                = new(
                    "CR0221",
                    "Cannot use 'this' when a run-time expression is expected.",
                    "The expression 'this' cannot be used where a run-time expression is expected. Use 'meta.This' instead.",
                    _category,
                    Error );

    }
}