﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAttributeAsPredecessor : IIntrospectionAttributeAsPredecessor
{
    private readonly IntrospectionFactory _factory;
    private readonly ConcurrentLinkedList<AspectPredecessor> _successors = new();

    public IntrospectionAttributeAsPredecessor( IAttribute attribute, IntrospectionFactory factory )
    {
        this._factory = factory;
        this.Attribute = attribute;
    }

    public int PredecessorDegree => 0;

    public IDeclaration TargetDeclaration => this.Attribute.ContainingDeclaration;

    public ImmutableArray<IntrospectionAspectRelationship> Predecessors => ImmutableArray<IntrospectionAspectRelationship>.Empty;

    [Memo]
    public ImmutableArray<IntrospectionAspectRelationship> Successors
        => this._successors.Select( x => new IntrospectionAspectRelationship( AspectPredecessorKind.Attribute, this._factory.GetIntrospectionAspectInstance( (IAspectInstance)x.Instance ) )).ToImmutableArray();

    public void AddSuccessor( AspectPredecessor aspectInstance ) => this._successors.Add( aspectInstance );

    public IAttribute Attribute { get; }

    public override string ToString() => this.Attribute.ToString()!;
}