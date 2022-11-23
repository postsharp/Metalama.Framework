// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Introspection;
using Metalama.Framework.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionFactory : IProjectService
{
    private readonly ConcurrentDictionary<IAspectInstance, IntrospectionAspectInstance> _aspectInstances = new();
    private readonly ConcurrentDictionary<IAspectClass, IntrospectionAspectClass> _aspectClasses = new();
    private readonly CompilationModel _compilation;

    public IntrospectionFactory( CompilationModel compilation )
    {
        this._compilation = compilation;
    }

    public IntrospectionAspectInstance GetIntrospectionAspectInstance( IAspectInstance aspectInstance )
        => this._aspectInstances.GetOrAdd( aspectInstance, x => new IntrospectionAspectInstance( x, this._compilation, this ) );

    public IIntrospectionAspectPredecessorInternal GetIntrospectionAspectPredecessor( IAspectPredecessor aspectPredecessor )
        => aspectPredecessor switch
        {
            IAspectInstance aspectInstance => this.GetIntrospectionAspectInstance( aspectInstance ),
            IAttribute attribute => new IntrospectionAttributeAsPredecessor( attribute, this ),
            FabricInstance fabricInstance => new IntrospectionFabric( fabricInstance, this._compilation, this ),
            _ => throw new ArgumentOutOfRangeException( nameof(aspectPredecessor) )
        };

    public IIntrospectionAspectClass GetIntrospectionAspectClass( IAspectClass aspectClass )
    {
        if ( !this._aspectClasses.TryGetValue( aspectClass, out var introspectionAspectClass ) )
        {
            throw new AssertionFailedException( $"The aspect class '{aspectClass}' was not added." );
        }

        return introspectionAspectClass;
    }

    public IIntrospectionAspectClass CreateIntrospectionAspectClass( IAspectClass aspectClass, ImmutableArray<AspectInstanceResult> results )
    {
        var introspectionAspectClass = new IntrospectionAspectClass( aspectClass, results, this );

        if ( !this._aspectClasses.TryAdd( aspectClass, introspectionAspectClass ) )
        {
            throw new AssertionFailedException( $"The aspect class '{aspectClass}' was already added." );
        }
        else
        {
            return introspectionAspectClass;
        }
    }
}