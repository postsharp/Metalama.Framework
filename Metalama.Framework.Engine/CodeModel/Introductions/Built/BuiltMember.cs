// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

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

    public sealed override bool CanBeInherited => (this.IsAbstract || this.IsVirtual || this.IsOverride ) && !this.IsSealed;

    [Memo]
    public IMember? OverriddenMember => this.MapDeclaration( this.MemberBuilder.OverriddenMember );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
    {
        if ( !this.CanBeInherited )
        {
            return [];
        }
        else
        {
            return Member.GetDerivedDeclarationsCore( this, options );
        }
    }

    IMember IMember.Definition => this;

    IRef<IMember> IMember.ToRef() => throw new NotSupportedException();
}