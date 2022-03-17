// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class GenericParameterList : DeclarationList<ITypeParameter, Ref<ITypeParameter>>, IGenericParameterList
    {
        public static GenericParameterList Empty { get; } = new();

        private GenericParameterList() { }

        public GenericParameterList( INamedType containingDeclaration, IEnumerable<Ref<ITypeParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        public GenericParameterList( IMethod containingDeclaration, IEnumerable<Ref<ITypeParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }
    }
}