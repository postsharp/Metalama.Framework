﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Assets
{
    public class CustomComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T? x, T? y ) => true;

        public int GetHashCode( T obj ) => 0;
    }
}