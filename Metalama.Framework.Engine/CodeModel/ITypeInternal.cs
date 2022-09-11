﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel
{
    internal interface ITypeInternal : ISdkType
    {
        ITypeInternal Accept( TypeRewriter visitor );
    }

    internal interface INamedTypeInternal : INamedType, ITypeInternal, IGenericInternal, IDeclarationImpl
    {
        /// <summary>
        /// Gets the set of methods that override a given member of a base type or interface. In case of
        /// a generic interface, this method can return several members. 
        /// </summary>
        IEnumerable<IMember> GetOverridingMembers( IMember member );

        bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember );
    }
}