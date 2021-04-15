// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represent a transformation that is not observable by the aspects running after the aspect
    /// that provided the transformation..
    /// </summary>
    internal interface INonObservableTransformation { }
}