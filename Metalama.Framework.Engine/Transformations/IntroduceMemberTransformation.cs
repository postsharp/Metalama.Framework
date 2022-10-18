// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class IntroduceMemberTransformation<T> : IntroduceDeclarationTransformation<T>, IIntroduceMemberTransformation
    where T : MemberBuilder
{
    public IntroduceMemberTransformation( Advice advice, T introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context );

    public InsertPosition InsertPosition => this.IntroducedDeclaration.ToInsertPosition();

    protected SyntaxToken GetCleanName()
    {
        return
            SyntaxFactory.Identifier(
                this.IntroducedDeclaration.IsExplicitInterfaceImplementation
                    ? this.IntroducedDeclaration.Name.Split( '.' ).Last()
                    : this.IntroducedDeclaration.Name );
    }
}