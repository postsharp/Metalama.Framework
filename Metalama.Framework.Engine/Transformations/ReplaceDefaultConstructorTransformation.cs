// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class ReplaceDefaultConstructorTransformation : IntroduceMemberTransformation<ConstructorBuilder>, IReplaceMemberTransformation
{
    public ReplaceDefaultConstructorTransformation( Advice advice, ConstructorBuilder introducedDeclaration ) : base( advice, introducedDeclaration )
    {
        Invariant.Assert( !introducedDeclaration.IsStatic );

        var targetType = introducedDeclaration.DeclaringType;

        if ( targetType.Constructors.Any( c => c.GetSymbol().AssertNotNull().GetPrimarySyntaxReference() == null ) )
        {
            this.ReplacedMember = targetType.Constructors.OfExactSignature( Array.Empty<IType>() ).AssertNotNull().ToMemberRef<IMember>();
        }
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var constructorBuilder = this.IntroducedDeclaration;

        var syntax =
            ConstructorDeclaration(
                constructorBuilder.GetAttributeLists( context ),
                TokenList( Token( TriviaList(), SyntaxKind.PublicKeyword, TriviaList( Space ) ) ),
                ((TypeDeclarationSyntax) constructorBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                ParameterList(),
                null,
                SyntaxFactoryEx.FormattedBlock().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                null );

        return new[]
        {
            new InjectedMember(
                this,
                syntax,
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Introduction,
                constructorBuilder )
        };
    }

    public MemberRef<IMember> ReplacedMember { get; }

    public override InsertPosition InsertPosition => this.ReplacedMember.GetTarget( this.TargetDeclaration.Compilation ).ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;
}