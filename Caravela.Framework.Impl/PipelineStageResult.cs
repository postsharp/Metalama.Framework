using System.Collections.Generic;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal sealed class PipelineStageResult
    {
        public CSharpCompilation Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<ResourceDescription> Resources { get; }

        public IReactiveCollection<AspectInstance> AspectInstances { get; }

        public PipelineStageResult( CSharpCompilation compilation, IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<ResourceDescription> resources, IReactiveCollection<AspectInstance> aspectInstances )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
            this.Resources = resources;
            this.AspectInstances = aspectInstances;
        }
    }
}
