using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class ReflectionTestBase : TestBase
    {
        private readonly ITestOutputHelper _helper;

        public ReflectionTestBase( ITestOutputHelper helper )
        {
            this._helper = helper;
        }
        

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
                Assert.Equal( expected, actual );
            }
        }
    }
}