// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoParameterList : List<PseudoParameter>, IParameterList
    {
        public PseudoParameterList( PseudoParameter parameter ) : base( 1 )
        {
            this.Add( parameter );
        }

        public PseudoParameterList( IEnumerable<PseudoParameter> collection ) : base( collection ) { }

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

        public IParameter this[ string name ] => this.Single<IParameter>( p => p.Name == name );
    }
}