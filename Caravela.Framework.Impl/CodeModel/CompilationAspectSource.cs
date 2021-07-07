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

        public CompilationAspectSource( IReadOnlyList<AspectClass> aspectTypes, CompileTimeProjectLoader loader )
        {
            this._loader = loader;
            this.AspectTypes = aspectTypes;
        }

        public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

        public IEnumerable<AspectClass> AspectTypes { get; }

        // TODO: implement aspect exclusion based on ExcludeAspectAttribute
        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            AspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            if ( !compilation.Factory.TryGetTypeByReflectionName( aspectClass.FullName, out var aspectType ) )
            {
                // This happens at design time when the IDE sends an incomplete compilation. We cannot apply the aspects in this case,
                // but we prefer not to throw an exception since the case is expected.
                return Enumerable.Empty<AspectInstance>();
            }

            return compilation.GetAllAttributesOfType( aspectType )
                .Select(
                    attribute =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if ( this._loader.AttributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out var attributeInstance ) )
                        {
                            return aspectClass.CreateAspectInstance( (IAspect) attributeInstance, attribute.ContainingDeclaration.AssertNotNull() );
                        }
                        else
                        {
                            return null;
                        }
                    } )
                .WhereNotNull();
        }
    }
}