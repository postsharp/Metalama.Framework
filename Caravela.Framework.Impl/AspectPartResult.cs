using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    sealed class AspectPartResult
    {
        private readonly IImmutableList<AspectSource> _aspectSources;

        // TODO: should this be reactive or handled as a side value?
        public CompilationModel Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<ResourceDescription> Resources { get; }

        public IReactiveCollection<AspectInstance> Aspects { get; }

        public IReactiveCollection<Transformation> Transformations { get; }

        public IReactiveGroupBy<string, AspectInstance> AspectsByAspectType { get; }

        public AspectPartResult( CompilationModel compilation, CompileTimeAssemblyLoader loader )
            : this(
                  Array.Empty<Diagnostic>(), Array.Empty<ResourceDescription>(), compilation,
                  ImmutableList.Create<AspectSource> ( new AttributeAspectSource( compilation, loader ) ) ) { }

        private AspectPartResult(
            IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<ResourceDescription> resources, CompilationModel compilation, IImmutableList<AspectSource> aspectSources)
        {
            this.Diagnostics = diagnostics;
            this.Resources = resources;
            this.Compilation = compilation;
            this._aspectSources = aspectSources;

            this.Aspects = aspectSources.ToReactive().SelectMany(s => s.GetAspects());
            this.AspectsByAspectType = this.Aspects.GroupBy( ai => ai.AspectType.FullName );
        }

        public AspectPartResult Update( IReadOnlyList<Diagnostic> addedDiagnostics, IReadOnlyList<ResourceDescription> addedResources, CompilationModel newCompilation )
        {
            var newDiagnostics = this.Diagnostics.Concat(addedDiagnostics).ToImmutableList();
            var newResources = this.Resources.Concat( addedResources ).ToImmutableList();

            return new( newDiagnostics, newResources, newCompilation, this._aspectSources );
        }

        internal AspectPartResult Update( IReactiveCollection<AspectInstanceResult> instanceResults )
        {
            instanceResults = instanceResults.Materialize();

            var addedDiagnostics = instanceResults.SelectMany( air => air.Diagnostics );
            var addedAdvices = instanceResults.SelectMany( air => air.Advices );
            var addedAspects = instanceResults.SelectMany( air => air.Aspects );

            var newDiagnostics = this.Diagnostics.Concat( addedDiagnostics.GetValue() ).ToImmutableList();
            var newCompilation = this.Compilation;
            var newAspectSources = this._aspectSources.Add( new ReactiveAspectSource( addedAspects ) );

            return new( newDiagnostics, Array.Empty<ResourceDescription>(), newCompilation, newAspectSources );
        }
    }
}
