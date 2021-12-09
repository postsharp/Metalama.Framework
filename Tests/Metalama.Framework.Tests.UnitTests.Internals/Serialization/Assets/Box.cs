// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Tests.UnitTests.Serialization.Assets
{
    internal class Box<T>
    {
        public T? Value { get; set; }

        public class InnerBox
        {
            public enum Shiny
            {
                Yes,
                No
            }
        }

        [Flags]
        public enum Color
        {
            Blue = 4,
            Red = 8
        }
    }
}