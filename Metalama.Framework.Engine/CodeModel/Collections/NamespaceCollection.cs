// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class NamespaceCollection : DeclarationCollection<INamespace, Ref<INamespace>>, INamespaceCollection
    {
        public NamespaceCollection( INamespace declaringType, IReadOnlyList<Ref<INamespace>> sourceItems ) : base(
            declaringType,
            sourceItems ) { }

        public INamespace? OfName( string name ) => this.SingleOrDefault( ns => ns.Name == name );
    }
}