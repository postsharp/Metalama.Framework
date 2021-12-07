// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamespaceList : DeclarationList<INamespace, Ref<INamespace>>, INamespaceList
    {
        public NamespaceList( INamespace containingDeclaration, IEnumerable<Ref<INamespace>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        private NamespaceList() { }

        public static NamespaceList Empty { get; } = new();

        public INamespace? OfName( string name ) => this.SingleOrDefault( ns => ns.Name == name );

        public INamespace? OfFullName( string name ) => this.SingleOrDefault( ns => ns.FullName == name );
    }
}