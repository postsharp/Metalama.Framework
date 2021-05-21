// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using static Caravela.Framework.Diagnostics.Severity;

namespace Caravela.Framework.Impl
{
    internal static class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _category = "Caravela.General";

        public static readonly DiagnosticDefinition<(string Message, string File)> UncaughtException =
            new( "CR0001", _category, "Unexpected exception occurred in Caravela: {0} Exception details are in {1}.", Error,
                 "Unexpected exception in Caravela." );

        public static readonly DiagnosticDefinition<(ITypeSymbol AspectType, CodeElementKind CodeElementKind, ICodeElement CodeElement, ITypeSymbol
                InterfaceType)>
            AspectAppliedToIncorrectElement =
                new( "CR0003", _category, "Aspect '{0}' cannot be applied to {1} '{2}', because this aspect does not implement the '{3}' interface.", Error,
                     "Aspect applied to incorrect kind of element." );

        public static readonly DiagnosticDefinition<(INamedTypeSymbol AspectType, string Weavers)> AspectHasMoreThanOneWeaver =
            new( "CR0004", _category, "Aspect '{0}' can have at most one weaver, but it has the following: {1}.", Error, "Aspect has more than one weaver." );

        public static readonly DiagnosticDefinition<(string AspectType, string Exception)> ExceptionInWeaver =
            new( "CR0006", _category, "Exception occurred while executing the weaver of aspect '{0}': {1}", Error, "Exception in aspect weaver." );

        public static readonly DiagnosticDefinition<(ICodeElement Member, int ArgumentsCount)> MemberRequiresNArguments =
            new( "CR0012", _category, "Member '{0}' requires {1} arguments.", Error, "Member requires number of arguments." );

        public static readonly DiagnosticDefinition<(ICodeElement Member, int ArgumentsCount)> MemberRequiresAtLeastNArguments =
            new( "CR0013", _category, "Member '{0}' requires at least {1} arguments.", Error, "Member requires more arguments." );

        public static readonly DiagnosticDefinition<IMember> CannotProvideInstanceForStaticMember =
            new( "CR0014", _category, "Member {0} is static, but has been used with a non-null instance.", Error,
                 "Cannot provide instance for a static member." );

        public static readonly DiagnosticDefinition<IMember> MustProvideInstanceForInstanceMember =
            new( "CR0015", _category, "Member {0} is not static, but has been used with a null instance.", Error,
                 "Has to provide instance for an instance member." );

        public static readonly DiagnosticDefinition<IMember> CannotAccessOpenGenericMember =
            new( "CR0016", _category, "Member {0} Cannot be accessed without specifying generic arguments.", Error, "Cannot access an open generic member." );

        public static readonly DiagnosticDefinition<IMember> CannotProvideInstanceForLocalFunction =
            new( "CR0018", _category, "{0} is a local function, so it Cannot be invoked with a non-null instance.", Error,
                 "Cannot provide instance for a local function." );

        public static readonly DiagnosticDefinition<(string Expression, string ParameterName, IMember Method)> CannotPassExpressionToByRefParameter =
            new( "CR0019", _category, "Cannot pass the expression '{0}' to the '{1}' parameter of method '{2}' because the parameter is 'out' or 'ref'.", Error,
                 "Cannot use an expression in an out or ref parameter." );

        public static readonly DiagnosticDefinition<string> CannotFindType =
            new( "CR0020", _category, "Cannot find the type '{0}'.", Error, "Cannot find a type" );

        public static readonly DiagnosticDefinition<string> CycleInAspectOrdering =
            new( "CR0021", _category, "A cycle was found in the specifications of aspect ordering between the following aspect part: {0}.", Error,
                 "A cycle was found in aspect ordering." );

        public static readonly DiagnosticDefinition<(string ParentType, string ChildType)> CannotAddChildAspectToPreviousPipelineStep = new(
            "CR0022", _category, "The aspect '{0}' cannot add a child aspect to of type '{1}' because this aspect type has already been processed.", Error,
            "Cannot add an aspect to a previous step of the compilation pipeline." );

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElement Target)> CannotAddAdviceToPreviousPipelineStep = new(
            "CR0023", _category, "The aspect '{0}' cannot add an advice to '{1}' because this declaration has already been processed.", Error,
            "Cannot add an advice to a previous step of the compilation pipeline." );

        public static readonly DiagnosticDefinition<(CodeElementKind ElementKind, ISymbol Symbol, ITypeSymbol AttributeType, string AdviceMethod )>
            TemplateMemberMissesAttribute = new(
                "CR0024",
                "The template member does not have the expected custom attribute.",
                "The template {0} '{1}' must be annotated with the custom attribute [{2}] otherwise it cannot be used with the dynamic advice '{3}'.",
                _category,
                Error );

        public static readonly DiagnosticDefinition<(INamedType AspectType, string MethodName)> AspectMustHaveExactlyOneTemplateMethod = new(
            "CR0025",
            "The aspect type must have exactly one member of a given name otherwise it cannot be used as a dynamic advice.",
            "The type '{0}' must have exactly one member named '{1}'.",
            _category,
            Error );

        public static readonly DiagnosticDefinition<(INamedTypeSymbol AspectType, string ExceptionType, Exception Exception)> ExceptionInUserCode = new(
            "CR0026", _category, "The aspect '{0}' has thrown an exception of the '{1}': {2}", Error, "The aspect has thrown an exception." );

        public static readonly DiagnosticDefinition<AssemblyIdentity> CannotFindCompileTimeAssembly = new(
            "CR0027", _category, "The assembly '{0}' required at compile-time cannot be found.", Error,
            "Cannot find an assembly required by the compile-time assembly." );

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElementBuilder MemberBuilder, INamedType AttributeType)>
            CompatibleAttributeConstructorDoesNotExist = new(
                "CR0028", _category, "The aspect '{0}' cannot add attribute '{1}' to member '{2}' because no compatible constructor exists for given types.",
                Error, "Compatible attribute constructor does not exist." );

        public static readonly DiagnosticDefinition<string>
            InvalidCachedManifestFile = new(
                "CR0029", _category, "The cache file '{0}' was corrupted. It has been deleted. Please restart the compilation.", Error,
                "The compile-time project manifest file is corrupted." );

        public static readonly DiagnosticDefinition<string>
            InvalidCompileTimeProjectResource = new(
                "CR0030", _category, "The compile-time project in assembly '{0}' is corrupted.", Error,
                "The compile-time project resource file was corrupted." );
    }
}