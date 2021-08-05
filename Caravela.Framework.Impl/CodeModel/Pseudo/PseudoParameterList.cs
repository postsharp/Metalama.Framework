// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal class PseudoParameterList : List<PseudoParameter>, IParameterList
    {
        public PseudoParameterList( PseudoParameter parameter ) : base( 1 )
        {
            this.Add( parameter );
        }

        public PseudoParameterList( IEnumerable<PseudoParameter> collection ) : base( collection ) { }

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.Cast<IParameter>().GetEnumerator();

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

        public IParameter this[ string name ] => this.Single<IParameter>( p => p.Name == name );

        
    }
}