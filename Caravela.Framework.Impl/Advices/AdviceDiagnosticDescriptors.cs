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
        // Reserved range 0-99

        private const string _category = "Caravela.Advices";

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "CRA0001",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceOverrideOfSealed = new(
                "CRA0002",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and is sealed.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceWithDifferentStaticity = new(
                "CRA0003",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and " +
                "its IsStatic flag is opposite of the introduced member.",
                Error, _category );

        public static readonly DiagnosticDefinition<(string AspectType, ICodeElement Member, ICodeElement TargetType)>
            CannotIntroduceInstanceMemberIntoStaticType = new(
                "CRA0004",
                "Cannot introduce instance member into a static type.",
                "The aspect '{0}' cannot introduce instance member '{1}' into a type '{2}' because it is static.",
                Error, _category );
    }
}