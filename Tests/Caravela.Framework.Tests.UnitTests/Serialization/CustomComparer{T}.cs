﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{

    public class CustomComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T? x, T? y ) => true;

        public int GetHashCode( T obj ) => 0;
    }
}