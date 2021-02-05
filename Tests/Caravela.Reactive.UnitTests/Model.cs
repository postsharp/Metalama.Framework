using Caravela.Reactive.Sources;
using System.Collections.Immutable;

namespace Caravela.Reactive.UnitTests
{
    class TestCompilation
    {
        public ReactiveHashSet<SourceType> Types { get; } = new();
    }

    class SourceType

    {
        public SourceType( string name, IImmutableList<string>? baseTypes )
        {
            this.Name = name;
            this.BaseTypes = baseTypes ?? ImmutableList<string>.Empty;

        }
        public string Name { get; }
        public IImmutableList<string> BaseTypes { get; }
        public ReactiveHashSet<Member> Members { get; } = new();
        public ReactiveHashSet<SourceType> NestedTypes { get; } = new();
    }

    record Member( string Name );
}
