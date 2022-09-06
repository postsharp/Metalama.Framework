// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Engine.Formatting
{
    internal enum EndOfLineStyle
    {
        Unknown = 0,
        CR = 1,
        LF = 2,
        CRLF = CR | LF,
        Unix = LF,
        MacOs = CR,
        Windows = CRLF
    }
}