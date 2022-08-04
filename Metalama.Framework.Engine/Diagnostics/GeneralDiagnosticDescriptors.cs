// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Metalama.Framework.Engine.Diagnostics
{
    public class GeneralDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _category = "Metalama.General";

        internal static readonly DiagnosticDefinition<(string Message, string File)> UnhandledException =
            new(
                "LAMA0001",
                _category,
                "Unexpected exception occurred in Metalama: {0} Exception details are in '{1}'. " +
                " Please report this issue at https://www.postsharp.net/support and attach this file to the ticket.",
                Error,
                "Unexpected exception in Metalama." );

        internal static readonly
            DiagnosticDefinition<(string AspectType, DeclarationKind DeclarationKind, IDeclaration Declaration, ITypeSymbol InterfaceType)>
            AspectAppliedToIncorrectDeclaration =
                new(
                    "LAMA0003",
                    _category,
                    "Aspect '{0}' cannot be applied to {1} '{2}', because this aspect does not implement the '{3}' interface.",
                    Error,
                    "Aspect applied to incorrect kind of declaration." );

        internal static readonly DiagnosticDefinition<(string AspectType, string Exception)> ExceptionInWeaver =
            new( "LAMA0006", _category, "Exception occurred while executing the weaver of aspect '{0}': {1}", Error, "Exception in aspect weaver." );

        internal static readonly DiagnosticDefinition<(IDeclaration Member, int RequiredArgumentsCount, int ActualArgumentsCount)> MemberRequiresNArguments =
            new( "LAMA0012", _category, "Member '{0}' requires {1} arguments but received {2}.", Error, "Member requires number of arguments." );

        internal static readonly DiagnosticDefinition<(IDeclaration Member, int RequiredArgumentsCount, int ActualArgumentsCount)>
            MemberRequiresAtLeastNArguments =
                new( "LAMA0013", _category, "Member '{0}' requires at least {1} arguments but received {2}.", Error, "Member requires more arguments." );

        internal static readonly DiagnosticDefinition<IMemberOrNamedType> MustProvideInstanceForInstanceMember =
            new(
                "LAMA0015",
                _category,
                "Member {0} is not static, but has been used with a null instance.",
                Error,
                "Has to provide instance for an instance member." );

        internal static readonly DiagnosticDefinition<IMemberOrNamedType> CannotProvideInstanceForLocalFunction =
            new(
                "LAMA0018",
                _category,
                "{0} is a local function, so it Cannot be invoked with a non-null instance.",
                Error,
                "Cannot provide instance for a local function." );

        internal static readonly DiagnosticDefinition<(string Expression, string ParameterName, IMemberOrNamedType Method)>
            CannotPassExpressionToByRefParameter =
                new(
                    "LAMA0019",
                    _category,
                    "Cannot pass the expression '{0}' to the '{1}' parameter of method '{2}' because the parameter is 'out' or 'ref'.",
                    Error,
                    "Cannot use an expression in an out or ref parameter." );

        internal static readonly DiagnosticDefinition<(string TypeNane, string? AssemblyName)> CannotFindType =
            new( "LAMA0020", _category, "Cannot find the type '{0}' of assembly '{1}'.", Error, "Cannot find a type" );

        internal static readonly DiagnosticDefinition<string> CycleInAspectOrdering =
            new(
                "LAMA0021",
                _category,
                "A cycle was found in the specifications of aspect ordering between the following aspect part: {0}.",
                Error,
                "A cycle was found in aspect ordering." );

        internal static readonly DiagnosticDefinition<(string ParentType, string ChildType)> CannotAddChildAspectToPreviousPipelineStep = new(
            "LAMA0022",
            _category,
            "The aspect '{0}' cannot add a child aspect to of type '{1}' because this aspect type has already been processed.",
            Error,
            "Cannot add an aspect to a previous step of the compilation pipeline." );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Target)> CannotAddAdviceToPreviousPipelineStep = new(
            "LAMA0023",
            _category,
            "The aspect '{0}' cannot add an advice to '{1}' because this declaration has already been processed.",
            Error,
            "Cannot add an advice to a previous step of the compilation pipeline." );

        internal static readonly DiagnosticDefinition<(DeclarationKind ElementKind, ISymbol Symbol, ITypeSymbol AttributeType, string AdviceMethod)>
            TemplateMemberMissesAttribute = new(
                "LAMA0024",
                "The template member does not have the expected custom attribute.",
                "The template {0} '{1}' must be annotated with the custom attribute [{2}] otherwise it cannot be used with the dynamic advice '{3}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, string MethodName)> AspectMustHaveExactlyOneTemplateMember = new(
            "LAMA0025",
            "The aspect type must have exactly one member of a given name otherwise it cannot be used as a dynamic advice.",
            "The type '{0}' must have exactly one member named '{1}'.",
            _category,
            Error );

        internal static readonly DiagnosticDefinition<AssemblyIdentity> CannotFindCompileTimeAssembly = new(
            "LAMA0027",
            _category,
            "The assembly '{0}' required at compile-time cannot be found.",
            Error,
            "Cannot find an assembly required by the compile-time assembly." );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclarationBuilder MemberBuilder, INamedType AttributeType)>
            CompatibleAttributeConstructorDoesNotExist = new(
                "LAMA0028",
                _category,
                "The aspect '{0}' cannot add attribute '{1}' to member '{2}' because no compatible constructor exists for given types.",
                Error,
                "Compatible attribute constructor does not exist." );

        internal static readonly DiagnosticDefinition<string>
            InvalidCachedManifestFile = new(
                "LAMA0029",
                _category,
                "The cache file '{0}' was corrupted. It has been deleted. Please restart the compilation.",
                Error,
                "The compile-time project manifest file is corrupted." );

        internal static readonly DiagnosticDefinition<string>
            InvalidCompileTimeProjectResource = new(
                "LAMA0030",
                _category,
                "The compile-time project in assembly '{0}' is corrupted.",
                Error,
                "The compile-time project resource file was corrupted." );

        internal static readonly DiagnosticDefinition<(string TemplateName, string ClassName)>
            TemplateWithSameNameAlreadyDefined = new(
                "LAMA0032",
                _category,
                "The class '{1}' defines several templates named '{0}'. Template names must be unique.",
                Error,
                "The class already defines a template of the same name." );

        internal static readonly DiagnosticDefinition<(string ClassName, string MemberName)>
            MemberDoesNotHaveTemplateAttribute = new(
                "LAMA0033",
                _category,
                "The class '{0}' defines a member named '{1}', but the member is not annotated with the [Template] custom attribute.",
                Error,
                "The member does not have a template custom attribute." );

        internal static readonly DiagnosticDefinition<(string ClassName, string MemberName, string ExpectedAttribute, string ActualAttribute)>
            TemplateIsOfTheWrongType = new(
                "LAMA0034",
                _category,
                "The template '{0}.{1}' was expected to be annotated with the [{2}] attribute, but it is annotated with [{3}].",
                Error,
                "The member does not have a template custom attribute." );

        internal static readonly DiagnosticDefinition<(string Layer1, string Layer2)> UnorderedLayers = new(
            "LAMA0035",
            _category,
            "The aspect layers '{0}' and '{1}' are not strongly ordered. Add an [assembly: " + nameof(AspectOrderAttribute) +
            "(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.",
            Warning,
            "Two layers are not strongly ordered." );

        internal static readonly DiagnosticDefinition<(string TemplateName, string ClassName, string BaseClassName)>
            TemplateWithSameNameAlreadyDefinedInBaseClass = new(
                "LAMA0036",
                _category,
                "The class '{1}' defines a new template named '{0}', but the base class '{2}' already defines a template of the same name. Template names must be unique.",
                Error,
                "The class already defines a template of the same name." );

        internal static readonly DiagnosticDefinition<(string AspectName, IDeclaration Target, FormattableString Reason)>
            AspectNotEligibleOnTarget = new(
                "LAMA0037",
                _category,
                "The aspect '{0}' cannot be applied to '{1}' because {2}.",
                Error,
                "The aspect cannot be applied to a declaration because eligibility conditions are not met." );

        internal static readonly DiagnosticDefinition<(FormattableString Predecessor, string AspectType, IDeclaration Child, IDeclaration Parent)>
            CanAddChildAspectOnlyUnderParent = new(
                "LAMA0038",
                _category,
                "The {0} cannot add a child aspect of type '{1}' to '{2}' because it is not contained in '{3}'.",
                Error,
                "A parent aspect or fabric can add child aspects only under its target declaration." );

        internal static readonly DiagnosticDefinition<(FormattableString Predecessor, string AspectType, IDeclaration Child, FormattableString Reason)>
            IneligibleChildAspect = new(
                "LAMA0039",
                _category,
                "The {0} cannot add a child aspect of type '{1}' to '{2}' because {3}.",
                Error,
                "A parent aspect or fabric can add child aspects only under its target declaration." );

        internal static readonly DiagnosticDefinition<Type>
            TypeMustHavePublicDefaultConstructor = new(
                "LAMA0040",
                _category,
                "The  type '{0}' must have a default constructor.",
                Error,
                "The type must have a default constructor." );

        internal static readonly DiagnosticDefinition<(UserCodeMemberInfo TemplateSymbol, IDeclaration TargetDeclaration, string ExceptionType, string
                ExceptionMessage, string Details)>
            ExceptionInUserCodeWithTarget
                = new(
                    "LAMA0041",
                    "Exception in user code",
                    "'{0}' threw '{2}' when applied to '{1}': {3}. Exception details are in '{4}'. To attach a debugger to the compiler, use the " +
                    " '-p:MetalamaDebugCompiler=True' command-line option.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(UserCodeMemberInfo TemplateSymbol, string ExceptionType, string ExceptionMessage, string Details)>
            ExceptionInUserCodeWithoutTarget
                = new(
                    "LAMA0042",
                    "Exception in user code",
                    "'{0}' threw '{1}': {2}. Exception details are in '{3}'. To attach a debugger to the compiler, use the " +
                    " '-p:MetalamaDebugCompiler=True' command-line option.",
                    _category,
                    Error );

        public static readonly DiagnosticDefinition<string>
            SuggestedCodeFix
                = new(
                    "LAMA0043",
                    Hidden,
                    "Suggestion: {0}",
                    "Code fix suggestion",
                    _category );

        internal static readonly DiagnosticDefinition<(FormattableString Predecessor, IDeclaration Child, IDeclaration Parent)>
            CanAddValidatorOnlyUnderParent = new(
                "LAMA0044",
                _category,
                "The {0} cannot add a validator to '{1}' because it is not contained in '{2}'.",
                Error,
                "An aspect or fabric can add validators only under its target declaration." );

        internal static readonly DiagnosticDefinition<INamedTypeSymbol>
            LiveTemplateMustHaveDefaultConstructor = new(
                "LAMA0045",
                _category,
                "The class '{0}' must have a default constructor because of the [LiveTemplate] attribute.",
                Error,
                "Live templates must have a default constructor." );

        public static readonly DiagnosticDefinition<INamedType>
            TypeNotPartial
                = new(
                    "LAMA0048",
                    "The type must be made partial.",
                    "Aspects add members to '{0}' but it is not marked as 'partial'. Make the type 'partial' to make it possible to "
                    + "reference aspect-generated artefacts from source code.",
                    _category,
                    Warning );

        internal static readonly DiagnosticDefinition<(string Message, string File)> IgnorableUnhandledException =
            new(
                "LAMA00049",
                _category,
                "Unexpected exception occurred in Metalama: {0} Exception details are in '{1}'. " +
                " Please report this issue at https://www.postsharp.net/support and attach this file to the ticket.",
                Warning,
                "Unexpected exception in Metalama." );

        internal static readonly DiagnosticDefinition<(string WeaverType, string AspectType)> CannotFindAspectWeaver =
            new(
                "LAMA00050",
                _category,
                "The weaver type '{0}' required to weave aspect '{1}' is not found in the project. The weaver assembly must be included as an analyzer.",
                Error,
                "Cannot find an aspect weaver." );

        internal static readonly DiagnosticDefinition PreviewCSharpVersionNotSupported =
            new(
                "LAMA00051",
                Error,
                "Metalama does not support the 'preview' language version. Change the LangVersion property of your csproj file to 'latest'. "
                + "If you want to use preview features at your own risks, set the MSBuild property 'MetalamaAllowPreviewLanguageFeatures' to 'true'. It may work if you don't use preview features in templates.",
                "Metalama does not support the 'preview' C# language version",
                _category );

        // TODO: Use formattable string (C# does not seem to find extension methods).
        public static readonly DiagnosticDefinition<string>
            UnsupportedFeature = new(
                "LAMA0099",
                "Feature is not yet supported.",
                "Feature is not yet supported: {0}",
                _category,
                Error );
    }
}