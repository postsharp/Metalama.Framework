// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// An interface that must be implemented by classes that want to extend <see cref="IProject"/> with project-local data using the <see cref="IProject.Data{T}"/> method.
    /// </summary>
    [CompileTimeOnly]
    public interface IProjectData
    {
        /// <summary>
        /// Initializes the object from project properties.
        /// </summary>
        /// <param name="project"></param>
        void Initialize( IProject project )
#if NET5_0
        { }
#else
            ;
#endif
    }
}