// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl
{
    internal static class AdviceDiagnosticDescriptors
    {
        // Reserved range 0-99

        private const string _category = "Caravela.Advices";

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceMemberAlreadyExists = new(
                "CRA0001",
                "Cannot introduce member into a type because it already exists.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}'.",
                _category,
                Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceOverrideOfSealed = new(
                "CRA0002",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and is sealed.",
                _category,
                Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, ICodeElement Member, ICodeElement TargetType, ICodeElement DeclaringType)>
            CannotIntroduceWithDifferentStaticity = new(
                "CRA0003",
                "Cannot introduce member into a type because it is sealed in a base class.",
                "The aspect '{0}' cannot introduce member '{1}' into type '{2}' because it is already defined in type '{3}' and it's IsStatic flag is opposite of the introduced member."
               ,
                _category,
                Error );

        public static readonly StrongDiagnosticDescriptor<(IType AspectType, ICodeElement Member, ICodeElement TargetType)>
            CannotIntroduceInstanceMemberIntoStaticType = new(
                "CRA0003",
                "Cannot introduce instance member into a static type.",
                "The aspect '{0}' cannot introduce instance member '{1}' into a type '{2}' because it is static.",
                _category,
                Error );
    }
}