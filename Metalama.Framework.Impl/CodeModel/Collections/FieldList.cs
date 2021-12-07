// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CodeModel.Collections
{
    internal class FieldList : MemberOrNamedTypeList<IField, MemberRef<IField>>, IFieldList
    {
        public FieldList( INamedType containingDeclaration, IEnumerable<MemberRef<IField>> sourceItems ) : base( containingDeclaration, sourceItems ) { }
    }
}