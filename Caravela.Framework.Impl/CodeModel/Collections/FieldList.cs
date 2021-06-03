// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class FieldList : MemberList<IField, MemberRef<IField>>, IFieldList
    {
        public FieldList( INamedType containingDeclaration, IEnumerable<MemberRef<IField>> sourceItems ) : base( containingDeclaration, sourceItems ) { }
    }
}