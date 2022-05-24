// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoSetter : PseudoAccessor<IFieldOrPropertyImpl>
    {
        private readonly Accessibility? _accessibility;

        public override Accessibility Accessibility => this._accessibility ?? this.DeclaringMember.Accessibility;

        public PseudoSetter( IFieldOrPropertyImpl property, Accessibility? accessibility ) : base( property, MethodKind.PropertySet ) 
        {
            this._accessibility = accessibility;
        }

        [Memo]
        public override IParameterList Parameters => new PseudoParameterList( new PseudoParameter( this, 0, this.DeclaringMember.Type, "value" ) );

        public override string Name => "set_" + this.DeclaringMember.Name;
    }
}