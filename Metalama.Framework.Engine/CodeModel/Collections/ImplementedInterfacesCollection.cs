// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class ImplementedInterfacesCollection : DeclarationCollection<INamedType, Ref<INamedType>>, IImplementedInterfaceCollection
    {
        public ImplementedInterfacesCollection( NamedType declaringType, InterfaceUpdatableCollection source ) : base( declaringType, source ) { }

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