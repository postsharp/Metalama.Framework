﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Metalama.Framework.Engine.Advising
{
    public static class AdviceDiagnosticDescriptors
    {
        // Reserved range 500-599

        private const string _category = "Metalama.Advices";

        // Subrange 500-509: General introduction diagnostics.

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, IDeclaration DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "LAMA0500",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
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

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType, DeclarationKind DeclarationKind)>
            CannotIntroduceWithDifferentKind = new(
                "LAMA0506",
                "Cannot introduce member into a type because another member of a different kind already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because there is already a {3} of the same name in the type.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType AttributeType, IDeclaration TargetDeclaration)>
            AttributeAlreadyPresent = new(
                "LAMA0507",
                "Cannot introduce a custom attribute when the attribute is already present on the target declaration.",
                "The aspect '{0}' cannot introduce the custom attribute '{1}' into '{2}' because it this attribute is already present on the declaration and WhenExists is set to Fail.",
                _category,
                Error );

        // Subrange 510-519: Interface implementation diagnostics.

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

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType, IMember InterfaceMember)>
            ImplicitInterfaceMemberConflict = new(
                "LAMA0514",
                "Cannot introduce an implicit interface member when the target type already contains a declaration with the same signature.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' because the type already contains '{3}' and WhenExists is set to Fail.",
                _category,
                Error );

        // 520-529: Parameter introduction diagnostics.
        internal static readonly DiagnosticDefinition<(string AspectType, IConstructor Constructor)>
            CannotIntroduceParameterIntoStaticConstructor = new(
                "LAMA0520",
                "Cannot introduce a parameter into a static constructor.",
                "The aspect '{0}' cannot introduce a parameter into '{1}' because it is a static constructor.",
                _category,
                Error );
    }
}