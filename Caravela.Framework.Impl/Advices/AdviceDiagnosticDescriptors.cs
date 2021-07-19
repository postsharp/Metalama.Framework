// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using static Caravela.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceDiagnosticDescriptors
    {
        // Reserved range 500-599

        private const string _category = "Caravela.Advices";

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "CR0500",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, DeclarationKind TargetDeclarationKind)>
            CannotUseIntroduceWithoutDeclaringType = new(
                "CR0501",
                "Cannot use [Introduce] in an aspect that is applied to a declaration that is neither a type nor a type member.",
                "The aspect '{0}' cannot introduce a {1} because it has been applied to a {2}, which is neither a type nor a type member.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceOverrideOfSealed = new(
                "CR0502",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' " +
                "and is static, non-virtual or sealed.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType, IType ReturnType)>
            CannotIntroduceDifferentExistingReturnType = new(
                "CR0503",
                "Cannot introduce member into a type because it has a different return type in the base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' " +
                "and has return type '{4}'.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceWithDifferentStaticity = new(
                "CR0504",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and " +
                "its IsStatic flag is opposite of the introduced member.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceInstanceMemberIntoStaticType = new(
                "CR0505",
                "Cannot introduce instance member into a static type.",
                "The aspect '{0}' cannot introduce instance member '{1}' into a type '{2}' because it is static.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember InterfaceMember)>
            MissingDeclarativeInterfaceMember = new(
                "CR0510",
                "Declarative interface member introduction is missing.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into the type '{2}' because it" +
                " does not contain a declarative introduction (using [Introduce]) for the interface member '{3}'.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember DeclarativeIntroduction,
                IMember InterfaceMember)>
            DeclarativeInterfaceMemberDoesNotMatch = new(
                "CR0511",
                "Declarative interface member introduction does match interface member return type.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into the  type '{2}' because the introduced member '{3}'" +
                " does not have the same return type as interface member '{4}'.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType)>
            InterfaceIsAlreadyImplemented = new(
                "CR0512",
                "Cannot introduce an interface when the target type already implements it.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' because it is already implemented and WhenExists is set to Fail.",
                _category, Error );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType)>
            InterfaceIsAlreadyIntroducedByTheAspect = new(
                "CR0513",
                "Cannot introduce an interface was already introduced by the aspect.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' because it has already introduced an implementation of this interface. " +
                "If interface introductions with shared .",
                _category, Error );
    }
}