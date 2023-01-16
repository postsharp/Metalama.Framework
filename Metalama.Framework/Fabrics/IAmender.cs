// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    [CompileTime]
    public interface IAmender<out T> : IAspectReceiverSelector<T>
        where T : class, IDeclaration
    {
        /// <summary>
        /// Gets the project being built.
        /// </summary>
        IProject Project { get; }
    }
}