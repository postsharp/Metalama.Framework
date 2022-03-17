// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using static Metalama.Framework.Diagnostics.Severity;

namespace Metalama.Framework.Engine.Templating
{
#pragma warning disable SA1118 // Allow multi-line parameters.

    public static class TemplatingDiagnosticDescriptors
    {
        // Reserved range 100-199

        private const string _category = "Metalama.Template";

        internal static readonly DiagnosticDefinition<(string Expression, string ParentExpression)>
            CannotReferenceRuntimeExpressionFromBuildTimeExpression
                = new(
                    "LAMA0100",
                    "Cannot reference a run-time expression from a compile-time expression",
                    "Cannot reference the run-time expression {0} because the parent expression {1} is compile-time",
                    _category,
                    Error );

        public static readonly DiagnosticDefinition<string> LanguageFeatureIsNotSupported
            = new(
                "LAMA0101",
                "The C# language feature is not supported.",
                "'{0}' is not supported in a template.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string Expression, string ActualScope, string ExpectedScope, string Context)> ScopeMismatch
            = new(
                "LAMA0104",
                "The expression is expected to be of a different scope (run-time or compile-time)",
                "The expression '{0}' is {1} but it is expected to be {2} because the expression appears in {3}.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> SplitVariables
            = new(
                "LAMA0105",
                "Build-time and run-time local variables cannot be mixed in the same declaration. Split them into different declarations; "
                + "one for run-time variables, and one for compile-time variables",
                "Local variables {0} cannot be declared in the same declaration. "
                + "Split them into different declarations; one for run-time variables, and one for compile-time variables",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> UnsupportedContextForProceed
            = new(
                "LAMA0106",
                "The meta.Proceed() method can only be invoked from a local variable assignment or a return statement.",
                "The meta.Proceed() method can only be invoked from a local variable assignment or a return statement.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string Expression, ITypeSymbol ExpressionType)> CannotConvertBuildTime
            = new(
                "LAMA0107",
                "Cannot convert an expression into compile-time code because the expression is of an unsupported type",
                "The expression '{0}' of type '{1}' cannot be compiled into compile-time code because it is of an unsupported type.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string VariableName, string RunTimeCondition)> CannotSetCompileTimeVariableInRunTimeConditionalBlock
            = new(
                "LAMA0108",
                "Cannot set a compile-time variable in a block whose execution depends on a run-time condition",
                "Cannot set the compile-time variable '{0}' here because it is part of a block whose execution depends on the run-time condition '{1}'. " +
                "Move the assignment out of the run-time-conditional block.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> UndeclaredRunTimeIdentifier
            = new(
                "LAMA0109",
                "The run-time identifier was not declared",
                "The run-time identifier '{0}' was not defined.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string VariableName, string RunTimeCondition)> CannotHaveCompileTimeLoopInRunTimeConditionalBlock
            = new(
                "LAMA0110",
                "Cannot have a compile-time loop in a block whose execution depends on a run-time condition",
                "The compile-time loop '{0}' is not allowed here because it is a part of block whose execution depends on the run-time condition '{1}'. " +
                "Move the loop out of the run-time-conditional block.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectName, string AssemblyName)>
            CannotFindAspectInCompilation
                = new(
                    "LAMA0113",
                    "An aspect type defined in a reference assembly could not be found in the compilation",
                    "Cannot find in the current compilation the aspect type '{0}' defined in the aspect library '{1}'.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(IDeclaration Advice, string Expression, IDeclaration TargetDeclaration, DeclarationKind TargetKind)>
            CannotUseThisInStaticContext
                = new(
                    "LAMA0114",
                    "Cannot use 'meta.This' from a static context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because the target {3} is static.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(IDeclaration Advice, string Expression, IDeclaration TargetDeclaration, DeclarationKind TargetKind,
                string MissingKind)>
            MemberMemberNotAvailable
                = new(
                    "LAMA0115",
                    "Cannot use a meta member in the current context",
                    "The advice '{0}' cannot use '{1}' in an advice applied to {3} '{2}' because there is no '{4}' in the context of a {3}.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol DeclaringSymbol, ISymbol ReferencedSymbol)>
            CannotReferenceCompileTimeOnly
                = new(
                    "LAMA0117",
                    "Cannot reference a compile-time-only declaration in a non-compile-time-only declaration.",
                    "Cannot reference '{1}' in '{0}' because '{1}' is compile-time-only but '{0}' is not. " +
                    "Consider adding [CompileTimeOnly] to '{0}', or do not use '{1}' in '{0}'.'",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<ISymbol>
            CompileTimeTypeNeedsRebuild
                = new(
                    "LAMA0118",
                    "The compile-time type needs rebuild.",
                    "The compile-time type '{0}' has been modified since the last build. Metalama will stop analyzing this solution until the "
                    + "next build and you may get errors related to the absence of generated source. "
                    + "To resume analysis, finish the work on all compile-time logic, then build the project (even if the run-time code still has issues).",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol Declaration, string Namespace)>
            CompileTimeCodeNeedsNamespaceImport
                = new(
                    "LAMA0119",
                    "The declaration contains compile-time code but it does not import the proper namespaces.",
                    "The compile-time declaration '{0}' contains compile-time code but it does not explicitly import the '{1}' namespaces. "
                    + "This may cause an inconsistent design-time experience. Import this namespace explicitly.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(ISymbol ReferencedDeclaration, ISymbol ReferencingDeclaration)>
            TemplateCannotReferenceTemplate
                = new(
                    "LAMA0220",
                    "A template cannot reference another template.",
                    "The declaration '{0}' cannot be referenced from '{1}' both declarations are templates, "
                    + "and templates cannot reference each other yet.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<string>
            CannotUseThisInRunTimeContext
                = new(
                    "LAMA0221",
                    "Cannot use 'this' when a run-time expression is expected.",
                    "Cannot use 'this' in expression '{0}' because a run-time expression is expected, and 'this' "
                    + "in a template is a compile-time keyword. Use 'meta.This' instead.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<string>
            CannotEmitCompileTimeAssembly
                = new(
                    "LAMA0222",
                    "Error compiling the compile-time assembly.",
                    "The compile-time project could not be compiled. In most cases, this is due to a problem in your code and can be diagnosed " +
                    "using the other reported errors. If, however, you believe this is due to a bug in Metalama, please report the issue and include diagnostic "
                    +
                    "information available in '{0}'.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(string MethodName, IDeclaration TargetDeclaration)>
            CannotUseSpecificProceedInThisContext
                = new(
                    "LAMA0223",
                    "Cannot use a specific Proceed variant in the current context.",
                    "Cannot use the {0} method in '{1}' because the return type of the method is compatible with the {0} method.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<string>
            CannotUseDynamicInUninitializedLocal
                = new(
                    "LAMA0224",
                    "Cannot declare local variable with the dynamic type if the variable is not initialized.",
                    "The 'dynamic' keyword cannot be used in the local variable '{0}' because it is not initialized. Use 'var'.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<string>
            CallToExpressionMustHaveExplicitCast
                = new(
                    "LAMA0225",
                    "Calls to meta.Expression must be explicitly cast to IExpression.",
                    "The call to '{0}' must be explicitly cast to IExpression.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<string> ScopeConflict
            = new(
                "LAMA0226",
                "The generic type combines run-time-only and compile-time-only types.",
                "The generic type '{0}' combines run-time-only and compile-time-only types.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> InvalidDynamicTypeConstruction
            = new(
                "LAMA0227",
                "'dynamic' is forbidden as a generic parameter type or array element type in a template.",
                "The type '{0}' is forbidden in a template 'dynamic' cannot be used as a generic argument type or an array element type.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> TemplateMustBeInNullableContext
            = new(
                "LAMA0228",
                "A template must be fully in a nullable context",
                "The template '{0}' must be contained in a nullable context.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<ITypeSymbol> NeutralTypesForbiddenInNestedRunTimeTypes
            = new(
                "LAMA0229",
                "Types that are both compile-time and run-time are forbidden in run-time-only types.",
                "The type '{0}' cannot be [CompileTime] because it is nested in a run-time-type. It can however be [CompileTimeOnly].",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<ITypeSymbol> NestedCompileTypesMustBePrivate
            = new(
                "LAMA0230",
                "Nested compile-time types must have private accessibility.",
                "The compile-time type '{0}' must have private visibility because it is nested in a run-time-type.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(ITypeSymbol NestedType, Type Interface)> RunTimeTypesCannotHaveCompileTimeTypesExceptClasses
            = new(
                "LAMA0231",
                "Run-time types cannot have other compile-time types than classes.",
                "The compile-time type '{0}' cannot be nested in a run-time class. The only compile-time type that can be nested in run-time type is a class implementing '{1}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<string> TemplateUsesUnsupportedLanguageFeature
            = new(
                "LAMA0232",
                "Template code must be written in the specified C# version.",
                "Template code must be written in C# {0}.",
                _category,
                Error );
    }
}