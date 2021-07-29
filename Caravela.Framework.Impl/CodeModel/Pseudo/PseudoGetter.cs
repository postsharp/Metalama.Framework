// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.Collections;

namespace Caravela.Framework.Impl.CodeModel.Pseudo
{
    internal class PseudoGetter : PseudoAccessor<IFieldOrProperty>
    {
        public PseudoGetter( IFieldOrProperty property ) : base( property, MethodKind.PropertyGet ) { }

        public override IParameterList Parameters => ParameterList.Empty;

        public override string Name => "get_" + this.DeclaringMember.Name;
    }
}