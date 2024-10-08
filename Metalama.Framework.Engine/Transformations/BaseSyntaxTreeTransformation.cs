// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class BaseSyntaxTreeTransformation : BaseTransformation, ISyntaxTreeTransformation
{
    protected BaseSyntaxTreeTransformation( AdviceInfo advice, SyntaxTree transformedSyntaxTree ) : base( advice )
    {
        this.TransformedSyntaxTree = transformedSyntaxTree;
    }

    protected BaseSyntaxTreeTransformation( AdviceInfo advice, IFullRef<IDeclaration> targetDeclaration ) : base( advice )
    {
        this.TransformedSyntaxTree = targetDeclaration.GetPrimarySyntaxTree()
                                     ?? advice.SourceCompilation.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;
    }

    public SyntaxTree TransformedSyntaxTree { get; }
}