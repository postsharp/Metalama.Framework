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
        // Reserved range 1000-1099

        private const string _category = "Caravela.Advices";

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "CR1000",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceOverrideOfSealed = new(
                "CR1001",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and is sealed.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceWithDifferentStaticity = new(
                "CR1002",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and " +
                "its IsStatic flag is opposite of the introduced member.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceInstanceMemberIntoStaticType = new(
                "CR1003",
                "Cannot introduce instance member into a static type.",
                "The aspect '{0}' cannot introduce instance member '{1}' into a type '{2}' because it is static.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember InterfaceMember)>
            MissingDeclarativeInterfaceMemberIntroduction = new(
                "CR1004",
                "Declarative interface member introduction is missing.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into a type '{2}' because it does not contain a declarative introduction corresponding to '{3}'.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember DeclarativeIntroduction,
                IMember InterfaceMember)>
            DeclarativeInterfaceMemberIntroductionDoesNotMatch = new(
                "CR1004",
                "Declarative interface member introduction does match interface member return type.",
                "The aspect '{0}' cannot implicitly introduce interface '{1}' into a type '{2}' because the declarative introduction '{3}' does not have the same return type as interface member '{4}'.",
                Error, _category );
    }
}