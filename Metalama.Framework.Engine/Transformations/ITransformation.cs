// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any transformation.
    /// </summary>
    internal interface ITransformation
    {
        SyntaxTree TransformedSyntaxTree { get; }
        
        IDeclaration TargetDeclaration { get; }

        Advice ParentAdvice { get; }

        int OrderWithinAspectInstance { get; set; }
    }

    internal abstract class BaseTransformation : ITransformation
    {
        protected BaseTransformation( Advice advice )
        {
            this.ParentAdvice = advice;
        }

        /// <summary>
        /// Gets the declaration that is transformed, or the declaration into which a new declaration is being introduced. 
        /// </summary>
        public abstract IDeclaration TargetDeclaration { get; }

        public SyntaxTree TransformedSyntaxTree => this.TargetDeclaration.GetPrimarySyntaxTree().AssertNotNull();

        public Advice ParentAdvice { get; }

        public int OrderWithinAspectInstance { get; set; }
    }
}