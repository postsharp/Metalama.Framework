using Caravela.Reactive.Sources;
using Xunit;

namespace Caravela.Reactive.UnitTests
{
    public class SomeTests
    {

        [Fact]
        public void SomeTest()
        {
            var source = new ReactiveHashSet<int>();

            var some = source.Some();

            source.Add( 1 );

            Assert.Equal( 1, some.GetValue( default ) );

            source.Replace( 1, 2 );

            Assert.Equal( 2, some.GetValue( default ) );

            source.Add( 3 );

            Assert.Equal( 2, some.GetValue( default ) );

            source.Remove( 2 );

            Assert.Equal( 3, some.GetValue( default ) );
        }
    }


}

