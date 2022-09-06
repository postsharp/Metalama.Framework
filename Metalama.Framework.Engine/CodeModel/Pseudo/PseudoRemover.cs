// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoRemover : PseudoAccessor<Event>
    {
        public override Accessibility Accessibility => this.DeclaringMember.Accessibility;

        public PseudoRemover( Event @event ) : base( @event, MethodKind.EventRemove ) { }

        [Memo]
        public override IParameterList Parameters => new PseudoParameterList( new PseudoParameter( this, 0, this.DeclaringMember.Type, "value" ) );

        public override string Name => "add_" + this.DeclaringMember.Name;
    }
}