// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any transformation.
    /// </summary>
    internal interface ITransformation
    {
        Advice ParentAdvice { get; }

        int OrderWithinAspectInstance { get; set; }
    }

    internal abstract class BaseTransformation : ITransformation
    {
        protected BaseTransformation( Advice advice )
        {
            this.ParentAdvice = advice;
        }

        public Advice ParentAdvice { get; }

        public int OrderWithinAspectInstance { get; set; }
    }
}