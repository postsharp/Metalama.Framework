// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using System;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The identifier of a <see cref="PipelineStepId"/>. For inequality comparison, see <see cref="PipelineStepIdComparer"/>.
    /// </summary>
    internal readonly struct PipelineStepId : IEquatable<PipelineStepId>
    {
        public AspectLayerId AspectLayerId { get; }

        public int Depth { get; }

        public PipelineStepId( AspectLayerId aspectLayerId, int depth )
        {
            this.AspectLayerId = aspectLayerId;
            this.Depth = depth;
        }

        public bool Equals( PipelineStepId other ) => this.AspectLayerId.Equals( other.AspectLayerId ) && this.Depth == other.Depth;

        public override bool Equals( object? obj ) => obj is PipelineStepId other && this.Equals( other );

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.AspectLayerId.GetHashCode() * 397) ^ this.Depth;
            }
        }

        public override string ToString() => this.AspectLayerId + ":" + this.Depth;
    }
}