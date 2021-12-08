// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CodeModel.Collections
{
    internal static class ImplementedInterfaceListExtensions
    {
        public static ImplementedInterfaceList ToImplementedInterfaceList( this IEnumerable<INamedType> interfaces )
        {
            return new ImplementedInterfaceList( interfaces );
        }
    }
}