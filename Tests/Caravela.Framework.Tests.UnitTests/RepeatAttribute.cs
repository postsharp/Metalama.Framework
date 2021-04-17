// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Caravela.Framework.Tests.UnitTests
{
    public class RepeatAttribute : DataAttribute
    {
        private readonly int _count;

        public RepeatAttribute( int count )
        {
            if ( count < 1 )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Repeat count must be greater than 0." );
            }

            this._count = count;
        }

        public override IEnumerable<object[]> GetData( MethodInfo testMethod )
        {
            return Enumerable.Range( 0, this._count ).Select( i => new object[] { i } );
        }
    }
}