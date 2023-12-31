// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;

namespace Metalama.Framework.Engine.CodeModel.Pseudo
{
    internal sealed class PseudoGetter : PseudoAccessor<IFieldOrPropertyOrIndexerImpl>
    {
        public override Accessibility Accessibility => this.DeclaringMember.Accessibility;

        public PseudoGetter( IFieldOrPropertyOrIndexerImpl property ) : base( property, MethodKind.PropertyGet ) { }

        public override IParameterList Parameters => ParameterList.Empty;

        public override string Name => "get_" + this.DeclaringMember.Name;
    }
}