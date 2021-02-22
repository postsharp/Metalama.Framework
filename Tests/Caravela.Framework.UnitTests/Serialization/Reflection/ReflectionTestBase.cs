using Xunit.Abstractions;

namespace Caravela.Framework.UnitTests.Templating.Serialization.Reflection
{
    public class ReflectionTestBase : TestBase
    {
        private readonly ITestOutputHelper _helper;

        public ReflectionTestBase( ITestOutputHelper helper )
        {
            this._helper = helper;
        }

        /// <summary>
        /// As <see cref="Xunit.Assert.Equal{T}(T,T)"/>, except that if they are not equal, it prints the actual string on XUnit output, in verbatim string form,
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
                this._helper.WriteLine( "Actual result to compare against:" );
                this._helper.WriteLine( "----" );
                this._helper.WriteLine( "@\"" + actual.Replace( "\"", "\"\"" ) + '\"' );
                this._helper.WriteLine( "----" );
                Xunit.Assert.Equal( expected, actual );
            }
        }
    }
}