// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Linq.Expressions;

namespace Caravela.Framework.Aspects
{
    public sealed class InterfaceMemberSpecification
    {
#pragma warning disable IDE0051 // Remove unused private members
        private InterfaceMemberSpecification(IMember interfaceMember, IMember implementationMember)
#pragma warning restore IDE0051 // Remove unused private members
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the member of the interface to be implemented.
        /// </summary>
        public IMember InterfaceMember { get; }

        /// <summary>
        /// Gets the implementation member, i.e. either a callable member of the target type or member of the aspect marked with [Introduce] or [InterfaceMember].
        /// </summary>
        public IMember ImplementationMember { get; }

        [Obsolete( "Not implemented." )]
        public static InterfaceMemberSpecification Create<TInterfaceType, TReturnType>(
            Expression<Func<TInterfaceType, TReturnType>> expression,
            string aspectMemberName )
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public static InterfaceMemberSpecification Create<TInterfaceType, TReturnType>(
            Expression<Func<TInterfaceType, TReturnType>> expression,
            IMember targetTypeMember )
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public static InterfaceMemberSpecification Create( IMember interfaceMember, string aspectMemberName ) => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public static InterfaceMemberSpecification Create( IMember interfaceMember, IMember targetTypeMember ) => throw new NotImplementedException();
    }
}