// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationAspectSource : IAspectSource
    {
        private readonly CompileTimeProjectLoader _loader;

        public CompilationAspectSource( IReadOnlyList<AspectClassMetadata> aspectTypes, CompileTimeProjectLoader loader )
        {
            this._loader = loader;
            this.AspectTypes = aspectTypes;
        }

        public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

        public IEnumerable<AspectClassMetadata> AspectTypes { get; }

        // TODO: implement aspect exclusion based on ExcludeAspectAttribute
        public IEnumerable<ICodeElement> GetExclusions( INamedType aspectType ) => Enumerable.Empty<ICodeElement>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            AspectClassMetadata aspectClassMetadata,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
            => compilation.GetAllAttributesOfType( compilation.Factory.GetTypeByReflectionName( aspectClassMetadata.FullName ) )
                .Select(
                    attribute =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if ( this._loader.AttributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out var attributeInstance ) )
                        {
                            return aspectClassMetadata.CreateAspectInstance( (IAspect) attributeInstance, attribute.ContainingElement.AssertNotNull() );
                        }
                        else
                        {
                            return null;
                        }
                    } )
                .WhereNotNull();
    }
}