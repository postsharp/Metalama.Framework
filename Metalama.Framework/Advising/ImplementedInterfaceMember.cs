// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Advising
{
    /// <summary>
    /// Describes an interface member implemented by <see cref="IAdviceFactory.ImplementInterface(INamedType, INamedType, OverrideStrategy, object?)"/>.
    /// </summary>
    [CompileTime]
    public class ImplementedInterfaceMember
    {
        internal ImplementedInterfaceMember(
            IRef<IMember> interfaceMember,
            ImplementedInterfaceMemberAction action,
            IRef<IMember> targetMember )
        {
            this.InterfaceMember = interfaceMember;
            this.Action = action;
            this.TargetMember = targetMember;
        }

        /// <summary>
        /// Gets an interface member that was implemented.
        /// </summary>
        public IRef<IMember> InterfaceMember { get; }

        /// <summary>
        /// Gets a value indicating the action taken to implement the interface member.
        /// </summary>
        public ImplementedInterfaceMemberAction Action { get; }

        /// <summary>
        /// Gets the member used to implement the interface. This may be either an existing member or a newly introduced member.
        /// </summary>
        public IRef<IMember> TargetMember { get; }
    }
}