// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations;

internal class AddExplicitDefaultConstructorTransformation : BaseTransformation, ITypeLevelTransformation
{
    public AddExplicitDefaultConstructorTransformation( Advice advice, INamedType type ) : base( advice )
    {
        this.TargetType = type;
    }

    public INamedType TargetType { get; }

    public override IDeclaration TargetDeclaration => this.TargetType;

}