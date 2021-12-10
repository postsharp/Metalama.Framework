// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable UnusedTypeParameter

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization
{
    public class GenericTestClass<T>
    {
        public class SecondSubType { }

        public class SecondSubType<T2> { }
    }
}