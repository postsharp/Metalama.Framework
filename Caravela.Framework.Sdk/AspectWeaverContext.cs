// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Context for the <see cref="IAspectWeaver"/>.
    /// </summary>
    public sealed class AspectWeaverContext
    {
        /// <summary>
        /// Gets the type of aspects that must be handled.
        /// </summary>
        public IAspectType AspectType { get; }

        /// <summary>
        /// Gets the set of aspect instances that must be weaved.
        /// </summary>
        public IReadOnlyList<IAspectInstance> AspectInstances { get; }

        /// <summary>
        /// Gets the input <see cref="CSharpCompilation"/>.
        /// </summary>
        public CSharpCompilation Compilation { get; }

        private readonly Action<Diagnostic> _addDiagnostic;

        // TODO: support reading existing resources
        private readonly Action<ResourceDescription> _addManifestResource;

        /// <summary>
        /// Adds a new <see cref="ResourceDescription"/> to the compilation.
        /// </summary>
        /// <param name="resource"></param>
        public void AddManifestResource( ResourceDescription resource ) => this._addManifestResource( resource );

        internal AspectWeaverContext(
            IAspectType aspectType,
            IReadOnlyList<IAspectInstance> aspectInstances,
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

        /// <summary>
        /// Reports a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        public void ReportDiagnostic( Diagnostic diagnostic ) => this._addDiagnostic( diagnostic );
    }
}