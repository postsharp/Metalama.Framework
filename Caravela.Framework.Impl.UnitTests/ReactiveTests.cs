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
        public void SelectManyTests()
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
    }
}
