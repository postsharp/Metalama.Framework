// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Aspects;

internal static class DeclarativeAdviceDiagnosticDescriptors
{
    internal static readonly DiagnosticDefinition<(string AspectType, DeclarationKind IntroducedDeclarationKind, DeclarationKind TargetDeclarationKind)>
        CannotUseIntroduceWithoutDeclaringType = new(
            "LAMA0501",
            "Cannot use [Introduce] in an aspect that is applied to a declaration that is neither a type nor a type member.",
            "The aspect '{0}' cannot introduce a {1} because it has been applied to a {2}, which is neither a type nor a type member.",
            "Metalama.Advices",
            Severity.Error );
}