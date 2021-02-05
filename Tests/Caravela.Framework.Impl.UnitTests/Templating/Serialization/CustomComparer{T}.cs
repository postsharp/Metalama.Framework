using System;
using System.Collections.Generic;
using System.Globalization;
using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{

    public class CustomComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T? x, T? y ) => true;

        public int GetHashCode( T obj ) => 0;
    }
}