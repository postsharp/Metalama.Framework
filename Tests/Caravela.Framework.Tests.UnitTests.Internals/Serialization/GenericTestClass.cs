// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable UnusedTypeParameter

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class GenericTestClass<T>
    {
        public class SecondSubType { }

        public class SecondSubType<T2> { }
    }
}