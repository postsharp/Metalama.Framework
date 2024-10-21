// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class MemberUpdatableCollection<T> : DeclarationUpdatableCollection<T>
    where T : class, INamedDeclaration
{
    protected MemberUpdatableCollection( CompilationModel compilation, IRef containingDeclaration ) : base( compilation )
    {
        this.ContainingDeclaration = containingDeclaration;
    }

    protected IRef ContainingDeclaration { get; }

    protected abstract DeclarationKind ItemsDeclarationKind { get; }

    protected virtual IEnumerable<IFullRef<T>> GetMemberRefsOfName( string name )
        => this.ContainingDeclaration.AsFullRef()
            .GetMembersOfName(
                name,
                this.ItemsDeclarationKind,
                this.Compilation )
            .Cast<IFullRef<T>>();

    protected virtual IEnumerable<IFullRef<T>> GetMemberRefs()
        => this.ContainingDeclaration.AsFullRef()
            .GetMembers( this.ItemsDeclarationKind, this.Compilation )
            .Cast<IFullRef<T>>();
}