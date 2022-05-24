// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Metalama.Framework.Engine.Advices
{
    public static class AdviceDiagnosticDescriptors
    {
        // Reserved range 500-599

        private const string _category = "Metalama.Advices";

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "LAMA0500",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, DeclarationKind TargetDeclarationKind)>
            CannotUseIntroduceWithoutDeclaringType = new(
                "LAMA0501",
                "Cannot use [Introduce] in an aspect that is applied to a declaration that is neither a type nor a type member.",
                "The aspect '{0}' cannot introduce a {1} because it has been applied to a {2}, which is neither a type nor a type member.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceOverrideOfSealed = new(
                "LAMA0502",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' " +
                "and is static, non-virtual or sealed.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType, IType
                ReturnType)>
            CannotIntroduceDifferentExistingReturnType = new(
                "LAMA0503",
                "Cannot introduce member into a type because it has a different type or return type.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' " +
                "and has a different type or return type '{4}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceWithDifferentStaticity = new(
                "LAMA0504",
                "Cannot introduce member into a type because the type already contains a member of the same name or signature but with a different staticity.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and " +
                "its IsStatic flag is opposite of the introduced member.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceInstanceMemberIntoStaticType = new(
                "LAMA0505",
                "Cannot introduce instance member into a static type.",
                "The aspect '{0}' cannot introduce instance member '{1}' into a type '{2}' because it is static.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember InterfaceMember)>
            MissingDeclarativeInterfaceMember = new(
                "LAMA0510",
                "Declarative interface member introduction is missing.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into the type '{2}' because it" +
                " does not contain a declarative introduction (using [InterfaceMember]) for the interface member '{3}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember DeclarativeIntroduction,
                IMember InterfaceMember)>
            DeclarativeInterfaceMemberDoesNotMatch = new(
                "LAMA0511",
                "Declarative interface member introduction does match interface member return type.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into the  type '{2}' because the introduced member '{3}'" +
                " does not have the same return type as interface member '{4}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType)>
            InterfaceIsAlreadyImplemented = new(
                "LAMA0512",
                "Cannot introduce an interface when the target type already implements it.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' because it is already implemented and WhenExists is set to Fail.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType)>
            InterfaceIsAlreadyIntroducedByTheAspect = new(
                "LAMA0513",
                "Cannot introduce an interface was already introduced by the aspect.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' because there is already introduced an implementation of this interface. " +
                "This happens when you introduce an interface after introducing another interface that extends it.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, DeclarationKind DeclarationKind)>
            CannotIntroduceWithDifferentKind = new(
                "LAMA0514",
                "Cannot introduce member into a type because another member of a different kind already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because there is already a {3} of the same name in the type.",
                _category,
                Error );
    }
}