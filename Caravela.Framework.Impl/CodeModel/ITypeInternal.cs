// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ITypeInternal : ISdkType { }

    internal interface INamedTypeInternal : INamedType, ITypeInternal, IGenericInternal
    {
        /// <summary>
        /// Gets the set of methods that override a given member of a base type or interface. In case of
        /// a generic interface, this method can return several members. 
        /// </summary>
        IEnumerable<IMember> GetOverridingMembers( IMember member );

        bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember );
    }
}