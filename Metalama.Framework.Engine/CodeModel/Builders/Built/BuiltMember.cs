// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Data;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal abstract class BuiltMember : BuiltMemberOrNamedType, IMemberImpl
{
    protected BuiltMember( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract MemberBuilderData MemberBuilder { get; }

    public abstract bool IsExplicitInterfaceImplementation { get; }

    public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

    public bool IsVirtual => this.MemberBuilder.IsVirtual;

    public bool IsAsync => this.MemberBuilder.IsAsync;

    public bool IsOverride => this.MemberBuilder.IsOverride;

    public bool HasImplementation => !this.IsAbstract; // TODO - partials?

    [Memo]
    public IMember? OverriddenMember => this.MapDeclaration( this.MemberBuilder.OverriddenMember );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
    {
        if ( !this.CanBeInherited )
        {
            return Enumerable.Empty<IDeclaration>();
        }
        else
        {
            return Member.GetDerivedDeclarationsCore( this, options );
        }
    }

    IMember IMember.Definition => this;

    public abstract IRef<IMember> ToMemberRef();

    IRef<IMember> IMember.ToRef() => this.ToMemberRef();
}