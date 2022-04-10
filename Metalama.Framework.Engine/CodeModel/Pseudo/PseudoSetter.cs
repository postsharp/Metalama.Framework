// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoSetter : PseudoAccessor<IFieldOrPropertyImpl>
    {
        public PseudoSetter( IFieldOrPropertyImpl property ) : base( property, MethodKind.PropertySet ) { }

        [Memo]
        public override IParameterList Parameters => new PseudoParameterList( new PseudoParameter( this, 0, this.DeclaringMember.Type, "value" ) );

        public override string Name => "set_" + this.DeclaringMember.Name;
    }
}