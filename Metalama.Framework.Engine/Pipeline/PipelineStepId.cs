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

        public PipelineStepPhase Phase { get; }

        public int AspectTargetTypeDepth { get; }

        public int AspectTargetDepth { get; }

        public int AdviceTargetDepth { get; }

        public PipelineStepId( AspectLayerId aspectLayerId, int aspectTargetTypeDepth, int aspectTargetDepth, PipelineStepPhase phase, int adviceTargetDepth )
        {
            this.AspectLayerId = aspectLayerId;
            this.Phase = phase;
            this.AspectTargetTypeDepth = aspectTargetTypeDepth;
            this.AspectTargetDepth = aspectTargetDepth;
            this.AdviceTargetDepth = adviceTargetDepth;
        }

        public bool Equals( PipelineStepId other )
            => this.AspectLayerId.Equals( other.AspectLayerId ) && this.AspectTargetTypeDepth == other.AspectTargetTypeDepth && this.Phase == other.Phase
               && this.AspectTargetDepth == other.AspectTargetDepth && this.AdviceTargetDepth == other.AdviceTargetDepth;

        public override bool Equals( object? obj ) => obj is PipelineStepId other && this.Equals( other );

        public override int GetHashCode()
            => HashCode.Combine( this.AspectLayerId, this.AspectTargetTypeDepth, this.AspectTargetDepth, this.Phase, this.AdviceTargetDepth );

        public override string ToString()
            => $"{this.AspectLayerId}:{this.AspectTargetTypeDepth}:{this.AspectTargetDepth}:{this.Phase}:{this.AdviceTargetDepth}";
    }
}