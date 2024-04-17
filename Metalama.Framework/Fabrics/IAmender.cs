// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Fabrics
{
    [InternalImplement]
    [CompileTime]
    public interface IAmender
    {
        /// <summary>
        /// Gets the project being built.
        /// </summary>
        IProject Project { get; }
    }

    /// <summary>
    /// Base interface for the argument of <see cref="ProjectFabric.AmendProject"/>, <see cref="NamespaceFabric.AmendNamespace"/>
    /// or <see cref="TypeFabric.AmendType"/>. Allows to report diagnostics and add aspects to the target declaration of the fabric.
    /// </summary>
    public interface IAmender<out T> : IAmender
        where T : class, IDeclaration
    {
        /// <summary>
        /// Gets an object that allows to add child advice and to validate code and code references.
        /// </summary>
        IAspectReceiver<T> Outbound { get; }
    }
}