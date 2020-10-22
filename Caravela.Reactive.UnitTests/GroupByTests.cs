using Xunit;

namespace Caravela.Reactive.UnitTests
{
    public class GroupByTests
    {

        [Fact]
        public void GroupByImmutableTest()
        {
            var source = new[] { 1, 2, 11 }.ToImmutableReactive();

            // with using System.Linq, LINQ on ReactiveHashSet above is ambiguous
            var grouped = System.Linq.Enumerable.ToList( source.GroupBy( i => i % 10 ).GetValue() );

            Assert.Equal( 2, grouped.Count );

            var group1 = grouped[0];
            Assert.Equal( 1, group1.Key );
            Assert.Equal( new[] { 1, 11 }, group1.GetValue() );

            var group2 = grouped[1];
            Assert.Equal( 2, group2.Key );
            Assert.Equal( new[] { 2 }, group2.GetValue() );
        }

        [Fact]
        public void EmptyGroupTest()
        {
            _ = new[] { 1, 2, 3 }.ToImmutableReactive()
                .GroupBy( i => i % 10 )
                [0];
        }
    }

    
    }

