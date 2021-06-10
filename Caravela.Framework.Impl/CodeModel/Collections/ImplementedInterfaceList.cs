// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    public class ImplementedInterfaceList : List<INamedType>, IImplementedInterfaceList
    {
        public ImplementedInterfaceList( IEnumerable<INamedType> interfaces ) : base( interfaces ) { }

        public bool Contains( Type type ) => this.Any( i => i.Is( type ) );
    }
}