using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Caravela.Reactive.Sources;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    sealed class AspectCompilation
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public ICompilation Compilation { get; }

        public ReactiveHashSet<AspectSource> AspectSources { get; } = new();

        public IReactiveCollection<AspectInstance> Aspects { get; }

        public AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, ICompilation compilation)
            : this(diagnostics, compilation, new ReactiveHashSet<AspectSource>() { new AttributeAspectSource(compilation) }) { }

        private AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, ICompilation compilation, ReactiveHashSet<AspectSource> aspectSources)
        {
            this.Diagnostics = diagnostics;
            this.Compilation = compilation;
            this.AspectSources = aspectSources;

            this.Aspects = aspectSources.SelectMany(s => s.GetAspects());
        }

        public AspectCompilation Update(IReadOnlyList<Diagnostic> addedDiagnostics, ICompilation newCompilation)
        {
            var newDiagnostics = this.Diagnostics.Concat(addedDiagnostics).ToImmutableArray();

            return new AspectCompilation(newDiagnostics, newCompilation, this.AspectSources );
        }
    }
}
