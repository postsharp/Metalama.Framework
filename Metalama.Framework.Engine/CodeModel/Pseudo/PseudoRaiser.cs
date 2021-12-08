// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoRaiser : PseudoAccessor<Event>
    {
        public PseudoRaiser( Event @event ) : base( @event, MethodKind.EventRaise ) { }

        [Memo]
        public override IParameterList Parameters
            => new PseudoParameterList(
                this.DeclaringMember.Type.Methods.OfName( "Invoke" )
                    .Single()
                    .Parameters.Select( p => new PseudoParameter( this, p.Index, p.Type, p.Name ) ) );

        public override string Name => "raise_" + this.DeclaringMember.Name;
    }
}