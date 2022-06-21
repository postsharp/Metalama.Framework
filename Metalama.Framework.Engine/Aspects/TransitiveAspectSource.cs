// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An aspect source that applies aspects that are inherited from referenced assemblies or projects.
/// </summary>
internal class TransitiveAspectSource : IAspectSource, IValidatorSource
{
    private readonly ImmutableDictionaryOfArray<IAspectClass, InheritableAspectInstance> _inheritedAspects;
    private readonly ImmutableArray<TransitiveValidatorInstance> _referenceValidators;

    public TransitiveAspectSource(
        Compilation compilation,
        ImmutableArray<IAspectClass> aspectClasses,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken )
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
                            manifest = TransitiveAspectsManifest.Deserialize( new MemoryStream( bytes ), serviceProvider );
                        }
                    }

                    break;

                case CompilationReference compilationReference:
                    manifest = inheritableAspectProvider?.GetTransitiveAspectsManifest( compilationReference.Compilation, cancellationToken );

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
                    // TODO: the next line may throw KeyNotFoundException.
                    var aspectClass = aspectClassesByName[aspectClassName];

                    var targets = manifest.GetInheritedAspects( aspectClassName )
                        .WhereNotNull();

                    inheritedAspectsBuilder.AddRange( aspectClass, targets );
                }

                // Process validators.
                validatorsBuilder.AddRange( manifest.Validators );
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
            var targetSymbol = inheritedAspectInstance.TargetDeclaration.GetSymbol( compilation.RoslynCompilation );

            if ( targetSymbol == null )
            {
                continue;
            }

            var baseDeclaration = compilation.Factory.GetDeclaration( targetSymbol );

            // We need to provide instances on the first level of derivation only because the caller will add to the next levels.

            foreach ( var derived in ((IDeclarationImpl) baseDeclaration).GetDerivedDeclarations( false ) )
            {
                aspectInstances.Add(
                    new AspectInstance(
                        inheritedAspectInstance.Aspect,
                        derived.ToTypedRef(),
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
        IDiagnosticSink diagnosticAdder )
    {
        if ( kind == ValidatorKind.Reference )
        {
            return this._referenceValidators.Select(
                v => new ReferenceValidatorInstance(
                    v.ValidatedDeclaration.GetTarget( compilation ),
                    GetReferenceValidatorDriver( v.Object.GetType(), v.MethodName ),
                    ValidatorImplementation.Create( v.Object, v.State ),
                    v.ReferenceKinds ) );
        }
        else
        {
            return Enumerable.Empty<ValidatorInstance>();
        }
    }

    private static ReferenceValidatorDriver GetReferenceValidatorDriver( Type type, string methodName )
    {
        var method = type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        if ( method == null )
        {
            throw new ArgumentOutOfRangeException( nameof(methodName), $"Cannot find a method named '{methodName}' in '{type}'." );
        }

        return ValidatorDriverFactory.GetInstance( type )
            .GetReferenceValidatorDriver( method );
    }
}