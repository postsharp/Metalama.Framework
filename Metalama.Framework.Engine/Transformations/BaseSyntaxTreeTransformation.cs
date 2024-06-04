// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class BaseSyntaxTreeTransformation : BaseTransformation, ISyntaxTreeTransformation
{
    protected BaseSyntaxTreeTransformation( Advice advice ) : base( advice ) { }

    public virtual SyntaxTree TransformedSyntaxTree => this.TargetDeclaration.GetPrimarySyntaxTree().AssertNotNull();
}