// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics
{
    internal static class CodeDisplayFormatExtensions
    {
        private static readonly Dictionary<CodeDisplayFormat, SymbolDisplayFormat> _map = new()
        {
            { CodeDisplayFormat.FullyQualified, SymbolDisplayFormat.FullyQualifiedFormat },
            { CodeDisplayFormat.MinimallyQualified, SymbolDisplayFormat.MinimallyQualifiedFormat },
            { CodeDisplayFormat.DiagnosticMessage, SymbolDisplayFormat.CSharpErrorMessageFormat },
            { CodeDisplayFormat.ShortDiagnosticMessage, SymbolDisplayFormat.CSharpShortErrorMessageFormat }
        };

        public static SymbolDisplayFormat? ToRoslyn( this CodeDisplayFormat? codeDisplayFormat )
            => codeDisplayFormat == null ? SymbolDisplayFormat.CSharpShortErrorMessageFormat : _map[codeDisplayFormat];
    }
}