// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable UnusedTypeParameter

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization
{
    public class GenericTestClass<T>
    {
        public sealed class SecondSubType { }

        public sealed class SecondSubType<T2> { }
    }
}