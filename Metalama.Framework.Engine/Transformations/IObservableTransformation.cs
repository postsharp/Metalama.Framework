// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents an introduction to the code model that should be observable by aspects running after the aspect that added the introduction. 
    /// </summary>
    internal interface IObservableTransformation : ITransformation
    {
        IDeclaration ContainingDeclaration { get; }

        /// <summary>
        /// Gets a value indicating whether the transformation should be included in the design-time generated code.
        /// </summary>
        bool IsDesignTime { get; }
    }
}