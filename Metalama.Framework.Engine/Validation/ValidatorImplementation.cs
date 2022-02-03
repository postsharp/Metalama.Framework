// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Engine.Validation;

public readonly struct ValidatorImplementation
{
    public object Implementation { get; }

    public IAspectState? State { get; }

    public static ValidatorImplementation Create( IAspectPredecessor predecessor )
        => predecessor switch
        {
            IAspectInstance aspectInstance => new ValidatorImplementation( aspectInstance ),
            IFabricInstance fabricInstance => new ValidatorImplementation( fabricInstance.Fabric ),
            _ => throw new AssertionFailedException()
        };

    public static ValidatorImplementation Create( object implementation, IAspectState? aspectState ) => new( implementation, aspectState );

    private ValidatorImplementation( IAspectInstance aspectInstance )
    {
        this.Implementation = aspectInstance.Aspect;
        this.State = aspectInstance.State;
    }

    private ValidatorImplementation( Fabric fabric )
    {
        this.Implementation = fabric;
        this.State = null;
    }

    private ValidatorImplementation( object implementation, IAspectState? aspectState )
    {
        this.Implementation = implementation;
        this.State = aspectState;
    }

    public void UpdateHash( XXH64 hasher )
    {
        HashUtilities.Update( hasher, this.Implementation.GetHashCode() );

        if ( this.State != null )
        {
            HashUtilities.Update( hasher, this.State.GetHashCode() );
        }
    }
}