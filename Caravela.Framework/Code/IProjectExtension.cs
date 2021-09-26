// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Base interface for project configuration objects, which are a way for compile-time libraries
    /// to expose configuration objects that can be configured in a <see cref="IProjectFabric"/>. 
    /// Implementations must implement the Freezable pattern. (Not implemented.)
    /// </summary>
    [CompileTimeOnly]
    public interface IProjectExtension
    {
        /// <summary>
        /// Initializes the object from project properties.
        /// </summary>
        /// <param name="project"></param>
        void Initialize( IProject project );

        /// <summary>
        /// Prevents further modifications of the current object and all its children objects. This method is called after
        /// all project policies have been built.
        /// </summary>
        void Freeze();
    }
}