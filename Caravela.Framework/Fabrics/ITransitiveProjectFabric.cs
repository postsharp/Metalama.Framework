// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// An interface that, when implemented by a type in an assembly (under any name or namespace), allows that type to analyze and
    /// add aspects to any project that references this assembly. However, the <see cref="IProjectFabric.AmendProject"/> method
    /// is not executed in the current project (you will need another class that does not implement <see cref="ITransitiveProjectFabric"/>
    /// to amend the current project). 
    /// </summary>
    /// <remarks>
    /// When the project contains several transitive project fabrics, the ones that are the deepest in the dependency graph are
    /// executed first. Then, transitive fabrics are ordered by assembly name, then by distance to the root directory in the file system,
    /// then by type name.
    /// </remarks>
    public interface ITransitiveProjectFabric : IProjectFabric { }
}