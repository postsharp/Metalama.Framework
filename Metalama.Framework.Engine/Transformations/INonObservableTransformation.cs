// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represent a transformation that is not observable by the aspects running after the aspect
    /// that provided the transformation..
    /// </summary>
    internal interface INonObservableTransformation : ITransformation { }
}