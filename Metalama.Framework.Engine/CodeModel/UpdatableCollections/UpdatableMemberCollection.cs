// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UpdatableMemberCollection<TDeclaration, TRef> : UpdatableDeclarationCollection<TDeclaration, TRef>
    where TDeclaration : class, INamedDeclaration
    where TRef : class, IRef
{
    protected UpdatableMemberCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringType ) : base( compilation )
    {
        this.DeclaringTypeOrNamespace = declaringType;
    }

    protected IRef<INamespaceOrNamedType> DeclaringTypeOrNamespace { get; }

    protected abstract DeclarationKind DeclarationKind { get; }

    protected virtual IEnumerable<TRef> GetMemberRefsOfName( string name )
        => this.DeclaringTypeOrNamespace.GetStrategy()
            .GetMembersOfName(
                this.DeclaringTypeOrNamespace,
                name,
                this.DeclarationKind,
                this.Compilation )
            .Cast<TRef>();

    protected virtual IEnumerable<TRef> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.GetStrategy().GetMembers( this.DeclaringTypeOrNamespace, this.DeclarationKind, this.Compilation ).Cast<TRef>();
}