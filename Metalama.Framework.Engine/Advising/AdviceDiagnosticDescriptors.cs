﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Metalama.Framework.Engine.Advising
{
    public static class AdviceDiagnosticDescriptors
    {
        // Reserved range 500-599.

        private const string _category = "Metalama.Advices";

        // Sub-range 500-509: General introduction diagnostics.

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
            CannotIntroduceInstanceMember = new(
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

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member)>
            CannotIntroduceStaticVirtualMember = new(
                "LAMA0507",
                "Cannot introduce virtual member because it is also static.",
                "The aspect '{0}' cannot introduce virtual member '{1}' because it is also static.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member)>
            CannotIntroduceStaticSealedMember = new(
                "LAMA0508",
                "Cannot introduce sealed member because it is also static.",
                "The aspect '{0}' cannot introduce sealed member '{1}' because it is also static.",
                _category,
                Error );

        // Sub-range 510-519: Interface implementation diagnostics.

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember InterfaceMember)>
            MissingDeclarativeInterfaceMember = new(
                "LAMA0510",
                "Declarative interface member introduction is missing.",
                "The aspect '{0}' cannot implicitly introduce interface '{2}' into the type '{1}' because it" +
                " does not contain a declarative introduction (using [InterfaceMember]) for the interface member '{3}'.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType TargetType, INamedType InterfaceType, IMember DeclarativeIntroduction,
                IMember InterfaceMember)>
            DeclarativeInterfaceMemberDoesNotMatch = new(
                "LAMA0511",
                "Declarative interface member introduction does match interface member return type.",
                "The aspect '{0}' cannot implicitly introduce interface '{2}' into the type '{1}' because the introduced member '{3}'" +
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

        internal static readonly DiagnosticDefinition<(string AspectType, IMember InterfaceMember, INamedType TargetType, IMember ExistingDeclaration)>
            ImplicitInterfaceMemberAlreadyExists = new(
                "LAMA0514",
                "Cannot introduce an implicit interface member when the target type already contains a declaration with the same signature.",
                "The aspect '{0}' cannot introduce interface member '{1}' into type '{2}' because the type already contains '{3}' which has the same signature "
                +
                "and WhenExists of the interface member is set to Fail.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IMember InterfaceMember, INamedType TargetType, IMember ExistingDeclaration)>
            ImplicitInterfaceMemberIsNotCompatible = new(
                "LAMA0515",
                "Cannot introduce an implicit interface member when the target type already contains a declaration that is not compatible with the interface member.",
                "The aspect '{0}' cannot introduce interface member '{1}' into type '{2}' because the type already contains '{3}' which has the same signature "
                +
                "but is incompatible with the interface member and WhenExists of the interface member is set to UseExisting.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType InterfaceType, INamedType TargetType, OverrideStrategy Strategy)>
            InterfaceUnsupportedOverrideStrategy = new(
                "LAMA0516",
                "Using unsupported override strategy for interface type.",
                "The aspect '{0}' cannot introduce interface '{1}' into type '{2}' with 'whenExists={3}' because it is not supported." +
                "Only Ignore or Fail strategies are supported for interface types. You can use 'whenExists' on individual members.",
                _category,
                Error );

        // Sub-range 520-549: Various introduction diagnostics.
        internal static readonly DiagnosticDefinition<(string AspectType, IConstructor Constructor)>
            CannotIntroduceParameterIntoStaticConstructor = new(
                "LAMA0520",
                "Cannot introduce a parameter into a static constructor.",
                "The aspect '{0}' cannot introduce a parameter into '{1}' because it is a static constructor.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType AttributeType, IDeclaration TargetDeclaration)>
            AttributeAlreadyPresent = new(
                "LAMA0521",
                "Cannot introduce a custom attribute when the attribute is already present on the target declaration.",
                "The aspect '{0}' cannot introduce the custom attribute '{1}' into '{2}' because it this attribute is already present on the declaration and WhenExists is set to Fail.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration TargetType, OverrideStrategy OverrideStrategy)>
            CannotUseNewOverrideStrategyWithFinalizers = new(
                "LAMA0522",
                "Invalid override strategy when introducing a finalizer.",
                "The aspect '{0}' cannot introduce finalizer into type '{1}' because the specified override strategy '{2}' is not valid.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IConstructor Constructor)>
            CannotIntroduceParameterIntoNonClassConstructor = new(
                "LAMA0523",
                "Cannot introduce a parameter into a constructor of a type that is not a class.",
                "The aspect '{0}' cannot introduce a parameter into '{1}' because the target type is not a class.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, INamedType Constructor)>
            CannotAddInitializerToRecord = new(
                "LAMA0524",
                "Cannot add an initializer to a record.",
                "The aspect '{0}' cannot add an initializer to '{1}' because it is a record.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceVirtualToTargetType = new(
                "LAMA0525",
                "Cannot introduce virtual member into a static type, sealed type or a struct.",
                "The aspect '{0}' cannot introduce virtual member '{1}' into a type '{2}' because it is static, sealed or a struct.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceIndexerWithoutParameters = new(
                "LAMA0526",
                "Cannot introduce indexer without any parameters.",
                "The aspect '{0}' cannot introduce indexer '{1}' into type '{2}' because it has no parameters.",
                _category,
                Error );

        internal static readonly DiagnosticDefinition<(string AspectType, IDeclaration Member, IDeclaration TargetType)>
            CannotIntroduceStaticIndexer = new(
                "LAMA0527",
                "Cannot introduce static indexer.",
                "The aspect '{0}' cannot introduce indexer '{1}' into type '{2}' because it is static.",
                _category,
                Error );
    }
}