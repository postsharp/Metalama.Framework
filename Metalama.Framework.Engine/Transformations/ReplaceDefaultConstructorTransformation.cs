// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SymbolExtensions = Metalama.Framework.Engine.CodeModel.SymbolExtensions;

namespace Metalama.Framework.Engine.Transformations;

internal class ReplaceDefaultConstructorTransformation : IntroduceMemberTransformation<ConstructorBuilder>, IReplaceMemberTransformation
{
    public ReplaceDefaultConstructorTransformation( Advice advice, ConstructorBuilder introducedDeclaration ) : base( advice, introducedDeclaration )
    {
        var targetType = introducedDeclaration.DeclaringType;

        if ( targetType.Constructors.Any( c => SymbolExtensions.GetSymbol( (IMethodBase) c ).AssertNotNull().GetPrimarySyntaxReference() == null ) )
        {
            Invariant.Assert( targetType.Constructors.Count == 1 );
            this.ReplacedMember = targetType.Constructors.Single().ToMemberRef<IMember>();
        }
    }

    public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
    {
        var constructorBuilder = this.IntroducedDeclaration;

        if ( constructorBuilder.IsStatic )
        {
            var syntax =
                SyntaxFactory.ConstructorDeclaration(
                    constructorBuilder.GetAttributeLists( context ),
                    SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.StaticKeyword ) ),
                    ((TypeDeclarationSyntax) constructorBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                    SyntaxFactory.ParameterList(),
                    null,
                    SyntaxFactory.Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[]
            {
                new IntroducedMember(
                    this,
                    syntax,
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.Introduction,
                    constructorBuilder )
            };
        }
        else
        {
            var syntax =
                SyntaxFactory.ConstructorDeclaration(
                    constructorBuilder.GetAttributeLists( context ),
                    SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.PublicKeyword ) ),
                    ((TypeDeclarationSyntax) constructorBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                    SyntaxFactory.ParameterList(),
                    null,
                    SyntaxFactory.Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[]
            {
                new IntroducedMember(
                    this,
                    syntax,
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.Introduction,
                    constructorBuilder )
            };
        }
    }

    public MemberRef<IMember> ReplacedMember { get; }

    public override TransformationObservability Observability => TransformationObservability.None;
}