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

    protected virtual IEnumerable<IRef<T>> GetMemberRefsOfName( string name )
        => this.ContainingDeclaration.GetStrategy()
            .GetMembersOfName(
                this.ContainingDeclaration,
                name,
                this.ItemsDeclarationKind,
                this.Compilation )
            .Cast<IRef<T>>();

    protected virtual IEnumerable<IRef<T>> GetMemberRefs()
        => this.ContainingDeclaration.GetStrategy()
            .GetMembers( this.ContainingDeclaration, this.ItemsDeclarationKind, this.Compilation )
            .Cast<IRef<T>>();
}