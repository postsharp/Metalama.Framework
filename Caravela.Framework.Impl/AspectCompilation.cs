using System;
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

        public IReadOnlyList<ResourceDescription> Resources { get; }

        public BaseCompilation Compilation { get; }

        public IReactiveCollection<AspectInstance> Aspects { get; }

        public IReactiveGroupBy<string, AspectInstance> AspectsByAspectType { get; }

        public AspectCompilation( BaseCompilation compilation, CompileTimeAssemblyLoader loader )
            : this(
                  Array.Empty<Diagnostic>(), Array.Empty<ResourceDescription>(), compilation,
                  ImmutableArray.Create<AspectSource> ( new AttributeAspectSource( compilation, loader ) ) ) { }

        private AspectCompilation(
            IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<ResourceDescription> resources, BaseCompilation compilation, ImmutableArray<AspectSource> aspectSources)
        {
            this.Diagnostics = diagnostics;
            this.Resources = resources;
            this.Compilation = compilation;
            this._aspectSources = aspectSources;

            this.Aspects = aspectSources.ToReactive().SelectMany(s => s.GetAspects());
            this.AspectsByAspectType = this.Aspects.GroupBy( ai => ai.AspectType.FullName );
        }

        public AspectCompilation Update( IReadOnlyList<Diagnostic> addedDiagnostics, IReadOnlyList<ResourceDescription> addedResources, BaseCompilation newCompilation )
        {
            var newDiagnostics = this.Diagnostics.Concat(addedDiagnostics).ToImmutableArray();
            var newResources = this.Resources.Concat( addedResources ).ToImmutableArray();

            return new( newDiagnostics, newResources, newCompilation, this._aspectSources );
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

            return new( newDiagnostics, Array.Empty<ResourceDescription>(), newCompilation, newAspectSources );
        }
    }
}
