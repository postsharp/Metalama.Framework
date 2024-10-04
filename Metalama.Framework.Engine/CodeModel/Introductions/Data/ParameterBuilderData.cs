// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using System;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class ParameterBuilderData : DeclarationBuilderData
{
    public string Name { get;  }

    public IRef<IType> Type { get;  }

    public RefKind RefKind { get;  }

    public int Index { get; }

    public TypedConstant? DefaultValue { get; }

    public bool IsParams { get; }

    public ParameterBuilderData( BaseParameterBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.Name = builder.Name;
        this.Type = builder.Type.ToRef();
        this.RefKind = builder.RefKind;
        this.Index = builder.Index;
        this.DefaultValue = builder.DefaultValue;
        this.IsParams = builder.IsParams;
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;
}