// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Aspects;

// Range: 0700-0799

internal static class FrameworkDiagnosticDescriptors
{
    internal static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, DeclarationKind TargetDeclarationKind)>
        CannotUseIntroduceWithoutDeclaringType = new(
            "LAMA0700",
            "Cannot use [Introduce] in an aspect that is applied to a declaration that is neither a type nor a type member.",
            "The aspect '{0}' cannot introduce a {1} because it has been applied to a {2}, which is neither a type nor a type member.",
            "Metalama.Advices",
            Severity.Error );

    internal static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, Code.TypeKind TargetTypeKind)>
        CannotApplyAdviceOnTypeOrItsMembers = new(
            "LAMA0701",
            "Cannot use [Introduce] in an aspect that is applied to a unsupported type or it's member.",
            "The aspect '{0}' cannot introduce a {1} because {2} is not a supported target type.",
            "Metalama.Advices",
            Severity.Error );
}