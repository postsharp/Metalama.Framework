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

        internal static readonly DiagnosticDefinition<string> CannotSetCompileTimeVariableInRunTimeConditionalBlock
            = new(
                "CR0108",
                "Cannot set a compile-time variable in a block whose execution depends on a run-time condition",
                "Cannot set the compile-time variable '{0}' here because it is part of a block whose execution depends on a run-time condition.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> UndeclaredRunTimeIdentifier
            = new(
                "CR0109",
                "The run-time identifier was not declared",
                "The run-time identifier '{0}' was not defined.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> CannotHaveCompileTimeLoopInRunTimeConditionalBlock
            = new(
                "CR0110",
                "Cannot have a compile-time loop in a block whose execution depends on a run-time condition",
                "The compile-time loop '{0}' is not allowed here because it is a part of block whose execution depends on a run-time condition.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(ISymbol TemplateSymbol, ICodeElement TargetDeclaration, string ExceptionType, string Exception)>
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

        internal static readonly DiagnosticDefinition<(ISymbol Advice, string Expression, ICodeElement TargetDeclaration, CodeElementKind TargetKind)>
            CannotUseThisInStaticContext
                = new(
                    "CR0114",
                    "Cannot use 'meta.This' from a static context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because the target {3} is static.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol Advice, string Expression, ICodeElement TargetDeclaration, CodeElementKind TargetKind,
                string MissingKind)>
            MemberMemberNotAvailable
                = new(
                    "CR0115",
                    "Cannot use a meta member in the current context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because there is no '{4}' in the context of a {3}.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<ISymbol>
            VariableIsCompileTime
                = new(
                    "CR0116",
                    "Cannot use a meta member in the current context",
                    "The variable '{0}' is compile-time Info.",
                    _category,
                    Info );
    }
}