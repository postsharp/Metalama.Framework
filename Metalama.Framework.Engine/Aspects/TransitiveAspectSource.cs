// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An aspect source that applies aspects that are inherited from referenced assemblies or projects.
/// </summary>
internal sealed class TransitiveAspectSource : IAspectSource, IValidatorSource
{
    private readonly ImmutableDictionaryOfArray<IAspectClass, InheritableAspectInstance> _inheritedAspects;
    private readonly ImmutableArray<TransitiveValidatorInstance> _referenceValidators;

    public TransitiveAspectSource(
        Compilation compilation,
        ImmutableArray<IAspectClass> aspectClasses,
        ProjectServiceProvider serviceProvider )
    {
        var inheritableAspectProvider = serviceProvider.GetService<ITransitiveAspectManifestProvider>();

        var inheritedAspectsBuilder = ImmutableDictionaryOfArray<IAspectClass, InheritableAspectInstance>.CreateBuilder();
        var validatorsBuilder = ImmutableArray.CreateBuilder<TransitiveValidatorInstance>();

        var aspectClassesByName = aspectClasses.ToDictionary( t => t.FullName, t => t );

        foreach ( var reference in compilation.References )
        {
            // Get the manifest of the reference.
            ITransitiveAspectsManifest? manifest = null;

            switch ( reference )
            {
                case PortableExecutableReference { FilePath: { } filePath }:
                    if ( MetadataReader.TryGetMetadata( filePath, out var metadataInfo ) )
                    {
                        if ( metadataInfo.Resources.TryGetValue( CompileTimeConstants.InheritableAspectManifestResourceName, out var bytes ) )
                        {
                            manifest = TransitiveAspectsManifest.Deserialize( new MemoryStream( bytes ), serviceProvider, compilation );
                        }
                    }

                    break;

                case CompilationReference compilationReference:
                    manifest = inheritableAspectProvider?.GetTransitiveAspectsManifest( compilationReference.Compilation );

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
            }

            // Process the manifest.
            if ( manifest != null )
            {
                // Process inherited aspects.
                foreach ( var aspectClassName in manifest.InheritableAspectTypes )
                {
                    if ( !aspectClassesByName.TryGetValue( aspectClassName, out var aspectClass ) )
                    {
                        // It seems to happen with inherited aspects at design time when the aspect class could not be found.
                        // In that case, an error should have been reported above. Anyway, this should not be the problem of the present
                        // method but of the code upstream and we should cope with that situation/
                        serviceProvider.GetLoggerFactory()
                            .GetLogger( nameof(TransitiveAspectSource) )
                            .Warning?.Log( $"Cannot find the aspect class '{aspectClassesByName}'." );

                        continue;
                    }

                    var targets = manifest.GetInheritableAspects( aspectClassName )
                        .WhereNotNull();

                    inheritedAspectsBuilder.AddRange( aspectClass, targets );
                }

                // Process validators.
                validatorsBuilder.AddRange( manifest.ReferenceValidators );
            }
        }

        this._inheritedAspects = inheritedAspectsBuilder.ToImmutable();
        this._referenceValidators = validatorsBuilder.ToImmutable();
    }

    public ImmutableArray<IAspectClass> AspectClasses => this._inheritedAspects.Keys.ToImmutableArray();

    public AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
    {
        List<AspectInstance> aspectInstances = new();

        foreach ( var inheritedAspectInstance in this._inheritedAspects[aspectClass] )
        {
            var baseDeclaration = inheritedAspectInstance.TargetDeclaration.GetTargetOrNull( compilation );

            if ( baseDeclaration == null )
            {
                continue;
            }

            // We need to provide instances on the first level of derivation only because the caller will add to the next levels.

            foreach ( var derived in ((IDeclarationImpl) baseDeclaration).GetDerivedDeclarations( DerivedTypesOptions.DirectOnly ) )
            {
                aspectInstances.Add(
                    new AspectInstance(
                        inheritedAspectInstance.Aspect,
                        derived,
                        (AspectClass) aspectClass,
                        new AspectPredecessor( AspectPredecessorKind.Inherited, inheritedAspectInstance ) ) );
            }
        }

        return new AspectSourceResult( aspectInstances );
    }

    IEnumerable<ValidatorInstance> IValidatorSource.GetValidators(
        ValidatorKind kind,
        CompilationModelVersion version,
        CompilationModel compilation,
        UserDiagnosticSink diagnosticAdder )
    {
        if ( kind == ValidatorKind.Reference )
        {
            return this._referenceValidators.Select(
                    v =>
                    {
                        var validationTarget = v.ValidatedDeclaration.GetTargetOrNull( compilation );

                        if ( validationTarget?.GetSymbol() == null )
                        {
                            return null;
                        }

                        return new ReferenceValidatorInstance(
                            validationTarget,
                            v.GetReferenceValidatorDriver(),
                            ValidatorImplementation.Create( v.Object, v.State ),
                            v.ReferenceKinds,
                            v.IncludeDerivedTypes,
                            v.DiagnosticSourceDescription );
                    } )
                .WhereNotNull();
        }
        else
        {
            return Enumerable.Empty<ValidatorInstance>();
        }
    }
}