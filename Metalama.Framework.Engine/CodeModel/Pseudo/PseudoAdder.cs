// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoAdder : PseudoAccessor<Event>
    {
        public override Accessibility Accessibility => this.DeclaringMember.Accessibility;

        public PseudoAdder( Event @event ) : base( @event, MethodKind.EventAdd ) { }

        [Memo]
        public override IParameterList Parameters => new PseudoParameterList( new PseudoParameter( this, 0, this.DeclaringMember.Type, "value" ) );

        public override string Name => "add_" + this.DeclaringMember.Name;
    }
}