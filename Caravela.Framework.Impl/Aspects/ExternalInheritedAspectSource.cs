// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// An aspect source that applies aspects that are inherited from referenced assemblies or projects.
    /// </summary>
    internal class ExternalInheritedAspectSource : IAspectSource
    {
        private readonly ImmutableMultiValueDictionary<IAspectClass, ISymbol> _inheritedAspects;
        private readonly CompileTimeProjectLoader _loader;

        public ExternalInheritedAspectSource( Compilation compilation, ImmutableArray<IAspectClass> aspectTypes, CompileTimeProjectLoader loader )
        {
            this._loader = loader;
            var inheritedAspectsBuilder = ImmutableMultiValueDictionary<IAspectClass, ISymbol>.CreateBuilder();
            var aspectTypesByName = aspectTypes.ToDictionary( t => t.FullName, t => t );

            foreach ( var reference in compilation.References )
            {
                InheritableAspectsManifest? manifest = null;

                switch ( reference )
                {
                    case PortableExecutableReference { FilePath: { } filePath }:
                        if ( ManagedResourceReader.TryGetCompileTimeResource( filePath, out var resources ) )
                        {
                            if ( resources.TryGetValue( CompileTimeConstants.InheritableAspectManifestResourceName, out var bytes ) )
                            {
                                manifest = InheritableAspectsManifest.Deserialize( new MemoryStream( bytes ) );
                            }
                        }

                        break;

                    case CompilationReference:
                        // This is the design-time scenario.
                        throw new NotImplementedException();

                    default:
                        throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
                }

                if ( manifest != null )
                {
                    inheritedAspectsBuilder.AddRange(
                        manifest.InheritableAspects,
                        x => aspectTypesByName[x.Key],
                        x => x.Value.Select( id => DocumentationCommentId.GetFirstSymbolForReferenceId( id, compilation ).AssertNotNull() ) );
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
            foreach ( var baseSymbol in this._inheritedAspects[aspectClass] )
            {
                var baseDeclaration = compilation.Factory.GetDeclaration( baseSymbol );
                var attributeData = baseSymbol.GetAttributes().Single( a => a.AttributeClass?.GetReflectionName() == aspectClass.FullName );
                var attribute = baseDeclaration.Attributes.Single( a => a.Type.FullName == aspectClass.FullName );

                foreach ( var derived in ((IDeclarationImpl) baseDeclaration).GetDerivedDeclarations() )
                {
                    if ( !this._loader.AttributeDeserializer.TryCreateAttribute( attributeData, diagnosticAdder, out var attributeInstance ) )
                    {
                        throw new AssertionFailedException();
                    }

                    yield return new AttributeAspectInstance( (IAspect) attributeInstance, derived, (AspectClass) aspectClass, attribute, this._loader );
                }
            }
        }
    }
}