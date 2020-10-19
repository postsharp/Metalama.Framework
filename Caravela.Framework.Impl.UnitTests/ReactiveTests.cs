using System.Collections.Immutable;
using Caravela.Reactive;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class ReactiveTests
    {
        class Compilation
        {
            public ReactiveHashSet<SourceType> Types { get; } = new();
        }

        record SourceType(string Name, IImmutableList<string> baseTypes)
        {
            public ReactiveHashSet<Member> Members { get; } = new();
        }

        record Member(string Name);

        [Fact]
        public void SelectManyTest()
        {
            var compilation = new Compilation();

            var memberNames = from type in compilation.Types
                              from member in type.Members
                              select $"{type.Name}.{member.Name}";

            var baseTypeNames = from type in compilation.Types
                                from baseType in type.baseTypes
                                select $"{type.Name} : {baseType}";

            Assert.Empty(memberNames.GetValue(default));
            Assert.Empty(baseTypeNames.GetValue(default));

            var c = new SourceType("C", ImmutableList.Create("B"));

            compilation.Types.Add(c);

            Assert.Empty(memberNames.GetValue(default));
            Assert.Equal(new[] { "C : B" }, baseTypeNames.GetValue(default));

            c.Members.Add(new Member("M"));

            Assert.Equal(new[] { "C.M" }, memberNames.GetValue(default));
        }

        [Fact]
        public void SomeTest()
        {
            var source = new ReactiveHashSet<int>();

            var some = source.Some();

            source.Add(1);

            Assert.Equal(1, some.GetValue(default));

            source.Replace(1, 2);

            Assert.Equal(2, some.GetValue(default));

            source.Add(3);

            Assert.Equal(2, some.GetValue(default));

            source.Remove(2);

            Assert.Equal(3, some.GetValue(default));
        }

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
    }
}
