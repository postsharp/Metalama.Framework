// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Linq.Expressions;

namespace Metalama.Framework.Aspects
{
    // ReSharper disable once ClassNeverInstantiated.Global

    /// <summary>
    /// Not implemented.
    /// </summary>
    internal sealed class InterfaceMemberSpecification
    {
#pragma warning disable IDE0051 // Remove unused private members

        // ReSharper disable UnusedParameter.Local

        private InterfaceMemberSpecification(
            IMember interfaceMember,
            IMember implementationMember,
            InterfaceMemberOverrideStrategy overrideStrategy,
            object? tags )
#pragma warning restore IDE0051 // Remove unused private members
        {
            throw new NotImplementedException();
        }

        // ReSharper enable UnusedParameter.Local

        // ReSharper disable once UnassignedGetOnlyAutoProperty

        /// <summary>
        /// Gets the member of the interface to be implemented.
        /// </summary>
        public IMember InterfaceMember { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty

        /// <summary>
        /// Gets the implementation member, i.e. either a callable member of the target type or member of the aspect marked with [Introduce] or [InterfaceMember].
        /// </summary>
        public IMember ImplementationMember { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty

        /// <summary>
        /// Gets a value indication the override strategy when interface member conflicts with an existing class member.
        /// </summary>
        public InterfaceMemberOverrideStrategy OverrideStrategy { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty

        /// <summary>
        /// Gets an optional opaque object of anonymous type passed to the template method and exposed under the <see cref="meta.Tags"/> property
        /// of the <see cref="meta"/> API. Replaces values provided for the whole advice.
        /// </summary>
        public object? Tags { get; }

        [Obsolete( "Not implemented.", true )]
        public static InterfaceMemberSpecification Create<TInterfaceType, TReturnType>(
            Expression<Func<TInterfaceType, TReturnType>> expression,
            string aspectMemberName,
            InterfaceMemberOverrideStrategy overrideStrategy = InterfaceMemberOverrideStrategy.Default,
            object? tags = null )
            => throw new NotImplementedException();

        [Obsolete( "Not implemented.", true )]
        public static InterfaceMemberSpecification Create<TInterfaceType, TReturnType>(
            Expression<Func<TInterfaceType, TReturnType>> expression,
            IMember targetTypeMember,
            InterfaceMemberOverrideStrategy overrideStrategy = InterfaceMemberOverrideStrategy.Default,
            object? tags = null )
            => throw new NotImplementedException();

        [Obsolete( "Not implemented.", true )]
        public static InterfaceMemberSpecification Create(
            IMember interfaceMember,
            string aspectMemberName,
            InterfaceMemberOverrideStrategy overrideStrategy = InterfaceMemberOverrideStrategy.Default,
            object? tags = null )
            => throw new NotImplementedException();

        [Obsolete( "Not implemented.", true )]
        public static InterfaceMemberSpecification Create(
            IMember interfaceMember,
            IMember targetTypeMember,
            InterfaceMemberOverrideStrategy overrideStrategy = InterfaceMemberOverrideStrategy.Default,
            object? tags = null )
            => throw new NotImplementedException();
    }
}