using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    sealed class AspectCompilation
    {
        private readonly ImmutableArray<AspectSource> _aspectSources;

        // TODO: should this be reactive or handled as a side value?
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public BaseCompilation Compilation { get; }

        public IReactiveCollection<AspectInstance> Aspects { get; }

        public IReactiveGroupBy<string, AspectInstance> AspectsByAspectType { get; }

        public AspectCompilation( IReadOnlyList<Diagnostic> diagnostics, BaseCompilation compilation, CompileTimeAssemblyLoader loader )
            : this( diagnostics, compilation, ImmutableArray.Create<AspectSource> ( new AttributeAspectSource( compilation, loader ) ) ) { }

        private AspectCompilation(IReadOnlyList<Diagnostic> diagnostics, BaseCompilation compilation, ImmutableArray<AspectSource> aspectSources)
        {
            this.Diagnostics = diagnostics;
            this.Compilation = compilation;
            this._aspectSources = aspectSources;

            this.Aspects = aspectSources.ToReactive().SelectMany(s => s.GetAspects());
            this.AspectsByAspectType = this.Aspects.GroupBy( ai => ai.AspectType.FullName );
        }

        public AspectCompilation Update(IReadOnlyList<Diagnostic> addedDiagnostics, BaseCompilation newCompilation )
        {
            var newDiagnostics = this.Diagnostics.Concat(addedDiagnostics).ToImmutableArray();

            return new( newDiagnostics, newCompilation, this._aspectSources );
        }

        internal AspectCompilation Update( IReactiveCollection<AspectInstanceResult> instanceResults )
        {
            instanceResults = instanceResults.Materialize();

            var addedDiagnostics = instanceResults.SelectMany( air => air.Diagnostics );
            var addedAdvices = instanceResults.SelectMany( air => air.Advices );
            var addedAspects = instanceResults.SelectMany( air => air.Aspects );

            var newDiagnostics = this.Diagnostics.Concat( addedDiagnostics.GetValue() ).ToImmutableArray();
            var newCompilation = new ModifiedCompilation( this.Compilation, addedAdvices );
            var newAspectSources = this._aspectSources.Add( new ReactiveAspectSource( addedAspects ) );

            return new( newDiagnostics, newCompilation, newAspectSources );
        }
    }
}
