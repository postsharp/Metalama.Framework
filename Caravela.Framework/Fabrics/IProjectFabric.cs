// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// An interface that, when implemented by a type in a project (under any name or namespace), allows that type to analyze and
    /// add aspects to that project.
    /// </summary>
    public interface IProjectFabric : IFabric
    {
        /// <summary>
        /// The user can implement this method to analyze types in the current project, add aspects, and report or suppress diagnostics.
        /// </summary>
        void AmendProject( IProjectAmender builder );
    }
}