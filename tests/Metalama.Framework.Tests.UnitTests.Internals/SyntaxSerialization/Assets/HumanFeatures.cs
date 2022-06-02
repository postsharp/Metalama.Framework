// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Assets
{
    [Flags]
    internal enum HumanFeatures : byte
    {
        Tall = 1,
        Old = 2,
        Smart = 4,
        Wise = 8
    }
}