// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Introspection;

/// <summary>
/// Represents the relationship that an object (attribute, fabric, aspect) has created or required another aspect or validator.
/// These relationships are exposed on <see cref="IAspectPredecessor.Predecessors"/>.
/// </summary>
public sealed class IntrospectionAspectPredecessor
{
    /// <summary>
    /// Gets the kind of relationship represented by the current <see cref="AspectPredecessor"/>, and the kind of object
    /// present in the <see cref="Instance"/> property. 
    /// </summary>
    public AspectPredecessorKind Kind { get; }

    /// <summary>
    /// Gets the object that created the aspect instance. It can be an <see cref="IIntrospectionAspectInstance"/>, an <see cref="IIntrospectionFabric"/>, or an <see cref="IIntrospectionAttributeAsPredecessor"/>.
    /// </summary>
    public IIntrospectionAspectPredecessor Instance { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntrospectionAspectPredecessor"/> class.
    /// </summary>
    public IntrospectionAspectPredecessor( AspectPredecessorKind kind, IIntrospectionAspectPredecessor instance )
    {
        this.Kind = kind;
        this.Instance = instance;
    }

    public override string ToString() => $"Kind={this.Kind}, Instance={{{this.Instance}}}";
}