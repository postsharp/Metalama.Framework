// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// An aspect source that applies aspects that are inherited from referenced assemblies or projects.
    /// </summary>
    internal class ExternalInheritedAspectSource : IAspectSource
    {
        private readonly ImmutableDictionaryOfArray<IAspectClass, InheritableAspectInstance> _inheritedAspects;

        public ExternalInheritedAspectSource(
            Compilation compilation,
            ImmutableArray<IAspectClass> aspectClasses,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken )
        {
            var inheritableAspectProvider = serviceProvider.GetService<IInheritableAspectManifestProvider>();
            
            var inheritedAspectsBuilder = ImmutableDictionaryOfArray<IAspectClass, InheritableAspectInstance>.CreateBuilder();
            var aspectClassesByName = aspectClasses.ToDictionary( t => t.FullName, t => t );

            foreach ( var reference in compilation.References )
            {
                ITransitiveAspectsManifest? manifest = null;

                switch ( reference )
                {
                    case PortableExecutableReference { FilePath: { } filePath }:
                        if ( ManagedResourceReader.TryGetCompileTimeResource( filePath, out var resources ) )
                        {
                            if ( resources.TryGetValue( CompileTimeConstants.InheritableAspectManifestResourceName, out var bytes ) )
                            {
                                manifest = TransitiveAspectsManifest.Deserialize( new MemoryStream( bytes ), serviceProvider );
                            }
                        }

                        break;

                    case CompilationReference compilationReference:
                        manifest = inheritableAspectProvider?.GetInheritableAspectsManifest( compilationReference.Compilation, cancellationToken );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
                }

                if ( manifest != null )
                {
                    foreach ( var aspectClassName in manifest.InheritableAspectTypes )
                    {
                        var aspectClass = aspectClassesByName[aspectClassName];

                        var targets = manifest.GetInheritedAspects( aspectClassName )
                            .WhereNotNull();

                        inheritedAspectsBuilder.AddRange( aspectClass, targets );
                    }
                }
            }

            this._inheritedAspects = inheritedAspectsBuilder.ToImmutable();
        }

        public ImmutableArray<IAspectClass> AspectClasses => this._inheritedAspects.Keys.ToImmutableArray();

        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            foreach ( var inheritedAspectInstance in this._inheritedAspects[aspectClass] )
            {
                var targetSymbol = inheritedAspectInstance.TargetDeclaration.GetSymbol( compilation.RoslynCompilation );

                if ( targetSymbol == null )
                {
                    continue;
                }

                var baseDeclaration = compilation.Factory.GetDeclaration( targetSymbol );

                // We need to provide instances on the first level of derivation only because the caller will add to the next levels.

                foreach ( var derived in ((IDeclarationImpl) baseDeclaration).GetDerivedDeclarations( false ) )
                {
                    yield return new AspectInstance(
                        inheritedAspectInstance.Aspect,
                        derived.ToTypedRef(),
                        (AspectClass) aspectClass,
                        new AspectPredecessor( AspectPredecessorKind.Inherited, inheritedAspectInstance ) );
                }
            }
        }
    }
}