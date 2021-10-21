// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// An base class that must be implemented by classes that want to extend <see cref="IProject"/> with project-local data using the <see cref="IProject.Data{T}"/> method.
    /// </summary>
    /// <remarks>
    /// The implementation must not allow modifications of the state after the object has been made read only.
    /// </remarks>
    [CompileTimeOnly]
    public abstract class ProjectData
    {
        /// <summary>
        /// Initializes the object from project properties.
        /// </summary>
        /// <param name="project">The project to which the new <see cref="ProjectData"/> belongs.</param>
        /// <param name="isReadOnly">A value indicating whether the project data is already read-only. If <c>false</c>, the project data
        /// can still be modified by project fabrics, after which the <see cref="MakeReadOnly"/> method will be called.</param>
        public virtual void Initialize( IProject project, bool isReadOnly ) { }

        /// <summary>
        /// Signals that further modifications of the object must be prevented.
        /// </summary>
        public virtual void MakeReadOnly() { }
    }
}