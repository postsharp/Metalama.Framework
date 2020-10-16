using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    sealed class AspectCompilation
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public ICompilation Compilation { get; }

        public ReactiveHashSet<AspectSource> AspectSources { get; } = new();

        public IReactiveCollection<AspectInstance> Aspects { get; }

        public AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, ICompilation compilation, AspectTypeFactory aspectTypeFactory)
            : this(diagnostics, compilation, new ReactiveHashSet<AspectSource>() { new AttributeAspectSource(compilation, aspectTypeFactory) }) { }

        private AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, ICompilation compilation, ReactiveHashSet<AspectSource> aspectSources)
        {
            Diagnostics = diagnostics;
            Compilation = compilation;
            AspectSources = aspectSources;

            Aspects = aspectSources.SelectMany(s => s.GetAspects());
        }

        public AspectCompilation Update(IReadOnlyList<Diagnostic> addedDiagnostics, ICompilation newCompilation)
        {
            var newDiagnostics = Diagnostics.Concat(addedDiagnostics).ToImmutableArray();

            return new AspectCompilation(newDiagnostics, newCompilation, AspectSources);
        }
    }
}
