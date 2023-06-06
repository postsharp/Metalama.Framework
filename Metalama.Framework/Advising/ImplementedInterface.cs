// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Advising
{
    /// <summary>
    /// Describes an interface type implemented by <see cref="IAdviceFactory.ImplementInterface(INamedType, INamedType, OverrideStrategy, object?)"/>.
    /// </summary>
    [CompileTime]
    public class ImplementedInterface
    {
        internal ImplementedInterface(
            IRef<INamedType> @interface,
            ImplementedInterfaceAction action )
        {
            this.Interface = @interface;
            this.Action = action;
        }

        /// <summary>
        /// Gets an interface type that was considered by the advice.
        /// </summary>
        public IRef<INamedType> Interface { get; }

        /// <summary>
        /// Gets a value indicating the action taken to implement the interface type.
        /// </summary>
        public ImplementedInterfaceAction Action { get; }
    }
}