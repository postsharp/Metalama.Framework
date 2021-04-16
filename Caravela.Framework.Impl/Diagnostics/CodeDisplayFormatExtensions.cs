// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class CodeDisplayFormatExtensions
    {
        private static readonly Dictionary<CodeDisplayFormat, SymbolDisplayFormat> _map = new()
        {
            { CodeDisplayFormat.FullyQualified, SymbolDisplayFormat.FullyQualifiedFormat },
            { CodeDisplayFormat.MinimallyQualified, SymbolDisplayFormat.MinimallyQualifiedFormat },
            { CodeDisplayFormat.DiagnosticMessage, SymbolDisplayFormat.CSharpErrorMessageFormat },
            { CodeDisplayFormat.ShortDiagnosticMessage, SymbolDisplayFormat.CSharpShortErrorMessageFormat },
        };

        public static SymbolDisplayFormat? ToRoslyn( this CodeDisplayFormat? codeDisplayFormat )
            => codeDisplayFormat == null ? SymbolDisplayFormat.CSharpErrorMessageFormat : _map[codeDisplayFormat];
    }
}