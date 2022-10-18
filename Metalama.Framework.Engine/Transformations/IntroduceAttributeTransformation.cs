// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroduceAttributeTransformation : IntroduceDeclarationTransformation<AttributeBuilder>
{
    public IntroduceAttributeTransformation( Advice advice, AttributeBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }
}