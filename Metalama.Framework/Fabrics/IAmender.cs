// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Fabrics
{
    /// <summary>
    /// Base interface for the argument of <see cref="ProjectFabric.AmendProject"/>, <see cref="NamespaceFabric.AmendNamespace"/>
    /// or <see cref="TypeFabric.AmendType"/>. Allows to report diagnostics and add aspects to the target declaration of the fabric.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAmender<T> : IDeclarationSelector<T>
        where T : class, IDeclaration
    {
        /// <summary>
        /// Gets the project being built.
        /// </summary>
        IProject Project { get; }
    }
}