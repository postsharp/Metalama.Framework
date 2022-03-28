// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    internal interface IHierarchicalTransformation
    {
        /// <summary>
        /// Gets a list of dependencies of this transformation. Dependencies are always initialized before dependent transformations.
        /// </summary>
        IReadOnlyList<IHierarchicalTransformation> Dependencies { get; }

        /// <summary>
        /// Initializes the transformation. Transformation may return the initialization data which is then received by other transformation methods.
        /// </summary>
        /// <param name="context"></param>
        TransformationInitializationResult? Initialize( in InitializationContext context );
    }
}