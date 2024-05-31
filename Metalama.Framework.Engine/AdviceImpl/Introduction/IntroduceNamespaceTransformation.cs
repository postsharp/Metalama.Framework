// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceTransformation : IntroduceDeclarationTransformation<NamespaceBuilder>
{
    public IntroduceNamespaceTransformation( Advice advice, NamespaceBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override TransformationObservability Observability => TransformationObservability.Always;

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        // Assembly the qualified namespace name.
        var namespaceStack = new Stack<INamespace>();
        var currentNamespace = (INamespace)this.IntroducedDeclaration;

        while ( !currentNamespace.AssertNotNull().IsGlobalNamespace )
        {
            namespaceStack.Push( currentNamespace );
            currentNamespace = currentNamespace.ContainingNamespace;
        }

        NameSyntax currentName = IdentifierName( Identifier( TriviaList( ElasticSpace ), namespaceStack.Pop().Name, TriviaList() ) );

        while ( namespaceStack.Count > 0 )
        {
            var @namespace = namespaceStack.Pop();
            currentName = QualifiedName( currentName, IdentifierName( @namespace.Name ) );
        }

        yield return new InjectedMember(
            this,
            NamespaceDeclaration(
                currentName,
                List<ExternAliasDirectiveSyntax>(),
                List<UsingDirectiveSyntax>(),
                List<MemberDeclarationSyntax>() ),
            this.ParentAdvice.AspectLayerId,
            InjectedMemberSemantic.Introduction,
            this.IntroducedDeclaration );
    }

    public override SyntaxTree TransformedSyntaxTree => this.IntroducedDeclaration.PrimarySyntaxTree;
}