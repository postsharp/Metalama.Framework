using Caravela.Reactive.Sources;
using System.Collections.Immutable;

namespace Caravela.Reactive.UnitTests
{
    class TestCompilation
    {
        public ReactiveHashSet<SourceType> Types { get; } = new();
    }

    record SourceType( string Name, IImmutableList<string> BaseTypes )
    {
        public ReactiveHashSet<Member> Members { get; } = new();
        public ReactiveHashSet<SourceType> NestedTypes { get; } = new();
    }

    record Member( string Name );
}
