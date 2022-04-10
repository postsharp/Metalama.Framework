// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal class PseudoGetter : PseudoAccessor<IFieldOrPropertyImpl>
    {
        public PseudoGetter( IFieldOrPropertyImpl property ) : base( property, MethodKind.PropertyGet ) { }

        public override IParameterList Parameters => ParameterList.Empty;

        public override string Name => "get_" + this.DeclaringMember.Name;
    }
}