// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Impl.Transformations
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