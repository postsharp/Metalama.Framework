// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

internal sealed class IncrementalAspectRepository : AspectRepository
{
    private readonly ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstance> _aspects;

    // TODO: throw exception if the aspect of type T is not processed yet.
    // TODO: refactor the aspect lookup service so that the implementation can be injected from the CodeRefactoringService, so
    // that we don't have to run the pipeline to have a model including aspects.
    public override AspectRepository WithAspectInstances( IEnumerable<IAspectInstance> aspectInstances )
    {
        var newDictionary = this._aspects.AddRange( aspectInstances, instance => ((IAspectInstanceInternal) instance).TargetDeclaration );

        if ( newDictionary == this._aspects )
        {
            return this;
        }
        else
        {
            return new IncrementalAspectRepository( newDictionary );
        }
    }

    public override IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration ) => this._aspects[declaration.ToTypedRef()].Select( a => a.Aspect ).OfType<T>();

    // TODO: return null if the aspect of type T is not processed yet.
    public override bool HasAspect( IDeclaration declaration, Type aspectType )
        => this._aspects[declaration.ToTypedRef()].Any( a => a.AspectClass.Type == aspectType );

    private IncrementalAspectRepository( ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstance> aspects )
    {
        this._aspects = aspects;
    }

    public IncrementalAspectRepository() : this( ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstance>.Empty ) { }
}