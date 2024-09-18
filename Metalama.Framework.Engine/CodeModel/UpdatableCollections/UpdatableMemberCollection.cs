// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UpdatableMemberCollection<T> : UpdatableDeclarationCollection<T>, ISourceMemberCollection<T>
    where T : class, INamedDeclaration
{
    protected UpdatableMemberCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringType ) : base( compilation )
    {
        this.DeclaringTypeOrNamespace = declaringType;
    }

    protected IRef<INamespaceOrNamedType> DeclaringTypeOrNamespace { get; }

    protected abstract DeclarationKind DeclarationKind { get; }

    public abstract ImmutableArray<IRef<T>> OfName( string name );

    protected virtual IEnumerable<IRef<T>> GetMemberRefsOfName( string name )
        => this.DeclaringTypeOrNamespace.GetStrategy().GetMembersOfName<T>(
            this.DeclaringTypeOrNamespace,
            name,
            this.DeclarationKind,
            this.Compilation );

    protected virtual IEnumerable<IRef<T>> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.GetStrategy().GetMembers<T>( this.DeclaringTypeOrNamespace, this.DeclarationKind, this.Compilation );
}