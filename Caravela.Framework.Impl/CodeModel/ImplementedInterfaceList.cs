// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel
{
    public class ImplementedInterfaceList : List<INamedType>, IImplementedInterfaceList
    {
        public ImplementedInterfaceList( IEnumerable<INamedType> interfaces ) : base( interfaces ) { }
    }
}