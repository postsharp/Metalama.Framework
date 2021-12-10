// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

// ReSharper disable IdentifierTypo

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Assets
{
    [Flags]
    internal enum WorldFeatures : ulong
    {
        Icy = 1,
        Edenlike = 2,
        Poisonous = 4,
        Volcanic = 8
    }
}