// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ParameterList : DeclarationList<IParameter, DeclarationRef<IParameter>>, IParameterList
    {
        public ParameterList( IMethodBase containingElement, IEnumerable<DeclarationRef<IParameter>> sourceItems ) : base( containingElement, sourceItems ) { }

        public ParameterList( IProperty containingElement, IEnumerable<DeclarationRef<IParameter>> sourceItems ) : base( containingElement, sourceItems ) { }

        private ParameterList() { }

        public static ParameterList Empty { get; } = new();
    }
}