// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents the relationship that an object (attribute, fabric, aspect) has created or required another aspect.
    /// These relationships are exposed on <see cref="IAspectInstance.Predecessors"/>.
    /// </summary>
    [CompileTimeOnly]
    public readonly struct AspectPredecessor
    {
        public AspectPredecessorKind Kind { get; }

        /// <summary>
        /// Gets the object that created the aspect instance. It can be an <see cref="IAspectInstance"/>, an <see cref="IFabric"/>, or an <see cref="IAttribute"/>.
        /// </summary>
        public IAspectPredecessor Instance { get; }

        internal AspectPredecessor( AspectPredecessorKind kind, IAspectPredecessor instance )
        {
            this.Kind = kind;
            this.Instance = instance;
        }
    }
}