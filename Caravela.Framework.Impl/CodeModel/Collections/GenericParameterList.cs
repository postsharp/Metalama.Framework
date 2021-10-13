// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class GenericParameterList : DeclarationList<ITypeParameter, DeclarationRef<ITypeParameter>>, IGenericParameterList
    {
        public static GenericParameterList Empty { get; } = new();

        private GenericParameterList() { }

        public GenericParameterList( INamedType containingDeclaration, IEnumerable<DeclarationRef<ITypeParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        public GenericParameterList( IMethod containingDeclaration, IEnumerable<DeclarationRef<ITypeParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }
    }
}