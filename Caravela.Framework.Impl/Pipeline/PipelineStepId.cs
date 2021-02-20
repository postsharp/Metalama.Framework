using System;

namespace Caravela.Framework.Impl.Pipeline
{
    internal readonly struct PipelineStepId : IEquatable<PipelineStepId>
    {
        public AspectLayerId AspectLayerId { get; }

        public int Depth { get; }

        public PipelineStepId( AspectLayerId aspectLayerId, int depth )
        {
            this.AspectLayerId = aspectLayerId;
            this.Depth = depth;
        }

        public bool Equals(PipelineStepId other) => this.AspectLayerId.Equals(other.AspectLayerId) && this.Depth == other.Depth;

        public override bool Equals(object? obj) => obj is PipelineStepId other && this.Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.AspectLayerId.GetHashCode() * 397) ^ this.Depth;
            }
        }

    }
}