// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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