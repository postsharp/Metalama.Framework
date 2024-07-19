// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class ImplementedInterfacesCollection : DeclarationCollection<INamedType, Ref<INamedType>>, IImplementedInterfaceCollection
    {
        public ImplementedInterfacesCollection( INamedType declaringType, InterfaceUpdatableCollection source ) : base( declaringType, source ) { }

        public bool Contains( INamedType namedType ) => ((InterfaceUpdatableCollection) this.Source).Contains( namedType.ToTypedRef() );

        public bool Contains( Type type )
        {
            var itype = ((ICompilationInternal) this.ContainingDeclaration!.Compilation).Factory.GetTypeByReflectionType( type );

            if ( itype is not INamedType namedType )
            {
                return false;
            }

            return this.Contains( namedType );
        }
    }
}