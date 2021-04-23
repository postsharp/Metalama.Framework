// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationAspectSource : IAspectSource
    {
        private readonly CompilationModel _compilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public CompilationAspectSource( CompilationModel compilation, IReadOnlyList<INamedType> aspectTypes, CompileTimeAssemblyLoader loader )
        {
            this._compilation = compilation;
            this._loader = loader;
            this.AspectTypes = aspectTypes;
        }

        public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

        public IEnumerable<INamedType> AspectTypes { get; }

        // TODO: implement aspect exclusion based on ExcludeAspectAttribute
        public IEnumerable<ICodeElement> GetExclusions( INamedType aspectType ) => Enumerable.Empty<ICodeElement>();

        public IEnumerable<AspectInstance> GetAspectInstances( AspectType aspectType, IDiagnosticAdder diagnosticAdder )
            => this._compilation.GetAllAttributesOfType( aspectType.Type )
                .Select(
                    attribute =>
                    {
                        if ( this._loader.TryCreateAttributeInstance( attribute, diagnosticAdder, out var attributeInstance ) )
                        {
                            return aspectType.CreateAspectInstance( (IAspect) attributeInstance, attribute.ContainingElement.AssertNotNull() );
                        }
                        else
                        {
                            return null;
                        }
                    } )
                .WhereNotNull();
    }
}