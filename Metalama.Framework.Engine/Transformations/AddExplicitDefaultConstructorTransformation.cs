// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.Transformations;

internal class AddExplicitDefaultConstructorTransformation : BaseTransformation, ITypeLevelTransformation
{
    public AddExplicitDefaultConstructorTransformation( Advice advice, INamedType type ) : base( advice )
    {
        this.TargetType = type;
    }

    public INamedType TargetType { get; }
}