// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection
{
    public class ReflectionTestBase : SerializerTestsBase
    {
        public ITestOutputHelper Logger { get; }

        public ReflectionTestBase( ITestOutputHelper helper )
        {
            this.Logger = helper;
        }

        /// <summary>
        /// As <see cref="Assert.Equal{T}(T,T)"/>, except that if they are not equal, it prints the actual string on XUnit output, in verbatim string form,
        /// so that you can easily copy-paste it as the correct expected value into the test.
        /// </summary>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value from test.</param>
        public void AssertEqual( string expected, string actual )
        {
            if ( expected == actual )
            {
                // It's fine.
            }
            else
            {
                this.Logger.WriteLine( "Actual result to compare against:" );
                this.Logger.WriteLine( "----" );
                this.Logger.WriteLine( "@\"" + actual.ReplaceOrdinal( "\"", "\"\"" ) + '\"' );
                this.Logger.WriteLine( "----" );
                Assert.Equal( expected, actual );
            }
        }
    }
}