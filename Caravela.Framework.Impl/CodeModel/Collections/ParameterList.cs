// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ParameterList : DeclarationList<IParameter, DeclarationRef<IParameter>>, IParameterList
    {
        public ParameterList( IMethodBase containingDeclaration, IEnumerable<DeclarationRef<IParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        public ParameterList( IProperty containingDeclaration, IEnumerable<DeclarationRef<IParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        private ParameterList() { }

        public static ParameterList Empty { get; } = new();

        public IParameter this[ string name ] => this.Single( p => p.Name == name );
    }
}