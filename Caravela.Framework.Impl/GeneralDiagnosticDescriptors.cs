﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Caravela.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Caravela.Framework.Impl
{
    internal static class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _category = "Caravela.General";

        public static readonly DiagnosticDefinition<(string Message, string File)> UnhandledException =
            new(
                "CR0001",
                _category,
                "Unexpected exception occurred in Caravela: {0} Exception details are in '{1}'. " +
                " Please report this issue at https://www.postsharp.net/support and attach this file to the ticket.",
                Error,
                "Unexpected exception in Caravela." );

        public static readonly
            DiagnosticDefinition<(string AspectType, DeclarationKind DeclarationKind, IDeclaration Declaration, ITypeSymbol InterfaceType)>
            AspectAppliedToIncorrectDeclaration =
                new(
                    "CR0003",
                    _category,
                    "Aspect '{0}' cannot be applied to {1} '{2}', because this aspect does not implement the '{3}' interface.",
                    Error,
                    "Aspect applied to incorrect kind of declaration." );

        public static readonly DiagnosticDefinition<(INamedTypeSymbol AspectType, string Weavers)> AspectHasMoreThanOneWeaver =
            new( "CR0004", _category, "Aspect '{0}' can have at most one weaver, but it has the following: {1}.", Error, "Aspect has more than one weaver." );

        public static readonly DiagnosticDefinition<(string AspectType, string Exception)> ExceptionInWeaver =
            new( "CR0006", _category, "Exception occurred while executing the weaver of aspect '{0}': {1}", Error, "Exception in aspect weaver." );

        public static readonly DiagnosticDefinition<(IDeclaration Member, int RequiredArgumentsCount, int ActualArgumentsCount)> MemberRequiresNArguments =
            new( "CR0012", _category, "Member '{0}' requires {1} arguments but received {2}.", Error, "Member requires number of arguments." );

        public static readonly DiagnosticDefinition<(IDeclaration Member, int RequiredArgumentsCount, int ActualArgumentsCount)>
            MemberRequiresAtLeastNArguments =
                new( "CR0013", _category, "Member '{0}' requires at least {1} arguments but received {2}.", Error, "Member requires more arguments." );

        public static readonly DiagnosticDefinition<IMemberOrNamedType> MustProvideInstanceForInstanceMember =
            new(
                "CR0015",
                _category,
                "Member {0} is not static, but has been used with a null instance.",
                Error,
                "Has to provide instance for an instance member." );

        public static readonly DiagnosticDefinition<IMemberOrNamedType> CannotAccessOpenGenericMember =
            new( "CR0016", _category, "Member {0} Cannot be accessed without specifying generic arguments.", Error, "Cannot access an open generic member." );

        public static readonly DiagnosticDefinition<IMemberOrNamedType> CannotProvideInstanceForLocalFunction =
            new(
                "CR0018",
                _category,
                "{0} is a local function, so it Cannot be invoked with a non-null instance.",
                Error,
                "Cannot provide instance for a local function." );

        public static readonly DiagnosticDefinition<(string Expression, string ParameterName, IMemberOrNamedType Method)> CannotPassExpressionToByRefParameter =
            new(
                "CR0019",
                _category,
                "Cannot pass the expression '{0}' to the '{1}' parameter of method '{2}' because the parameter is 'out' or 'ref'.",
                Error,
                "Cannot use an expression in an out or ref parameter." );

        public static readonly DiagnosticDefinition<string> CannotFindType =
            new( "CR0020", _category, "Cannot find the type '{0}'.", Error, "Cannot find a type" );

        public static readonly DiagnosticDefinition<string> CycleInAspectOrdering =
            new(
                "CR0021",
                _category,
                "A cycle was found in the specifications of aspect ordering between the following aspect part: {0}.",
                Error,
                "A cycle was found in aspect ordering." );

        public static readonly DiagnosticDefinition<(string ParentType, string ChildType)> CannotAddChildAspectToPreviousPipelineStep = new(
            "CR0022",
            _category,
            "The aspect '{0}' cannot add a child aspect to of type '{1}' because this aspect type has already been processed.",
            Error,
            "Cannot add an aspect to a previous step of the compilation pipeline." );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Target)> CannotAddAdviceToPreviousPipelineStep = new(
            "CR0023",
            _category,
            "The aspect '{0}' cannot add an advice to '{1}' because this declaration has already been processed.",
            Error,
            "Cannot add an advice to a previous step of the compilation pipeline." );

        public static readonly DiagnosticDefinition<(DeclarationKind ElementKind, ISymbol Symbol, ITypeSymbol AttributeType, string AdviceMethod )>
            TemplateMemberMissesAttribute = new(
                "CR0024",
                "The template member does not have the expected custom attribute.",
                "The template {0} '{1}' must be annotated with the custom attribute [{2}] otherwise it cannot be used with the dynamic advice '{3}'.",
                _category,
                Error );

        public static readonly DiagnosticDefinition<(string AspectType, string MethodName)> AspectMustHaveExactlyOneTemplateMember = new(
            "CR0025",
            "The aspect type must have exactly one member of a given name otherwise it cannot be used as a dynamic advice.",
            "The type '{0}' must have exactly one member named '{1}'.",
            _category,
            Error );

        public static readonly DiagnosticDefinition<(string AspectType, string MethodName, string ExceptionType, string Exception)> ExceptionInUserCode = new(
            "CR0026",
            _category,
            "The aspect method '{0}.{1}' has thrown an exception of type '{2}': {3}",
            Error,
            "The aspect has thrown an exception." );

        public static readonly DiagnosticDefinition<AssemblyIdentity> CannotFindCompileTimeAssembly = new(
            "CR0027",
            _category,
            "The assembly '{0}' required at compile-time cannot be found.",
            Error,
            "Cannot find an assembly required by the compile-time assembly." );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclarationBuilder MemberBuilder, INamedType AttributeType)>
            CompatibleAttributeConstructorDoesNotExist = new(
                "CR0028",
                _category,
                "The aspect '{0}' cannot add attribute '{1}' to member '{2}' because no compatible constructor exists for given types.",
                Error,
                "Compatible attribute constructor does not exist." );

        public static readonly DiagnosticDefinition<string>
            InvalidCachedManifestFile = new(
                "CR0029",
                _category,
                "The cache file '{0}' was corrupted. It has been deleted. Please restart the compilation.",
                Error,
                "The compile-time project manifest file is corrupted." );

        public static readonly DiagnosticDefinition<string>
            InvalidCompileTimeProjectResource = new(
                "CR0030",
                _category,
                "The compile-time project in assembly '{0}' is corrupted.",
                Error,
                "The compile-time project resource file was corrupted." );

        public static readonly DiagnosticDefinition<(string TemplateName, string ClassName)>
            TemplateWithSameNameAlreadyDefined = new(
                "CR0032",
                _category,
                "The class '{1}' defines several templates named '{0}'. Template names must be unique.",
                Error,
                "The class already defines a template of the same name." );

        public static readonly DiagnosticDefinition<(string ClassName, string MemberName, string AttributeName)>
            MemberDoesNotHaveTemplateAttribute = new(
                "CR0033",
                _category,
                "The class '{0}' defines a member named '{1}', but the member is not annotated with the '{2}' custom attribute.",
                Error,
                "The member does not have a template custom attribute." );

        public static readonly DiagnosticDefinition<(string ClassName, string MemberName, string ExpectedAttribute, string ActualAttribute)>
            TemplateIsOfTheWrongType = new(
                "CR0034",
                _category,
                "The template '{0}.{1}' was expected to be annotated with the [{2}] attribute, but it is annotated with [{3}].",
                Error,
                "The member does not have a template custom attribute." );

        public static readonly DiagnosticDefinition<(string Layer1, string Layer2)> UnorderedLayers = new(
            "CR0035",
            _category,
            "The aspect layers '{0}' and '{1}' are not strongly ordered. Add an [assembly: " + nameof(AspectOrderAttribute) +
            "(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.",
            Warning,
            "Two layers are not strongly ordered." );

        public static readonly DiagnosticDefinition<(string TemplateName, string ClassName, string BaseClassName)>
            TemplateWithSameNameAlreadyDefinedInBaseClass = new(
                "CR0036",
                _category,
                "The class '{1}' defines a new template named '{0}', but the base class '{2}' already defines a template of the same name. Template names must be unique.",
                Error,
                "The class already defines a template of the same name." );

        // TODO: Use formattable string (C# does not seem to find extension methods).
        public static readonly DiagnosticDefinition<string>
            UnsupportedFeature = new(
                "CR0099",
                "Feature is not yet supported.",
                "Feature is not yet supported: {0}",
                _category,
                Error );
    }
}