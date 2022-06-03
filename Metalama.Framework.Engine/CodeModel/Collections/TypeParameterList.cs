// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class TypeParameterList : DeclarationCollection<ITypeParameter, Ref<ITypeParameter>>, IGenericParameterList
    {
        public static TypeParameterList Empty { get; } = new();

        private TypeParameterList() { }

        public TypeParameterList( INamedType declaringType, IReadOnlyList<Ref<ITypeParameter>> sourceItems ) : base(
            declaringType,
            sourceItems ) { }

        public TypeParameterList( IMethod declaringType, IReadOnlyList<Ref<ITypeParameter>> sourceItems ) : base(
            declaringType,
            sourceItems ) { }

        public ITypeParameter this[ int index ] => this.Source[index].GetTarget( this.Compilation );
    }
}