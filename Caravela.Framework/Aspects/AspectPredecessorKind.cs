// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Kinds of <see cref="AspectPredecessor"/>.
    /// </summary>
    [CompileTimeOnly]
    public enum AspectPredecessorKind
    {
        // First (lower) priorities are evaluated first. Order matters.

        /// <summary>
        /// The aspect has been created by a custom attribute. <see cref="AspectPredecessor.Instance"/> is an <see cref="IAttribute"/>.
        /// </summary>
        Attribute,

        /// <summary>
        /// The aspect has been created by another aspect. <see cref="AspectPredecessor.Instance"/> is an <see cref="IAspect"/>.
        /// </summary>
        ChildAspect,

        /// <summary>
        /// Provided implicitly by <see cref="IAspectDependencyBuilder.RequiresAspect{TAspect}"/>.
        /// </summary>
        [Obsolete( "Not implemented." )]
        RequiredAspect,

        /// <summary>
        /// Aspects added because of aspect inheritance.
        /// </summary>
        Inherited,

        /// <summary>
        /// The aspect has been created by a fabric. <see cref="AspectPredecessor.Instance"/> is an <see cref="Fabrics.Fabric"/>.
        /// </summary>
        Fabric
    }
}