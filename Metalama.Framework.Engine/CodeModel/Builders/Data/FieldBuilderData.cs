// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class FieldBuilderData : MemberBuilderData
{
    public IRef<IType> Type { get; }

    public Writeability Writeability { get; }

    public IObjectReader InitializerTags { get; }

    public RefKind RefKind { get; }

    public FieldBuilderData( FieldBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.Type = builder.Type.ToRef();
        this.Writeability = builder.Writeability;
        this.RefKind = builder.RefKind;

        // TODO: Potentional CompilationModel leak. (they could be safely ignored at design time)
        this.InitializerTags = builder.InitializerTags;
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Field;
}