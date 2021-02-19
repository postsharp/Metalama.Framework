using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    [CompileTime]
    public interface IAspectWeaver : IAspectDriver
    {
        CSharpCompilation Transform( AspectWeaverContext context );
    }

    public sealed class AspectWeaverContext
    {
        public INamedType AspectType { get; }

        public IReadOnlyList<AspectInstance> AspectInstances { get; }

        public CSharpCompilation Compilation { get; }


        private readonly Action<Diagnostic> _addDiagnostic;

        // TODO: suport reading existing resources
        private readonly Action<ResourceDescription> _addManifestResource;

        public void AddManifestResource( ResourceDescription resource ) => this._addManifestResource( resource );

        internal AspectWeaverContext(
            INamedType aspectType,
            IReadOnlyList<AspectInstance> aspectInstances,
            CSharpCompilation compilation,
            Action<Diagnostic> addDiagnostic,
            Action<ResourceDescription> addManifestResource )
        {
            this.AspectType = aspectType;
            this.AspectInstances = aspectInstances;
            this.Compilation = compilation;
            this._addDiagnostic = addDiagnostic;
            this._addManifestResource = addManifestResource;
        }

        public void ReportDiagnostic( Diagnostic diagnostic ) => this._addDiagnostic( diagnostic );

    }

}
