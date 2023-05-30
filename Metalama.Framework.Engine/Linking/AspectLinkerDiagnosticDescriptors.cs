// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Metalama.Framework.Diagnostics.Severity;

// ReSharper disable SA1118

namespace Metalama.Framework.Engine.Linking
{
    public static class AspectLinkerDiagnosticDescriptors
    {
        // Reserved range 650-699

        private const string _category = "Metalama.Linker";

        internal static readonly DiagnosticDefinition<(string AspectType, ISymbol TargetDeclaration)>
            DeclarationMustBeInlined = new(
                "LAMA0699",
                "Declaration must be inlined.",
                "Version of declaration '{1} provided by '{0}' cannot be inlined. It is not currently possible to generate non-inlined code for this declaration.",
                _category,
                Error );
    }
}