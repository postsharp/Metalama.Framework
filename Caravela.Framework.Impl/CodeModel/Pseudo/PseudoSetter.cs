// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.Utilities;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal class PseudoSetter : PseudoAccessor<IFieldOrProperty>
    {
        public PseudoSetter( IFieldOrProperty property ) : base( property, MethodKind.PropertySet ) { }

        [Memo]
        public override IParameterList Parameters => new PseudoParameterList( new PseudoParameter( this, 0, this.DeclaringMember.Type, "value" ) );

        public override string Name => "set_" + this.DeclaringMember.Name;
    }
}