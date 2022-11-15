// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionFabric : IIntrospectionFabric
{
    private readonly FabricInstance _fabric;
    private readonly ICompilation _compilation;
    private readonly IntrospectionFactory _factory;
    private readonly ConcurrentLinkedList<IAspectInstance> _successors = new();

    public IntrospectionFabric( FabricInstance fabric, ICompilation compilation, IntrospectionFactory factory )
    {
        this._fabric = fabric;
        this._compilation = compilation;
        this._factory = factory;
    }

    public int PredecessorDegree => 0;

    public IDeclaration TargetDeclaration => this._fabric.TargetDeclaration.GetTarget( this._compilation );

    public string FullName => this._fabric.Fabric.GetType().FullName!;

    public ImmutableArray<IntrospectionAspectPredecessor> Predecessors => ImmutableArray<IntrospectionAspectPredecessor>.Empty;

    [Memo]
    public ImmutableArray<IIntrospectionAspectInstance> Successors
        => this._successors.Select( x => this._factory.GetIntrospectionAspectInstance( x ) ).ToImmutableArray<IIntrospectionAspectInstance>();

    public void AddSuccessor( IAspectInstance aspectInstance ) => this._successors.Add( aspectInstance );
}