using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal sealed class PipelineStageResult
    {
        public CompilationModel Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<ResourceDescription> Resources { get; }

        public IReactiveCollection<AspectInstance> AspectInstances { get; }

        public PipelineStageResult( CompilationModel compilation, IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<ResourceDescription> resources, IReactiveCollection<AspectInstance> aspectInstances )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
            this.Resources = resources;
            this.AspectInstances = aspectInstances;
        }
    }
}
