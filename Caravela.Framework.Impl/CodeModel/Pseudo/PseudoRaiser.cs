// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal class PseudoRaiser : PseudoAccessor<Event>
    {
        public PseudoRaiser( Event @event ) : base( @event, MethodKind.EventRaise ) { }

        [Memo]
        public override IParameterList Parameters
            => new PseudoParameterList(
                this.DeclaringMember.EventType.Methods.OfName( "Invoke" )
                    .Single()
                    .Parameters.Select( p => new PseudoParameter( this, p.Index, p.ParameterType, p.Name ) ) );

        public override string Name => "add_" + this.DeclaringMember.Name;
    }
}