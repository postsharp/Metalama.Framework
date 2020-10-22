using Caravela.Reactive.Sources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caravela.Reactive.UnitTests
{
    class TestCompilation
    {
        public ReactiveHashSet<SourceType> Types { get; } = new();
    }

    record SourceType( string Name, IImmutableList<string> BaseTypes )
    {
        public ReactiveHashSet<Member> Members { get; } = new();
    }

    record Member( string Name );
}
