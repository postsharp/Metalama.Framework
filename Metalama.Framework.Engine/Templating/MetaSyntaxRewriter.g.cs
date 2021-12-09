#pragma warning disable CS8669 // Nullability
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating
{
	partial class MetaSyntaxRewriter
	{
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAccessorDeclaration( node );
				default: 
					return base.VisitAccessorDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAccessorDeclaration( AccessorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AccessorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAccessorList( AccessorListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAccessorList( node );
				default: 
					return base.VisitAccessorList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAccessorList( AccessorListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AccessorList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Accessors)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAliasQualifiedName( AliasQualifiedNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAliasQualifiedName( node );
				default: 
					return base.VisitAliasQualifiedName( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAliasQualifiedName( AliasQualifiedNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AliasQualifiedName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Alias)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAnonymousMethodExpression( node );
				default: 
					return base.VisitAnonymousMethodExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAnonymousMethodExpression( AnonymousMethodExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AnonymousMethodExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAnonymousObjectCreationExpression( node );
				default: 
					return base.VisitAnonymousObjectCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AnonymousObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAnonymousObjectMemberDeclarator( node );
				default: 
					return base.VisitAnonymousObjectMemberDeclarator( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AnonymousObjectMemberDeclarator))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameEquals)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArgument( ArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArgument( node );
				default: 
					return base.VisitArgument( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArgument( ArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Argument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.RefKindKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArgumentList( ArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArgumentList( node );
				default: 
					return base.VisitArgumentList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArgumentList( ArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArrayCreationExpression( ArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArrayCreationExpression( node );
				default: 
					return base.VisitArrayCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArrayCreationExpression( ArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArrayRankSpecifier( ArrayRankSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArrayRankSpecifier( node );
				default: 
					return base.VisitArrayRankSpecifier( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArrayRankSpecifier( ArrayRankSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ArrayRankSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Sizes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArrayType( ArrayTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArrayType( node );
				default: 
					return base.VisitArrayType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArrayType( ArrayTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ArrayType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.RankSpecifiers)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformArrowExpressionClause( node );
				default: 
					return base.VisitArrowExpressionClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformArrowExpressionClause( ArrowExpressionClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ArrowExpressionClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAssignmentExpression( AssignmentExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAssignmentExpression( node );
				default: 
					return base.VisitAssignmentExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAssignmentExpression( AssignmentExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AssignmentExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAttribute( AttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAttribute( node );
				default: 
					return base.VisitAttribute( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAttribute( AttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Attribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAttributeArgument( AttributeArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAttributeArgument( node );
				default: 
					return base.VisitAttributeArgument( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAttributeArgument( AttributeArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AttributeArgument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameEquals)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.NameColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAttributeArgumentList( AttributeArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAttributeArgumentList( node );
				default: 
					return base.VisitAttributeArgumentList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAttributeArgumentList( AttributeArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AttributeArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAttributeList( AttributeListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAttributeList( node );
				default: 
					return base.VisitAttributeList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAttributeList( AttributeListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AttributeList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Target)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAttributeTargetSpecifier( AttributeTargetSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAttributeTargetSpecifier( node );
				default: 
					return base.VisitAttributeTargetSpecifier( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAttributeTargetSpecifier( AttributeTargetSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AttributeTargetSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitAwaitExpression( AwaitExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformAwaitExpression( node );
				default: 
					return base.VisitAwaitExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformAwaitExpression( AwaitExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(AwaitExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBadDirectiveTrivia( BadDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBadDirectiveTrivia( node );
				default: 
					return base.VisitBadDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBadDirectiveTrivia( BadDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BadDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBaseExpression( BaseExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBaseExpression( node );
				default: 
					return base.VisitBaseExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBaseExpression( BaseExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BaseExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBaseList( BaseListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBaseList( node );
				default: 
					return base.VisitBaseList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBaseList( BaseListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BaseList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Types)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBinaryExpression( BinaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBinaryExpression( node );
				default: 
					return base.VisitBinaryExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBinaryExpression( BinaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BinaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBinaryPattern( BinaryPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBinaryPattern( node );
				default: 
					return base.VisitBinaryPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBinaryPattern( BinaryPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BinaryPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBlock( BlockSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBlock( node );
				default: 
					return base.VisitBlock( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBlock( BlockSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Block))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statements)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBracketedArgumentList( BracketedArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBracketedArgumentList( node );
				default: 
					return base.VisitBracketedArgumentList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBracketedArgumentList( BracketedArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BracketedArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBracketedParameterList( BracketedParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBracketedParameterList( node );
				default: 
					return base.VisitBracketedParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBracketedParameterList( BracketedParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BracketedParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitBreakStatement( BreakStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformBreakStatement( node );
				default: 
					return base.VisitBreakStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformBreakStatement( BreakStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(BreakStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BreakKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCasePatternSwitchLabel( node );
				default: 
					return base.VisitCasePatternSwitchLabel( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CasePatternSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhenClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCaseSwitchLabel( CaseSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCaseSwitchLabel( node );
				default: 
					return base.VisitCaseSwitchLabel( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCaseSwitchLabel( CaseSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CaseSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCastExpression( CastExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCastExpression( node );
				default: 
					return base.VisitCastExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCastExpression( CastExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CastExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCatchClause( CatchClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCatchClause( node );
				default: 
					return base.VisitCatchClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCatchClause( CatchClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CatchClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.CatchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Filter)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCatchDeclaration( CatchDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCatchDeclaration( node );
				default: 
					return base.VisitCatchDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCatchDeclaration( CatchDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CatchDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCatchFilterClause( CatchFilterClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCatchFilterClause( node );
				default: 
					return base.VisitCatchFilterClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCatchFilterClause( CatchFilterClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CatchFilterClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhenKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.FilterExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformClassDeclaration( node );
				default: 
					return base.VisitClassDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformClassDeclaration( ClassDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ClassDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitClassOrStructConstraint( ClassOrStructConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformClassOrStructConstraint( node );
				default: 
					return base.VisitClassOrStructConstraint( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformClassOrStructConstraint( ClassOrStructConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ClassOrStructConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ClassOrStructKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCompilationUnit( node );
				default: 
					return base.VisitCompilationUnit( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCompilationUnit( CompilationUnitSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CompilationUnit))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Externs)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Usings)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfFileToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConditionalAccessExpression( node );
				default: 
					return base.VisitConditionalAccessExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConditionalAccessExpression( ConditionalAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConditionalAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhenNotNull)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConditionalExpression( ConditionalExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConditionalExpression( node );
				default: 
					return base.VisitConditionalExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConditionalExpression( ConditionalExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConditionalExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhenTrue)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhenFalse)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConstantPattern( ConstantPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConstantPattern( node );
				default: 
					return base.VisitConstantPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConstantPattern( ConstantPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConstantPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConstructorConstraint( ConstructorConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConstructorConstraint( node );
				default: 
					return base.VisitConstructorConstraint( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConstructorConstraint( ConstructorConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConstructorConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConstructorDeclaration( node );
				default: 
					return base.VisitConstructorDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConstructorDeclaration( ConstructorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConstructorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConstructorInitializer( ConstructorInitializerSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConstructorInitializer( node );
				default: 
					return base.VisitConstructorInitializer( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConstructorInitializer( ConstructorInitializerSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConstructorInitializer))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ThisOrBaseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitContinueStatement( ContinueStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformContinueStatement( node );
				default: 
					return base.VisitContinueStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformContinueStatement( ContinueStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ContinueStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ContinueKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConversionOperatorDeclaration( node );
				default: 
					return base.VisitConversionOperatorDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConversionOperatorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ImplicitOrExplicitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitConversionOperatorMemberCref( ConversionOperatorMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformConversionOperatorMemberCref( node );
				default: 
					return base.VisitConversionOperatorMemberCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformConversionOperatorMemberCref( ConversionOperatorMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ConversionOperatorMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ImplicitOrExplicitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCrefBracketedParameterList( CrefBracketedParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCrefBracketedParameterList( node );
				default: 
					return base.VisitCrefBracketedParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCrefBracketedParameterList( CrefBracketedParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CrefBracketedParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCrefParameter( CrefParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCrefParameter( node );
				default: 
					return base.VisitCrefParameter( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCrefParameter( CrefParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CrefParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKindKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCrefParameterList( CrefParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCrefParameterList( node );
				default: 
					return base.VisitCrefParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCrefParameterList( CrefParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CrefParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDeclarationExpression( DeclarationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDeclarationExpression( node );
				default: 
					return base.VisitDeclarationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDeclarationExpression( DeclarationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DeclarationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDeclarationPattern( DeclarationPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDeclarationPattern( node );
				default: 
					return base.VisitDeclarationPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDeclarationPattern( DeclarationPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DeclarationPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDefaultConstraint( DefaultConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDefaultConstraint( node );
				default: 
					return base.VisitDefaultConstraint( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDefaultConstraint( DefaultConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DefaultConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.DefaultKeyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDefaultExpression( DefaultExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDefaultExpression( node );
				default: 
					return base.VisitDefaultExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDefaultExpression( DefaultExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DefaultExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDefaultSwitchLabel( DefaultSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDefaultSwitchLabel( node );
				default: 
					return base.VisitDefaultSwitchLabel( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDefaultSwitchLabel( DefaultSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DefaultSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDefineDirectiveTrivia( DefineDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDefineDirectiveTrivia( node );
				default: 
					return base.VisitDefineDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDefineDirectiveTrivia( DefineDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DefineDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DefineKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDelegateDeclaration( DelegateDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDelegateDeclaration( node );
				default: 
					return base.VisitDelegateDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDelegateDeclaration( DelegateDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DelegateDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDestructorDeclaration( DestructorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDestructorDeclaration( node );
				default: 
					return base.VisitDestructorDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDestructorDeclaration( DestructorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DestructorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TildeToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDiscardDesignation( DiscardDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDiscardDesignation( node );
				default: 
					return base.VisitDiscardDesignation( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDiscardDesignation( DiscardDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DiscardDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.UnderscoreToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDiscardPattern( DiscardPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDiscardPattern( node );
				default: 
					return base.VisitDiscardPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDiscardPattern( DiscardPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DiscardPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.UnderscoreToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDocumentationCommentTrivia( DocumentationCommentTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDocumentationCommentTrivia( node );
				default: 
					return base.VisitDocumentationCommentTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDocumentationCommentTrivia( DocumentationCommentTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DocumentationCommentTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Content)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfComment)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitDoStatement( DoStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformDoStatement( node );
				default: 
					return base.VisitDoStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformDoStatement( DoStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(DoStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhileKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitElementAccessExpression( ElementAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformElementAccessExpression( node );
				default: 
					return base.VisitElementAccessExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformElementAccessExpression( ElementAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ElementAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitElementBindingExpression( ElementBindingExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformElementBindingExpression( node );
				default: 
					return base.VisitElementBindingExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformElementBindingExpression( ElementBindingExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ElementBindingExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitElifDirectiveTrivia( ElifDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformElifDirectiveTrivia( node );
				default: 
					return base.VisitElifDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformElifDirectiveTrivia( ElifDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ElifDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ElifKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConditionValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitElseClause( ElseClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformElseClause( node );
				default: 
					return base.VisitElseClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformElseClause( ElseClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ElseClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformElseDirectiveTrivia( node );
				default: 
					return base.VisitElseDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformElseDirectiveTrivia( ElseDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ElseDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ElseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEmptyStatement( EmptyStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEmptyStatement( node );
				default: 
					return base.VisitEmptyStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEmptyStatement( EmptyStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EmptyStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEndIfDirectiveTrivia( node );
				default: 
					return base.VisitEndIfDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EndIfDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndIfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEndRegionDirectiveTrivia( EndRegionDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEndRegionDirectiveTrivia( node );
				default: 
					return base.VisitEndRegionDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEndRegionDirectiveTrivia( EndRegionDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EndRegionDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndRegionKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEnumDeclaration( EnumDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEnumDeclaration( node );
				default: 
					return base.VisitEnumDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEnumDeclaration( EnumDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EnumDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EnumKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEnumMemberDeclaration( EnumMemberDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEnumMemberDeclaration( node );
				default: 
					return base.VisitEnumMemberDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEnumMemberDeclaration( EnumMemberDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EnumMemberDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEqualsValueClause( EqualsValueClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEqualsValueClause( node );
				default: 
					return base.VisitEqualsValueClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEqualsValueClause( EqualsValueClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EqualsValueClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitErrorDirectiveTrivia( ErrorDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformErrorDirectiveTrivia( node );
				default: 
					return base.VisitErrorDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformErrorDirectiveTrivia( ErrorDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ErrorDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ErrorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEventDeclaration( EventDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEventDeclaration( node );
				default: 
					return base.VisitEventDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEventDeclaration( EventDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EventDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EventKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformEventFieldDeclaration( node );
				default: 
					return base.VisitEventFieldDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformEventFieldDeclaration( EventFieldDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(EventFieldDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EventKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitExplicitInterfaceSpecifier( ExplicitInterfaceSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformExplicitInterfaceSpecifier( node );
				default: 
					return base.VisitExplicitInterfaceSpecifier( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformExplicitInterfaceSpecifier( ExplicitInterfaceSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ExplicitInterfaceSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitExpressionColon( ExpressionColonSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformExpressionColon( node );
				default: 
					return base.VisitExpressionColon( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformExpressionColon( ExpressionColonSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ExpressionColon))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitExpressionStatement( ExpressionStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformExpressionStatement( node );
				default: 
					return base.VisitExpressionStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformExpressionStatement( ExpressionStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ExpressionStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitExternAliasDirective( ExternAliasDirectiveSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformExternAliasDirective( node );
				default: 
					return base.VisitExternAliasDirective( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformExternAliasDirective( ExternAliasDirectiveSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ExternAliasDirective))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ExternKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AliasKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFieldDeclaration( FieldDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFieldDeclaration( node );
				default: 
					return base.VisitFieldDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFieldDeclaration( FieldDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FieldDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFileScopedNamespaceDeclaration( node );
				default: 
					return base.VisitFileScopedNamespaceDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FileScopedNamespaceDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.NamespaceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Externs)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Usings)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFinallyClause( FinallyClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFinallyClause( node );
				default: 
					return base.VisitFinallyClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFinallyClause( FinallyClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FinallyClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FinallyKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFixedStatement( FixedStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFixedStatement( node );
				default: 
					return base.VisitFixedStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFixedStatement( FixedStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FixedStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.FixedKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitForEachStatement( ForEachStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformForEachStatement( node );
				default: 
					return base.VisitForEachStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformForEachStatement( ForEachStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ForEachStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ForEachKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitForEachVariableStatement( ForEachVariableStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformForEachVariableStatement( node );
				default: 
					return base.VisitForEachVariableStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformForEachVariableStatement( ForEachVariableStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ForEachVariableStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ForEachKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Variable)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitForStatement( ForStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformForStatement( node );
				default: 
					return base.VisitForStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformForStatement( ForStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ForStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ForKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.FirstSemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SecondSemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Incrementors)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFromClause( FromClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFromClause( node );
				default: 
					return base.VisitFromClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFromClause( FromClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FromClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FromKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerCallingConvention( FunctionPointerCallingConventionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerCallingConvention( node );
				default: 
					return base.VisitFunctionPointerCallingConvention( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerCallingConvention( FunctionPointerCallingConventionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerCallingConvention))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ManagedOrUnmanagedKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UnmanagedCallingConventionList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerParameter( FunctionPointerParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerParameter( node );
				default: 
					return base.VisitFunctionPointerParameter( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerParameter( FunctionPointerParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerParameterList( FunctionPointerParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerParameterList( node );
				default: 
					return base.VisitFunctionPointerParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerParameterList( FunctionPointerParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerType( FunctionPointerTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerType( node );
				default: 
					return base.VisitFunctionPointerType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerType( FunctionPointerTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AsteriskToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CallingConvention)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerUnmanagedCallingConvention( FunctionPointerUnmanagedCallingConventionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerUnmanagedCallingConvention( node );
				default: 
					return base.VisitFunctionPointerUnmanagedCallingConvention( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerUnmanagedCallingConvention( FunctionPointerUnmanagedCallingConventionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerUnmanagedCallingConvention))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitFunctionPointerUnmanagedCallingConventionList( FunctionPointerUnmanagedCallingConventionListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerUnmanagedCallingConventionList( node );
				default: 
					return base.VisitFunctionPointerUnmanagedCallingConventionList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformFunctionPointerUnmanagedCallingConventionList( FunctionPointerUnmanagedCallingConventionListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(FunctionPointerUnmanagedCallingConventionList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CallingConventions)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitGenericName( GenericNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformGenericName( node );
				default: 
					return base.VisitGenericName( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformGenericName( GenericNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(GenericName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitGlobalStatement( GlobalStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformGlobalStatement( node );
				default: 
					return base.VisitGlobalStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformGlobalStatement( GlobalStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(GlobalStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitGotoStatement( GotoStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformGotoStatement( node );
				default: 
					return base.VisitGotoStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformGotoStatement( GotoStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(GotoStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GotoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CaseOrDefaultKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitGroupClause( GroupClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformGroupClause( node );
				default: 
					return base.VisitGroupClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformGroupClause( GroupClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(GroupClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.GroupKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GroupExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ByKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ByExpression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCheckedExpression( CheckedExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCheckedExpression( node );
				default: 
					return base.VisitCheckedExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCheckedExpression( CheckedExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CheckedExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitCheckedStatement( CheckedStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformCheckedStatement( node );
				default: 
					return base.VisitCheckedStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformCheckedStatement( CheckedStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(CheckedStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIdentifierName( node );
				default: 
					return base.VisitIdentifierName( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIdentifierName( IdentifierNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IdentifierName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIfDirectiveTrivia( node );
				default: 
					return base.VisitIfDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIfDirectiveTrivia( IfDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IfDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConditionValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIfStatement( IfStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIfStatement( node );
				default: 
					return base.VisitIfStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIfStatement( IfStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IfStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Else)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitImplicitArrayCreationExpression( ImplicitArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformImplicitArrayCreationExpression( node );
				default: 
					return base.VisitImplicitArrayCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformImplicitArrayCreationExpression( ImplicitArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ImplicitArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Commas)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitImplicitElementAccess( ImplicitElementAccessSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformImplicitElementAccess( node );
				default: 
					return base.VisitImplicitElementAccess( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformImplicitElementAccess( ImplicitElementAccessSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ImplicitElementAccess))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformImplicitObjectCreationExpression( node );
				default: 
					return base.VisitImplicitObjectCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ImplicitObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitImplicitStackAllocArrayCreationExpression( ImplicitStackAllocArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformImplicitStackAllocArrayCreationExpression( node );
				default: 
					return base.VisitImplicitStackAllocArrayCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformImplicitStackAllocArrayCreationExpression( ImplicitStackAllocArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ImplicitStackAllocArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StackAllocKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIncompleteMember( IncompleteMemberSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIncompleteMember( node );
				default: 
					return base.VisitIncompleteMember( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIncompleteMember( IncompleteMemberSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IncompleteMember))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIndexerDeclaration( IndexerDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIndexerDeclaration( node );
				default: 
					return base.VisitIndexerDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIndexerDeclaration( IndexerDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IndexerDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ThisKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIndexerMemberCref( IndexerMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIndexerMemberCref( node );
				default: 
					return base.VisitIndexerMemberCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIndexerMemberCref( IndexerMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IndexerMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ThisKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInitializerExpression( InitializerExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInitializerExpression( node );
				default: 
					return base.VisitInitializerExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInitializerExpression( InitializerExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InitializerExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expressions)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterfaceDeclaration( node );
				default: 
					return base.VisitInterfaceDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterfaceDeclaration( InterfaceDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InterfaceDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterpolatedStringExpression( InterpolatedStringExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterpolatedStringExpression( node );
				default: 
					return base.VisitInterpolatedStringExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterpolatedStringExpression( InterpolatedStringExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InterpolatedStringExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StringStartToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Contents)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.StringEndToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterpolatedStringText( InterpolatedStringTextSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterpolatedStringText( node );
				default: 
					return base.VisitInterpolatedStringText( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterpolatedStringText( InterpolatedStringTextSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InterpolatedStringText))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.TextToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterpolation( InterpolationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterpolation( node );
				default: 
					return base.VisitInterpolation( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterpolation( InterpolationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Interpolation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AlignmentClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.FormatClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterpolationAlignmentClause( InterpolationAlignmentClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterpolationAlignmentClause( node );
				default: 
					return base.VisitInterpolationAlignmentClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterpolationAlignmentClause( InterpolationAlignmentClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InterpolationAlignmentClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.CommaToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInterpolationFormatClause( InterpolationFormatClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInterpolationFormatClause( node );
				default: 
					return base.VisitInterpolationFormatClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInterpolationFormatClause( InterpolationFormatClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InterpolationFormatClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.FormatStringToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitInvocationExpression( InvocationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformInvocationExpression( node );
				default: 
					return base.VisitInvocationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformInvocationExpression( InvocationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(InvocationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitIsPatternExpression( IsPatternExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformIsPatternExpression( node );
				default: 
					return base.VisitIsPatternExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformIsPatternExpression( IsPatternExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(IsPatternExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitJoinClause( JoinClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformJoinClause( node );
				default: 
					return base.VisitJoinClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformJoinClause( JoinClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(JoinClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.JoinKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.InExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OnKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LeftExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.RightExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Into)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitJoinIntoClause( JoinIntoClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformJoinIntoClause( node );
				default: 
					return base.VisitJoinIntoClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformJoinIntoClause( JoinIntoClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(JoinIntoClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.IntoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLabeledStatement( LabeledStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLabeledStatement( node );
				default: 
					return base.VisitLabeledStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLabeledStatement( LabeledStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LabeledStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLetClause( LetClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLetClause( node );
				default: 
					return base.VisitLetClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLetClause( LetClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LetClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LetKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLineDirectivePosition( LineDirectivePositionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLineDirectivePosition( node );
				default: 
					return base.VisitLineDirectivePosition( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLineDirectivePosition( LineDirectivePositionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LineDirectivePosition))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Line)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CommaToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Character)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLineDirectiveTrivia( LineDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLineDirectiveTrivia( node );
				default: 
					return base.VisitLineDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLineDirectiveTrivia( LineDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LineDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LineKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Line)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLineSpanDirectiveTrivia( LineSpanDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLineSpanDirectiveTrivia( node );
				default: 
					return base.VisitLineSpanDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLineSpanDirectiveTrivia( LineSpanDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LineSpanDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LineKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Start)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.MinusToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.End)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CharacterOffset)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLiteralExpression( LiteralExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLiteralExpression( node );
				default: 
					return base.VisitLiteralExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLiteralExpression( LiteralExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LiteralExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLoadDirectiveTrivia( LoadDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLoadDirectiveTrivia( node );
				default: 
					return base.VisitLoadDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLoadDirectiveTrivia( LoadDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LoadDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LoadKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLocalDeclarationStatement( node );
				default: 
					return base.VisitLocalDeclarationStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLocalDeclarationStatement( LocalDeclarationStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LocalDeclarationStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLocalFunctionStatement( node );
				default: 
					return base.VisitLocalFunctionStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLocalFunctionStatement( LocalFunctionStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LocalFunctionStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitLockStatement( LockStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformLockStatement( node );
				default: 
					return base.VisitLockStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformLockStatement( LockStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(LockStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LockKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitMakeRefExpression( MakeRefExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformMakeRefExpression( node );
				default: 
					return base.VisitMakeRefExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformMakeRefExpression( MakeRefExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(MakeRefExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformMemberAccessExpression( node );
				default: 
					return base.VisitMemberAccessExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformMemberAccessExpression( MemberAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(MemberAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformMemberBindingExpression( node );
				default: 
					return base.VisitMemberBindingExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformMemberBindingExpression( MemberBindingExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(MemberBindingExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformMethodDeclaration( node );
				default: 
					return base.VisitMethodDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformMethodDeclaration( MethodDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(MethodDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNameColon( NameColonSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNameColon( node );
				default: 
					return base.VisitNameColon( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNameColon( NameColonSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NameColon))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNameEquals( NameEqualsSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNameEquals( node );
				default: 
					return base.VisitNameEquals( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNameEquals( NameEqualsSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NameEquals))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNameMemberCref( NameMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNameMemberCref( node );
				default: 
					return base.VisitNameMemberCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNameMemberCref( NameMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NameMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNamespaceDeclaration( node );
				default: 
					return base.VisitNamespaceDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNamespaceDeclaration( NamespaceDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NamespaceDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.NamespaceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Externs)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Usings)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNullableDirectiveTrivia( NullableDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNullableDirectiveTrivia( node );
				default: 
					return base.VisitNullableDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNullableDirectiveTrivia( NullableDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NullableDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.NullableKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SettingToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TargetToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitNullableType( NullableTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformNullableType( node );
				default: 
					return base.VisitNullableType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformNullableType( NullableTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(NullableType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformObjectCreationExpression( node );
				default: 
					return base.VisitObjectCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformObjectCreationExpression( ObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOmittedArraySizeExpression( OmittedArraySizeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOmittedArraySizeExpression( node );
				default: 
					return base.VisitOmittedArraySizeExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOmittedArraySizeExpression( OmittedArraySizeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(OmittedArraySizeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OmittedArraySizeExpressionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOmittedTypeArgument( OmittedTypeArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOmittedTypeArgument( node );
				default: 
					return base.VisitOmittedTypeArgument( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOmittedTypeArgument( OmittedTypeArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(OmittedTypeArgument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OmittedTypeArgumentToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOperatorDeclaration( OperatorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOperatorDeclaration( node );
				default: 
					return base.VisitOperatorDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOperatorDeclaration( OperatorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(OperatorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOperatorMemberCref( OperatorMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOperatorMemberCref( node );
				default: 
					return base.VisitOperatorMemberCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOperatorMemberCref( OperatorMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(OperatorMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOrderByClause( OrderByClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOrderByClause( node );
				default: 
					return base.VisitOrderByClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOrderByClause( OrderByClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(OrderByClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OrderByKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Orderings)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitOrdering( OrderingSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformOrdering( node );
				default: 
					return base.VisitOrdering( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformOrdering( OrderingSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Ordering))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AscendingOrDescendingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParameter( ParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParameter( node );
				default: 
					return base.VisitParameter( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParameter( ParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Parameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Default)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParameterList( ParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParameterList( node );
				default: 
					return base.VisitParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParameterList( ParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParenthesizedExpression( ParenthesizedExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParenthesizedExpression( node );
				default: 
					return base.VisitParenthesizedExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParenthesizedExpression( ParenthesizedExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ParenthesizedExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParenthesizedLambdaExpression( node );
				default: 
					return base.VisitParenthesizedLambdaExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ParenthesizedLambdaExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParenthesizedPattern( ParenthesizedPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParenthesizedPattern( node );
				default: 
					return base.VisitParenthesizedPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParenthesizedPattern( ParenthesizedPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ParenthesizedPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitParenthesizedVariableDesignation( ParenthesizedVariableDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformParenthesizedVariableDesignation( node );
				default: 
					return base.VisitParenthesizedVariableDesignation( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformParenthesizedVariableDesignation( ParenthesizedVariableDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ParenthesizedVariableDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Variables)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPointerType( PointerTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPointerType( node );
				default: 
					return base.VisitPointerType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPointerType( PointerTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PointerType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AsteriskToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPositionalPatternClause( PositionalPatternClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPositionalPatternClause( node );
				default: 
					return base.VisitPositionalPatternClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPositionalPatternClause( PositionalPatternClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PositionalPatternClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Subpatterns)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPostfixUnaryExpression( PostfixUnaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPostfixUnaryExpression( node );
				default: 
					return base.VisitPostfixUnaryExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPostfixUnaryExpression( PostfixUnaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PostfixUnaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Operand)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPragmaChecksumDirectiveTrivia( PragmaChecksumDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPragmaChecksumDirectiveTrivia( node );
				default: 
					return base.VisitPragmaChecksumDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPragmaChecksumDirectiveTrivia( PragmaChecksumDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PragmaChecksumDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.PragmaKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ChecksumKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Guid)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Bytes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPragmaWarningDirectiveTrivia( node );
				default: 
					return base.VisitPragmaWarningDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PragmaWarningDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.PragmaKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WarningKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DisableOrRestoreKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ErrorCodes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPredefinedType( PredefinedTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPredefinedType( node );
				default: 
					return base.VisitPredefinedType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPredefinedType( PredefinedTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PredefinedType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPrefixUnaryExpression( PrefixUnaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPrefixUnaryExpression( node );
				default: 
					return base.VisitPrefixUnaryExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPrefixUnaryExpression( PrefixUnaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PrefixUnaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Operand)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPrimaryConstructorBaseType( node );
				default: 
					return base.VisitPrimaryConstructorBaseType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PrimaryConstructorBaseType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPropertyDeclaration( node );
				default: 
					return base.VisitPropertyDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPropertyDeclaration( PropertyDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PropertyDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitPropertyPatternClause( PropertyPatternClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformPropertyPatternClause( node );
				default: 
					return base.VisitPropertyPatternClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformPropertyPatternClause( PropertyPatternClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(PropertyPatternClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Subpatterns)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitQualifiedCref( QualifiedCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformQualifiedCref( node );
				default: 
					return base.VisitQualifiedCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformQualifiedCref( QualifiedCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(QualifiedCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Container)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Member)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitQualifiedName( QualifiedNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformQualifiedName( node );
				default: 
					return base.VisitQualifiedName( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformQualifiedName( QualifiedNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(QualifiedName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitQueryBody( QueryBodySyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformQueryBody( node );
				default: 
					return base.VisitQueryBody( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformQueryBody( QueryBodySyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(QueryBody))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Clauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SelectOrGroup)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Continuation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitQueryContinuation( QueryContinuationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformQueryContinuation( node );
				default: 
					return base.VisitQueryContinuation( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformQueryContinuation( QueryContinuationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(QueryContinuation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.IntoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitQueryExpression( QueryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformQueryExpression( node );
				default: 
					return base.VisitQueryExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformQueryExpression( QueryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(QueryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FromClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRangeExpression( RangeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRangeExpression( node );
				default: 
					return base.VisitRangeExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRangeExpression( RangeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RangeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LeftOperand)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.RightOperand)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRecordDeclaration( node );
				default: 
					return base.VisitRecordDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRecordDeclaration( RecordDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RecordDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ClassOrStructKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRecursivePattern( RecursivePatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRecursivePattern( node );
				default: 
					return base.VisitRecursivePattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRecursivePattern( RecursivePatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RecursivePattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.PositionalPatternClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.PropertyPatternClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitReferenceDirectiveTrivia( ReferenceDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformReferenceDirectiveTrivia( node );
				default: 
					return base.VisitReferenceDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformReferenceDirectiveTrivia( ReferenceDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ReferenceDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReferenceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRefExpression( RefExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRefExpression( node );
				default: 
					return base.VisitRefExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRefExpression( RefExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RefExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRefType( RefTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRefType( node );
				default: 
					return base.VisitRefType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRefType( RefTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RefType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReadOnlyKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRefTypeExpression( RefTypeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRefTypeExpression( node );
				default: 
					return base.VisitRefTypeExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRefTypeExpression( RefTypeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RefTypeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRefValueExpression( RefValueExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRefValueExpression( node );
				default: 
					return base.VisitRefValueExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRefValueExpression( RefValueExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RefValueExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Comma)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRegionDirectiveTrivia( RegionDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRegionDirectiveTrivia( node );
				default: 
					return base.VisitRegionDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRegionDirectiveTrivia( RegionDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RegionDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.RegionKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitRelationalPattern( RelationalPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformRelationalPattern( node );
				default: 
					return base.VisitRelationalPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformRelationalPattern( RelationalPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(RelationalPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformReturnStatement( node );
				default: 
					return base.VisitReturnStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformReturnStatement( ReturnStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ReturnStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSelectClause( SelectClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSelectClause( node );
				default: 
					return base.VisitSelectClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSelectClause( SelectClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SelectClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.SelectKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitShebangDirectiveTrivia( ShebangDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformShebangDirectiveTrivia( node );
				default: 
					return base.VisitShebangDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformShebangDirectiveTrivia( ShebangDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ShebangDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExclamationToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSimpleBaseType( SimpleBaseTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSimpleBaseType( node );
				default: 
					return base.VisitSimpleBaseType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSimpleBaseType( SimpleBaseTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SimpleBaseType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSimpleLambdaExpression( node );
				default: 
					return base.VisitSimpleLambdaExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSimpleLambdaExpression( SimpleLambdaExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SimpleLambdaExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameter)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSingleVariableDesignation( SingleVariableDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSingleVariableDesignation( node );
				default: 
					return base.VisitSingleVariableDesignation( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSingleVariableDesignation( SingleVariableDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SingleVariableDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSizeOfExpression( SizeOfExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSizeOfExpression( node );
				default: 
					return base.VisitSizeOfExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSizeOfExpression( SizeOfExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SizeOfExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSkippedTokensTrivia( SkippedTokensTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSkippedTokensTrivia( node );
				default: 
					return base.VisitSkippedTokensTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSkippedTokensTrivia( SkippedTokensTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SkippedTokensTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Tokens)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitStackAllocArrayCreationExpression( StackAllocArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformStackAllocArrayCreationExpression( node );
				default: 
					return base.VisitStackAllocArrayCreationExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformStackAllocArrayCreationExpression( StackAllocArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(StackAllocArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StackAllocKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformStructDeclaration( node );
				default: 
					return base.VisitStructDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformStructDeclaration( StructDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(StructDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSubpattern( SubpatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSubpattern( node );
				default: 
					return base.VisitSubpattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSubpattern( SubpatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(Subpattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ExpressionColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSwitchExpression( SwitchExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSwitchExpression( node );
				default: 
					return base.VisitSwitchExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSwitchExpression( SwitchExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SwitchExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.GoverningExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SwitchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arms)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSwitchExpressionArm( SwitchExpressionArmSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSwitchExpressionArm( node );
				default: 
					return base.VisitSwitchExpressionArm( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSwitchExpressionArm( SwitchExpressionArmSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SwitchExpressionArm))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhenClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSwitchSection( SwitchSectionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSwitchSection( node );
				default: 
					return base.VisitSwitchSection( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSwitchSection( SwitchSectionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SwitchSection))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Labels)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statements)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitSwitchStatement( SwitchStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformSwitchStatement( node );
				default: 
					return base.VisitSwitchStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformSwitchStatement( SwitchStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(SwitchStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SwitchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Sections)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitThisExpression( ThisExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformThisExpression( node );
				default: 
					return base.VisitThisExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformThisExpression( ThisExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ThisExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitThrowExpression( ThrowExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformThrowExpression( node );
				default: 
					return base.VisitThrowExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformThrowExpression( ThrowExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ThrowExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ThrowKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitThrowStatement( ThrowStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformThrowStatement( node );
				default: 
					return base.VisitThrowStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformThrowStatement( ThrowStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(ThrowStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ThrowKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTryStatement( TryStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTryStatement( node );
				default: 
					return base.VisitTryStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTryStatement( TryStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TryStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TryKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Catches)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Finally)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTupleElement( TupleElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTupleElement( node );
				default: 
					return base.VisitTupleElement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTupleElement( TupleElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TupleElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTupleExpression( TupleExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTupleExpression( node );
				default: 
					return base.VisitTupleExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTupleExpression( TupleExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TupleExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTupleType( TupleTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTupleType( node );
				default: 
					return base.VisitTupleType( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTupleType( TupleTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TupleType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Elements)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeArgumentList( TypeArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeArgumentList( node );
				default: 
					return base.VisitTypeArgumentList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeArgumentList( TypeArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeConstraint( TypeConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeConstraint( node );
				default: 
					return base.VisitTypeConstraint( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeConstraint( TypeConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeCref( TypeCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeCref( node );
				default: 
					return base.VisitTypeCref( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeCref( TypeCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeOfExpression( TypeOfExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeOfExpression( node );
				default: 
					return base.VisitTypeOfExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeOfExpression( TypeOfExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeOfExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeParameter( TypeParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeParameter( node );
				default: 
					return base.VisitTypeParameter( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeParameter( TypeParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.VarianceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeParameterConstraintClause( TypeParameterConstraintClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeParameterConstraintClause( node );
				default: 
					return base.VisitTypeParameterConstraintClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeParameterConstraintClause( TypeParameterConstraintClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeParameterConstraintClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhereKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Constraints)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypeParameterList( TypeParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypeParameterList( node );
				default: 
					return base.VisitTypeParameterList( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypeParameterList( TypeParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypeParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitTypePattern( TypePatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformTypePattern( node );
				default: 
					return base.VisitTypePattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformTypePattern( TypePatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(TypePattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitUnaryPattern( UnaryPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformUnaryPattern( node );
				default: 
					return base.VisitUnaryPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformUnaryPattern( UnaryPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(UnaryPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitUndefDirectiveTrivia( UndefDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformUndefDirectiveTrivia( node );
				default: 
					return base.VisitUndefDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformUndefDirectiveTrivia( UndefDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(UndefDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UndefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitUnsafeStatement( UnsafeStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformUnsafeStatement( node );
				default: 
					return base.VisitUnsafeStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformUnsafeStatement( UnsafeStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(UnsafeStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UnsafeKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitUsingDirective( UsingDirectiveSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformUsingDirective( node );
				default: 
					return base.VisitUsingDirective( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformUsingDirective( UsingDirectiveSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(UsingDirective))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.GlobalKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.StaticKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Alias)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitUsingStatement( UsingStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformUsingStatement( node );
				default: 
					return base.VisitUsingStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformUsingStatement( UsingStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(UsingStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitVariableDeclaration( VariableDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformVariableDeclaration( node );
				default: 
					return base.VisitVariableDeclaration( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformVariableDeclaration( VariableDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(VariableDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Variables)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitVariableDeclarator( VariableDeclaratorSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformVariableDeclarator( node );
				default: 
					return base.VisitVariableDeclarator( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformVariableDeclarator( VariableDeclaratorSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(VariableDeclarator))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitVarPattern( VarPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformVarPattern( node );
				default: 
					return base.VisitVarPattern( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformVarPattern( VarPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(VarPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.VarKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitWarningDirectiveTrivia( WarningDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformWarningDirectiveTrivia( node );
				default: 
					return base.VisitWarningDirectiveTrivia( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformWarningDirectiveTrivia( WarningDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(WarningDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WarningKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitWhenClause( WhenClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformWhenClause( node );
				default: 
					return base.VisitWhenClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformWhenClause( WhenClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(WhenClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhenKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitWhereClause( WhereClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformWhereClause( node );
				default: 
					return base.VisitWhereClause( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformWhereClause( WhereClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(WhereClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhereKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitWhileStatement( WhileStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformWhileStatement( node );
				default: 
					return base.VisitWhileStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformWhileStatement( WhileStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(WhileStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WhileKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitWithExpression( WithExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformWithExpression( node );
				default: 
					return base.VisitWithExpression( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformWithExpression( WithExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(WithExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.WithKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlCDataSection( XmlCDataSectionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlCDataSection( node );
				default: 
					return base.VisitXmlCDataSection( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlCDataSection( XmlCDataSectionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlCDataSection))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartCDataToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndCDataToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlComment( XmlCommentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlComment( node );
				default: 
					return base.VisitXmlComment( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlComment( XmlCommentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlComment))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanExclamationMinusMinusToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.MinusMinusGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlCrefAttribute( XmlCrefAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlCrefAttribute( node );
				default: 
					return base.VisitXmlCrefAttribute( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlCrefAttribute( XmlCrefAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlCrefAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Cref)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlElement( XmlElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlElement( node );
				default: 
					return base.VisitXmlElement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlElement( XmlElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartTag)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Content)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndTag)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlElementEndTag( XmlElementEndTagSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlElementEndTag( node );
				default: 
					return base.VisitXmlElementEndTag( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlElementEndTag( XmlElementEndTagSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlElementEndTag))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanSlashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlElementStartTag( XmlElementStartTagSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlElementStartTag( node );
				default: 
					return base.VisitXmlElementStartTag( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlElementStartTag( XmlElementStartTagSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlElementStartTag))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlEmptyElement( XmlEmptyElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlEmptyElement( node );
				default: 
					return base.VisitXmlEmptyElement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlEmptyElement( XmlEmptyElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlEmptyElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SlashGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlName( XmlNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlName( node );
				default: 
					return base.VisitXmlName( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlName( XmlNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Prefix)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.LocalName)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlNameAttribute( XmlNameAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlNameAttribute( node );
				default: 
					return base.VisitXmlNameAttribute( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlNameAttribute( XmlNameAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlNameAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlPrefix( XmlPrefixSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlPrefix( node );
				default: 
					return base.VisitXmlPrefix( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlPrefix( XmlPrefixSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlPrefix))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Prefix)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlProcessingInstruction( XmlProcessingInstructionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlProcessingInstruction( node );
				default: 
					return base.VisitXmlProcessingInstruction( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlProcessingInstruction( XmlProcessingInstructionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlProcessingInstruction))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartProcessingInstructionToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndProcessingInstructionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlText( XmlTextSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlText( node );
				default: 
					return base.VisitXmlText( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlText( XmlTextSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlText))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitXmlTextAttribute( XmlTextAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformXmlTextAttribute( node );
				default: 
					return base.VisitXmlTextAttribute( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformXmlTextAttribute( XmlTextAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(XmlTextAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		[ExcludeFromCodeCoverage]
		public override SyntaxNode VisitYieldStatement( YieldStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Transform: 
					return this.TransformYieldStatement( node );
				default: 
					return base.VisitYieldStatement( node );
			}
		}
		[ExcludeFromCodeCoverage]
		protected virtual ExpressionSyntax TransformYieldStatement( YieldStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(this.MetaSyntaxFactory.SyntaxFactoryMethod(nameof(YieldStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.YieldKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.ReturnOrBreakKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
	partial class MetaSyntaxFactoryImpl
	{
		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration(ExpressionSyntax @kind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration1(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration2(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration1(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration2(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorDeclaration(ExpressionSyntax @kind, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorList(ExpressionSyntax @openBraceToken, ExpressionSyntax @accessors, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @accessors ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AccessorList(ExpressionSyntax @accessors)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AccessorList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @accessors )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AliasQualifiedName(ExpressionSyntax @alias, ExpressionSyntax @colonColonToken, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AliasQualifiedName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @colonColonToken ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AliasQualifiedName(ExpressionSyntax @alias, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AliasQualifiedName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression1(ExpressionSyntax @asyncKeyword, ExpressionSyntax @delegateKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @delegateKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression2(ExpressionSyntax @modifiers, ExpressionSyntax @delegateKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @delegateKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression(ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression(ExpressionSyntax @parameterList, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression(ExpressionSyntax @asyncKeyword, ExpressionSyntax @delegateKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @delegateKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousMethodExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousMethodExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousObjectCreationExpression(ExpressionSyntax @newKeyword, ExpressionSyntax @openBraceToken, ExpressionSyntax @initializers, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @initializers ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousObjectCreationExpression(ExpressionSyntax @initializers)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @initializers )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousObjectMemberDeclarator(ExpressionSyntax @nameEquals, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousObjectMemberDeclarator" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @nameEquals ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AnonymousObjectMemberDeclarator(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AnonymousObjectMemberDeclarator" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent1(ExpressionSyntax @oldNode, ExpressionSyntax @newNode, ExpressionSyntax @topLevel)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AreEquivalent" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldNode ), 
				SyntaxFactory.Argument( @newNode ), 
				SyntaxFactory.Argument( @topLevel )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent2(ExpressionSyntax @oldNode, ExpressionSyntax @newNode, ExpressionSyntax @ignoreChildNode)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AreEquivalent" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldNode ), 
				SyntaxFactory.Argument( @newNode ), 
				SyntaxFactory.Argument( @ignoreChildNode )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent3(ExpressionSyntax @oldTree, ExpressionSyntax @newTree, ExpressionSyntax @topLevel)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AreEquivalent" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldTree ), 
				SyntaxFactory.Argument( @newTree ), 
				SyntaxFactory.Argument( @topLevel )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent4<TNode>(ExpressionSyntax @oldList, ExpressionSyntax @newList, ExpressionSyntax @topLevel)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "AreEquivalent", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldList ), 
				SyntaxFactory.Argument( @newList ), 
				SyntaxFactory.Argument( @topLevel )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent5<TNode>(ExpressionSyntax @oldList, ExpressionSyntax @newList, ExpressionSyntax @ignoreChildNode)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "AreEquivalent", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldList ), 
				SyntaxFactory.Argument( @newList ), 
				SyntaxFactory.Argument( @ignoreChildNode )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent1(ExpressionSyntax @oldToken, ExpressionSyntax @newToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AreEquivalent" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldToken ), 
				SyntaxFactory.Argument( @newToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AreEquivalent2(ExpressionSyntax @oldList, ExpressionSyntax @newList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AreEquivalent" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @oldList ), 
				SyntaxFactory.Argument( @newList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Argument(ExpressionSyntax @nameColon, ExpressionSyntax @refKindKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Argument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @nameColon ), 
				SyntaxFactory.Argument( @refKindKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Argument(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Argument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArgumentList(ExpressionSyntax @openParenToken, ExpressionSyntax @arguments, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @arguments ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArgumentList(ExpressionSyntax @arguments)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arguments )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayCreationExpression(ExpressionSyntax @newKeyword, ExpressionSyntax @type, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayCreationExpression(ExpressionSyntax @type, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayCreationExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayRankSpecifier(ExpressionSyntax @openBracketToken, ExpressionSyntax @sizes, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayRankSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @sizes ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayRankSpecifier(ExpressionSyntax @sizes)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayRankSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @sizes )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayType(ExpressionSyntax @elementType, ExpressionSyntax @rankSpecifiers)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType ), 
				SyntaxFactory.Argument( @rankSpecifiers )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrayType(ExpressionSyntax @elementType)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrayType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrowExpressionClause(ExpressionSyntax @arrowToken, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrowExpressionClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ArrowExpressionClause(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ArrowExpressionClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AssignmentExpression(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @operatorToken, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AssignmentExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AssignmentExpression(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AssignmentExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Attribute(ExpressionSyntax @name, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Attribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Attribute(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Attribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeArgument(ExpressionSyntax @nameEquals, ExpressionSyntax @nameColon, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeArgument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @nameEquals ), 
				SyntaxFactory.Argument( @nameColon ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeArgument(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeArgument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeArgumentList(ExpressionSyntax @openParenToken, ExpressionSyntax @arguments, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @arguments ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeArgumentList(ExpressionSyntax @arguments)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arguments )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeList(ExpressionSyntax @openBracketToken, ExpressionSyntax @target, ExpressionSyntax @attributes, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @target ), 
				SyntaxFactory.Argument( @attributes ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeList(ExpressionSyntax @target, ExpressionSyntax @attributes)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @target ), 
				SyntaxFactory.Argument( @attributes )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeList(ExpressionSyntax @attributes)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributes )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeTargetSpecifier(ExpressionSyntax @identifier, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeTargetSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AttributeTargetSpecifier(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AttributeTargetSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AwaitExpression(ExpressionSyntax @awaitKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AwaitExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax AwaitExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "AwaitExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BadDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @identifier, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BadDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BadDirectiveTrivia(ExpressionSyntax @identifier, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BadDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BadToken(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BadToken" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BaseExpression(ExpressionSyntax @token)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BaseExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @token )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BaseExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BaseExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BaseList(ExpressionSyntax @colonToken, ExpressionSyntax @types)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BaseList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @types )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BaseList(ExpressionSyntax @types)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BaseList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @types )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BinaryExpression(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @operatorToken, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BinaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BinaryExpression(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BinaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BinaryPattern(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @operatorToken, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BinaryPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BinaryPattern(ExpressionSyntax @kind, ExpressionSyntax @left, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BinaryPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Block(ExpressionSyntax @attributeLists, ExpressionSyntax @openBraceToken, ExpressionSyntax @statements, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Block" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @statements ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Block(ExpressionSyntax @attributeLists, ExpressionSyntax @statements)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Block" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @statements )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Block(params ExpressionSyntax[] @statements)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Block" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @statements.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Block(ExpressionSyntax @openBraceToken, ExpressionSyntax @statements, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Block" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @statements ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BracketedArgumentList(ExpressionSyntax @openBracketToken, ExpressionSyntax @arguments, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BracketedArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @arguments ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BracketedArgumentList(ExpressionSyntax @arguments)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BracketedArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arguments )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BracketedParameterList(ExpressionSyntax @openBracketToken, ExpressionSyntax @parameters, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BracketedParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BracketedParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BracketedParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BreakStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @breakKeyword, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BreakStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @breakKeyword ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BreakStatement(ExpressionSyntax @attributeLists)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BreakStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BreakStatement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BreakStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax BreakStatement(ExpressionSyntax @breakKeyword, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "BreakStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @breakKeyword ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CasePatternSwitchLabel(ExpressionSyntax @keyword, ExpressionSyntax @pattern, ExpressionSyntax @whenClause, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CasePatternSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @whenClause ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CasePatternSwitchLabel(ExpressionSyntax @pattern, ExpressionSyntax @whenClause, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CasePatternSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @whenClause ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CasePatternSwitchLabel(ExpressionSyntax @pattern, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CasePatternSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CaseSwitchLabel(ExpressionSyntax @keyword, ExpressionSyntax @value, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CaseSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CaseSwitchLabel(ExpressionSyntax @value, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CaseSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CaseSwitchLabel(ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CaseSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CastExpression(ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @closeParenToken, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CastExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CastExpression(ExpressionSyntax @type, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CastExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchClause(ExpressionSyntax @catchKeyword, ExpressionSyntax @declaration, ExpressionSyntax @filter, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @catchKeyword ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @filter ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchClause(ExpressionSyntax @declaration, ExpressionSyntax @filter, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @filter ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchClause()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchDeclaration(ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchDeclaration(ExpressionSyntax @type, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchDeclaration(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchFilterClause(ExpressionSyntax @whenKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @filterExpression, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchFilterClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @whenKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @filterExpression ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CatchFilterClause(ExpressionSyntax @filterExpression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CatchFilterClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @filterExpression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassOrStructConstraint(ExpressionSyntax @kind, ExpressionSyntax @classOrStructKeyword, ExpressionSyntax @questionToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassOrStructConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @classOrStructKeyword ), 
				SyntaxFactory.Argument( @questionToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassOrStructConstraint(ExpressionSyntax @kind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassOrStructConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ClassOrStructConstraint(ExpressionSyntax @kind, ExpressionSyntax @classOrStructKeyword)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ClassOrStructConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @classOrStructKeyword )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Comment(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Comment" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CompilationUnit(ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @attributeLists, ExpressionSyntax @members, ExpressionSyntax @endOfFileToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CompilationUnit" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @endOfFileToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CompilationUnit(ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @attributeLists, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CompilationUnit" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CompilationUnit()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CompilationUnit" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConditionalAccessExpression(ExpressionSyntax @expression, ExpressionSyntax @operatorToken, ExpressionSyntax @whenNotNull)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConditionalAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @whenNotNull )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConditionalAccessExpression(ExpressionSyntax @expression, ExpressionSyntax @whenNotNull)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConditionalAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @whenNotNull )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConditionalExpression(ExpressionSyntax @condition, ExpressionSyntax @questionToken, ExpressionSyntax @whenTrue, ExpressionSyntax @colonToken, ExpressionSyntax @whenFalse)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConditionalExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @questionToken ), 
				SyntaxFactory.Argument( @whenTrue ), 
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @whenFalse )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConditionalExpression(ExpressionSyntax @condition, ExpressionSyntax @whenTrue, ExpressionSyntax @whenFalse)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConditionalExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @whenTrue ), 
				SyntaxFactory.Argument( @whenFalse )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstantPattern(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstantPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorConstraint(ExpressionSyntax @newKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorConstraint()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration3(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @initializer, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorInitializer(ExpressionSyntax @kind, ExpressionSyntax @colonToken, ExpressionSyntax @thisOrBaseKeyword, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorInitializer" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @thisOrBaseKeyword ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConstructorInitializer(ExpressionSyntax @kind, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConstructorInitializer" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ContinueStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @continueKeyword, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ContinueStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @continueKeyword ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ContinueStatement(ExpressionSyntax @attributeLists)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ContinueStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ContinueStatement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ContinueStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ContinueStatement(ExpressionSyntax @continueKeyword, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ContinueStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @continueKeyword ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @operatorKeyword, ExpressionSyntax @type, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @type, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @operatorKeyword, ExpressionSyntax @type, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration(ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @operatorKeyword, ExpressionSyntax @type, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @type, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorMemberCref(ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @operatorKeyword, ExpressionSyntax @type, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorMemberCref(ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @type, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ConversionOperatorMemberCref(ExpressionSyntax @implicitOrExplicitKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ConversionOperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @implicitOrExplicitKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefBracketedParameterList(ExpressionSyntax @openBracketToken, ExpressionSyntax @parameters, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefBracketedParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefBracketedParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefBracketedParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefParameter(ExpressionSyntax @refKindKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @refKindKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefParameter(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefParameterList(ExpressionSyntax @openParenToken, ExpressionSyntax @parameters, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CrefParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CrefParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DeclarationExpression(ExpressionSyntax @type, ExpressionSyntax @designation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DeclarationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @designation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DeclarationPattern(ExpressionSyntax @type, ExpressionSyntax @designation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DeclarationPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @designation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultConstraint(ExpressionSyntax @defaultKeyword)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @defaultKeyword )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultConstraint()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultSwitchLabel(ExpressionSyntax @keyword, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultSwitchLabel(ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefaultSwitchLabel()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefaultSwitchLabel" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefineDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @defineKeyword, ExpressionSyntax @name, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefineDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @defineKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DefineDirectiveTrivia(ExpressionSyntax @name, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DefineDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DelegateDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @delegateKeyword, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DelegateDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @delegateKeyword ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DelegateDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DelegateDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DelegateDeclaration(ExpressionSyntax @returnType, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DelegateDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @tildeToken, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @tildeToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @tildeToken, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @tildeToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DestructorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @tildeToken, ExpressionSyntax @identifier, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DestructorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @tildeToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DisabledText(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DisabledText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DiscardDesignation(ExpressionSyntax @underscoreToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DiscardDesignation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @underscoreToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DiscardDesignation()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DiscardDesignation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DiscardPattern(ExpressionSyntax @underscoreToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DiscardPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @underscoreToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DiscardPattern()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DiscardPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DocumentationComment(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DocumentationComment" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DocumentationCommentExterior(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DocumentationCommentExterior" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DocumentationCommentTrivia(ExpressionSyntax @kind, ExpressionSyntax @content, ExpressionSyntax @endOfComment)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DocumentationCommentTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @content ), 
				SyntaxFactory.Argument( @endOfComment )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DocumentationCommentTrivia(ExpressionSyntax @kind, ExpressionSyntax @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DocumentationCommentTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @content )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DoStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @doKeyword, ExpressionSyntax @statement, ExpressionSyntax @whileKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @doKeyword ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @whileKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DoStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @statement, ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DoStatement(ExpressionSyntax @statement, ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax DoStatement(ExpressionSyntax @doKeyword, ExpressionSyntax @statement, ExpressionSyntax @whileKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "DoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @doKeyword ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @whileKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElasticEndOfLine(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElasticEndOfLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElasticWhitespace(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElasticWhitespace" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElementAccessExpression(ExpressionSyntax @expression, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElementAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElementAccessExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElementAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElementBindingExpression(ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElementBindingExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElementBindingExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElementBindingExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElifDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @elifKeyword, ExpressionSyntax @condition, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive, ExpressionSyntax @branchTaken, ExpressionSyntax @conditionValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElifDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @elifKeyword ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken ), 
				SyntaxFactory.Argument( @conditionValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElifDirectiveTrivia(ExpressionSyntax @condition, ExpressionSyntax @isActive, ExpressionSyntax @branchTaken, ExpressionSyntax @conditionValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElifDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken ), 
				SyntaxFactory.Argument( @conditionValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElseClause(ExpressionSyntax @elseKeyword, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElseClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elseKeyword ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElseClause(ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElseClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElseDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @elseKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive, ExpressionSyntax @branchTaken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElseDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @elseKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ElseDirectiveTrivia(ExpressionSyntax @isActive, ExpressionSyntax @branchTaken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ElseDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EmptyStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EmptyStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EmptyStatement1(ExpressionSyntax @attributeLists)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EmptyStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EmptyStatement2(ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EmptyStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EmptyStatement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EmptyStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndIfDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @endIfKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndIfDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @endIfKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndIfDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndIfDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndOfLine(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndOfLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndOfLine(ExpressionSyntax @text, ExpressionSyntax @elastic)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndOfLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @elastic )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndRegionDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @endRegionKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndRegionDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @endRegionKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EndRegionDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EndRegionDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @enumKeyword, ExpressionSyntax @identifier, ExpressionSyntax @baseList, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @enumKeyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @baseList, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumMemberDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @equalsValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumMemberDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @equalsValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumMemberDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumMemberDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EnumMemberDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @identifier, ExpressionSyntax @equalsValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EnumMemberDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @equalsValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EqualsValueClause(ExpressionSyntax @equalsToken, ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EqualsValueClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @equalsToken ), 
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EqualsValueClause(ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EqualsValueClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ErrorDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @errorKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ErrorDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @errorKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ErrorDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ErrorDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @eventKeyword, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @eventKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventDeclaration(ExpressionSyntax @type, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @eventKeyword, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @eventKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @eventKeyword, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @eventKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventFieldDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @eventKeyword, ExpressionSyntax @declaration, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventFieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @eventKeyword ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventFieldDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventFieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax EventFieldDeclaration(ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "EventFieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExplicitInterfaceSpecifier(ExpressionSyntax @name, ExpressionSyntax @dotToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExplicitInterfaceSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @dotToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExplicitInterfaceSpecifier(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExplicitInterfaceSpecifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExpressionColon(ExpressionSyntax @expression, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExpressionColon" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExpressionStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExpressionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExpressionStatement1(ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExpressionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExpressionStatement2(ExpressionSyntax @attributeLists, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExpressionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExpressionStatement(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExpressionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExternAliasDirective(ExpressionSyntax @externKeyword, ExpressionSyntax @aliasKeyword, ExpressionSyntax @identifier, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExternAliasDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @externKeyword ), 
				SyntaxFactory.Argument( @aliasKeyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ExternAliasDirective(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ExternAliasDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FieldDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @declaration, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FieldDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FieldDeclaration(ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FieldDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FileScopedNamespaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @namespaceKeyword, ExpressionSyntax @name, ExpressionSyntax @semicolonToken, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FileScopedNamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @namespaceKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @semicolonToken ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FileScopedNamespaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @name, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FileScopedNamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FileScopedNamespaceDeclaration(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FileScopedNamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FinallyClause(ExpressionSyntax @finallyKeyword, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FinallyClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @finallyKeyword ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FinallyClause(ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FinallyClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FixedStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @fixedKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FixedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @fixedKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FixedStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @declaration, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FixedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FixedStatement(ExpressionSyntax @declaration, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FixedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FixedStatement(ExpressionSyntax @fixedKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FixedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @fixedKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @awaitKeyword, ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachStatement(ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachStatement(ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachStatement(ExpressionSyntax @awaitKeyword, ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachVariableStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @awaitKeyword, ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @variable, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachVariableStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @variable ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachVariableStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @variable, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachVariableStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @variable ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachVariableStatement(ExpressionSyntax @variable, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachVariableStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @variable ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachVariableStatement(ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @variable, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachVariableStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @variable ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForEachVariableStatement(ExpressionSyntax @awaitKeyword, ExpressionSyntax @forEachKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @variable, ExpressionSyntax @inKeyword, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForEachVariableStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @forEachKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @variable ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @forKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @initializers, ExpressionSyntax @firstSemicolonToken, ExpressionSyntax @condition, ExpressionSyntax @secondSemicolonToken, ExpressionSyntax @incrementors, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @forKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @initializers ), 
				SyntaxFactory.Argument( @firstSemicolonToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @secondSemicolonToken ), 
				SyntaxFactory.Argument( @incrementors ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @declaration, ExpressionSyntax @initializers, ExpressionSyntax @condition, ExpressionSyntax @incrementors, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @initializers ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @incrementors ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForStatement(ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForStatement(ExpressionSyntax @declaration, ExpressionSyntax @initializers, ExpressionSyntax @condition, ExpressionSyntax @incrementors, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @initializers ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @incrementors ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ForStatement(ExpressionSyntax @forKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @initializers, ExpressionSyntax @firstSemicolonToken, ExpressionSyntax @condition, ExpressionSyntax @secondSemicolonToken, ExpressionSyntax @incrementors, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ForStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @forKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @initializers ), 
				SyntaxFactory.Argument( @firstSemicolonToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @secondSemicolonToken ), 
				SyntaxFactory.Argument( @incrementors ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FromClause(ExpressionSyntax @fromKeyword, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FromClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @fromKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FromClause(ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FromClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FromClause(ExpressionSyntax @identifier, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FromClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerCallingConvention(ExpressionSyntax @managedOrUnmanagedKeyword, ExpressionSyntax @unmanagedCallingConventionList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerCallingConvention" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @managedOrUnmanagedKeyword ), 
				SyntaxFactory.Argument( @unmanagedCallingConventionList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerCallingConvention(ExpressionSyntax @managedOrUnmanagedKeyword)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerCallingConvention" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @managedOrUnmanagedKeyword )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerParameter(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerParameter(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerParameterList(ExpressionSyntax @lessThanToken, ExpressionSyntax @parameters, ExpressionSyntax @greaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @greaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerType(ExpressionSyntax @delegateKeyword, ExpressionSyntax @asteriskToken, ExpressionSyntax @callingConvention, ExpressionSyntax @parameterList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @delegateKeyword ), 
				SyntaxFactory.Argument( @asteriskToken ), 
				SyntaxFactory.Argument( @callingConvention ), 
				SyntaxFactory.Argument( @parameterList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerType(ExpressionSyntax @callingConvention, ExpressionSyntax @parameterList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @callingConvention ), 
				SyntaxFactory.Argument( @parameterList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerType()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerUnmanagedCallingConvention(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerUnmanagedCallingConvention" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerUnmanagedCallingConventionList(ExpressionSyntax @openBracketToken, ExpressionSyntax @callingConventions, ExpressionSyntax @closeBracketToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerUnmanagedCallingConventionList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @callingConventions ), 
				SyntaxFactory.Argument( @closeBracketToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax FunctionPointerUnmanagedCallingConventionList(ExpressionSyntax @callingConventions)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "FunctionPointerUnmanagedCallingConventionList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @callingConventions )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GenericName(ExpressionSyntax @identifier, ExpressionSyntax @typeArgumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GenericName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeArgumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GenericName(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GenericName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_CarriageReturn()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_CarriageReturn" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_CarriageReturnLineFeed()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_CarriageReturnLineFeed" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticCarriageReturn()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticCarriageReturn" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticCarriageReturnLineFeed()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticCarriageReturnLineFeed" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticLineFeed()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticLineFeed" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticMarker()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticMarker" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticSpace()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticSpace" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_ElasticTab()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_ElasticTab" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_LineFeed()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_LineFeed" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_Space()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_Space" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax get_Tab()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "get_Tab" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GetNonGenericExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GetNonGenericExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GetStandaloneExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GetStandaloneExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GlobalStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GlobalStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GlobalStatement(ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GlobalStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GotoStatement(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @gotoKeyword, ExpressionSyntax @caseOrDefaultKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GotoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @gotoKeyword ), 
				SyntaxFactory.Argument( @caseOrDefaultKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GotoStatement(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @caseOrDefaultKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GotoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @caseOrDefaultKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GotoStatement(ExpressionSyntax @kind, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GotoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GotoStatement(ExpressionSyntax @kind, ExpressionSyntax @caseOrDefaultKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GotoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @caseOrDefaultKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GotoStatement(ExpressionSyntax @kind, ExpressionSyntax @gotoKeyword, ExpressionSyntax @caseOrDefaultKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GotoStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @gotoKeyword ), 
				SyntaxFactory.Argument( @caseOrDefaultKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GroupClause(ExpressionSyntax @groupKeyword, ExpressionSyntax @groupExpression, ExpressionSyntax @byKeyword, ExpressionSyntax @byExpression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GroupClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @groupKeyword ), 
				SyntaxFactory.Argument( @groupExpression ), 
				SyntaxFactory.Argument( @byKeyword ), 
				SyntaxFactory.Argument( @byExpression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax GroupClause(ExpressionSyntax @groupExpression, ExpressionSyntax @byExpression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "GroupClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @groupExpression ), 
				SyntaxFactory.Argument( @byExpression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedExpression(ExpressionSyntax @kind, ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedExpression(ExpressionSyntax @kind, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedStatement(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @keyword, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedStatement1(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedStatement2(ExpressionSyntax @kind, ExpressionSyntax @keyword, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax CheckedStatement(ExpressionSyntax @kind, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "CheckedStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Identifier(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Identifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Identifier(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Identifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Identifier(ExpressionSyntax @leading, ExpressionSyntax @contextualKind, ExpressionSyntax @text, ExpressionSyntax @valueText, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Identifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @contextualKind ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @valueText ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IdentifierName1(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IdentifierName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IdentifierName2(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IdentifierName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @ifKeyword, ExpressionSyntax @condition, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive, ExpressionSyntax @branchTaken, ExpressionSyntax @conditionValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @ifKeyword ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken ), 
				SyntaxFactory.Argument( @conditionValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfDirectiveTrivia(ExpressionSyntax @condition, ExpressionSyntax @isActive, ExpressionSyntax @branchTaken, ExpressionSyntax @conditionValue)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @isActive ), 
				SyntaxFactory.Argument( @branchTaken ), 
				SyntaxFactory.Argument( @conditionValue )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @ifKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement, ExpressionSyntax @else)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @ifKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @else )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @condition, ExpressionSyntax @statement, ExpressionSyntax @else)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @else )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfStatement(ExpressionSyntax @condition, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfStatement(ExpressionSyntax @condition, ExpressionSyntax @statement, ExpressionSyntax @else)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @else )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IfStatement(ExpressionSyntax @ifKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement, ExpressionSyntax @else)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IfStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @ifKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement ), 
				SyntaxFactory.Argument( @else )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitArrayCreationExpression(ExpressionSyntax @newKeyword, ExpressionSyntax @openBracketToken, ExpressionSyntax @commas, ExpressionSyntax @closeBracketToken, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @commas ), 
				SyntaxFactory.Argument( @closeBracketToken ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitArrayCreationExpression(ExpressionSyntax @commas, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @commas ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitArrayCreationExpression(ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitElementAccess(ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitElementAccess" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitElementAccess()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitElementAccess" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitObjectCreationExpression(ExpressionSyntax @newKeyword, ExpressionSyntax @argumentList, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @argumentList ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitObjectCreationExpression(ExpressionSyntax @argumentList, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @argumentList ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitObjectCreationExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitStackAllocArrayCreationExpression(ExpressionSyntax @stackAllocKeyword, ExpressionSyntax @openBracketToken, ExpressionSyntax @closeBracketToken, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitStackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stackAllocKeyword ), 
				SyntaxFactory.Argument( @openBracketToken ), 
				SyntaxFactory.Argument( @closeBracketToken ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ImplicitStackAllocArrayCreationExpression(ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ImplicitStackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IncompleteMember(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IncompleteMember" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IncompleteMember(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IncompleteMember" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @thisKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @accessorList, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @thisKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @accessorList ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @parameterList, ExpressionSyntax @accessorList, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @accessorList ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerDeclaration(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @parameterList, ExpressionSyntax @accessorList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @accessorList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerMemberCref(ExpressionSyntax @thisKeyword, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @thisKeyword ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IndexerMemberCref(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IndexerMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InitializerExpression(ExpressionSyntax @kind, ExpressionSyntax @openBraceToken, ExpressionSyntax @expressions, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InitializerExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @expressions ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InitializerExpression(ExpressionSyntax @kind, ExpressionSyntax @expressions)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InitializerExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expressions )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterfaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterfaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterfaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterfaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterfaceDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterfaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolatedStringExpression(ExpressionSyntax @stringStartToken, ExpressionSyntax @contents, ExpressionSyntax @stringEndToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolatedStringExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stringStartToken ), 
				SyntaxFactory.Argument( @contents ), 
				SyntaxFactory.Argument( @stringEndToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolatedStringExpression(ExpressionSyntax @stringStartToken, ExpressionSyntax @contents)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolatedStringExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stringStartToken ), 
				SyntaxFactory.Argument( @contents )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolatedStringExpression(ExpressionSyntax @stringStartToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolatedStringExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stringStartToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolatedStringText(ExpressionSyntax @textToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolatedStringText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @textToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolatedStringText()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolatedStringText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Interpolation(ExpressionSyntax @openBraceToken, ExpressionSyntax @expression, ExpressionSyntax @alignmentClause, ExpressionSyntax @formatClause, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Interpolation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @alignmentClause ), 
				SyntaxFactory.Argument( @formatClause ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Interpolation(ExpressionSyntax @expression, ExpressionSyntax @alignmentClause, ExpressionSyntax @formatClause)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Interpolation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @alignmentClause ), 
				SyntaxFactory.Argument( @formatClause )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Interpolation(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Interpolation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolationAlignmentClause(ExpressionSyntax @commaToken, ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolationAlignmentClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @commaToken ), 
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolationFormatClause(ExpressionSyntax @colonToken, ExpressionSyntax @formatStringToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolationFormatClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @formatStringToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InterpolationFormatClause(ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InterpolationFormatClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InvocationExpression(ExpressionSyntax @expression, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InvocationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax InvocationExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "InvocationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IsCompleteSubmission(ExpressionSyntax @tree)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IsCompleteSubmission" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @tree )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IsPatternExpression(ExpressionSyntax @expression, ExpressionSyntax @isKeyword, ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IsPatternExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @isKeyword ), 
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax IsPatternExpression(ExpressionSyntax @expression, ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "IsPatternExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax JoinClause(ExpressionSyntax @joinKeyword, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inKeyword, ExpressionSyntax @inExpression, ExpressionSyntax @onKeyword, ExpressionSyntax @leftExpression, ExpressionSyntax @equalsKeyword, ExpressionSyntax @rightExpression, ExpressionSyntax @into)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "JoinClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @joinKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inKeyword ), 
				SyntaxFactory.Argument( @inExpression ), 
				SyntaxFactory.Argument( @onKeyword ), 
				SyntaxFactory.Argument( @leftExpression ), 
				SyntaxFactory.Argument( @equalsKeyword ), 
				SyntaxFactory.Argument( @rightExpression ), 
				SyntaxFactory.Argument( @into )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax JoinClause(ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @inExpression, ExpressionSyntax @leftExpression, ExpressionSyntax @rightExpression, ExpressionSyntax @into)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "JoinClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inExpression ), 
				SyntaxFactory.Argument( @leftExpression ), 
				SyntaxFactory.Argument( @rightExpression ), 
				SyntaxFactory.Argument( @into )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax JoinClause(ExpressionSyntax @identifier, ExpressionSyntax @inExpression, ExpressionSyntax @leftExpression, ExpressionSyntax @rightExpression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "JoinClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @inExpression ), 
				SyntaxFactory.Argument( @leftExpression ), 
				SyntaxFactory.Argument( @rightExpression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax JoinIntoClause(ExpressionSyntax @intoKeyword, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "JoinIntoClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @intoKeyword ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax JoinIntoClause(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "JoinIntoClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LabeledStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @identifier, ExpressionSyntax @colonToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LabeledStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LabeledStatement1(ExpressionSyntax @attributeLists, ExpressionSyntax @identifier, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LabeledStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LabeledStatement2(ExpressionSyntax @identifier, ExpressionSyntax @colonToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LabeledStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LabeledStatement(ExpressionSyntax @identifier, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LabeledStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LetClause(ExpressionSyntax @letKeyword, ExpressionSyntax @identifier, ExpressionSyntax @equalsToken, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LetClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @letKeyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @equalsToken ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LetClause(ExpressionSyntax @identifier, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LetClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineDirectivePosition(ExpressionSyntax @openParenToken, ExpressionSyntax @line, ExpressionSyntax @commaToken, ExpressionSyntax @character, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineDirectivePosition" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @line ), 
				SyntaxFactory.Argument( @commaToken ), 
				SyntaxFactory.Argument( @character ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineDirectivePosition(ExpressionSyntax @line, ExpressionSyntax @character)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineDirectivePosition" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @line ), 
				SyntaxFactory.Argument( @character )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @lineKeyword, ExpressionSyntax @line, ExpressionSyntax @file, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @lineKeyword ), 
				SyntaxFactory.Argument( @line ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineDirectiveTrivia(ExpressionSyntax @line, ExpressionSyntax @file, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @line ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineDirectiveTrivia(ExpressionSyntax @line, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @line ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineSpanDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @lineKeyword, ExpressionSyntax @start, ExpressionSyntax @minusToken, ExpressionSyntax @end, ExpressionSyntax @characterOffset, ExpressionSyntax @file, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineSpanDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @lineKeyword ), 
				SyntaxFactory.Argument( @start ), 
				SyntaxFactory.Argument( @minusToken ), 
				SyntaxFactory.Argument( @end ), 
				SyntaxFactory.Argument( @characterOffset ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineSpanDirectiveTrivia(ExpressionSyntax @start, ExpressionSyntax @end, ExpressionSyntax @characterOffset, ExpressionSyntax @file, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineSpanDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @start ), 
				SyntaxFactory.Argument( @end ), 
				SyntaxFactory.Argument( @characterOffset ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LineSpanDirectiveTrivia(ExpressionSyntax @start, ExpressionSyntax @end, ExpressionSyntax @file, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LineSpanDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @start ), 
				SyntaxFactory.Argument( @end ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax List<TNode>()
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "List", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax List<TNode>(IEnumerable<ExpressionSyntax> @nodes)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "List", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( 
					SyntaxFactory.ArrayType( this.Type(typeof(TNode)) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), 
					SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @nodes ))
				)).NormalizeWhitespace()})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Literal(ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Literal" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Literal(ExpressionSyntax @text, ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Literal" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Literal(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @value, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Literal" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LiteralExpression(ExpressionSyntax @kind, ExpressionSyntax @token)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LiteralExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @token )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LiteralExpression(ExpressionSyntax @kind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LiteralExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LoadDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @loadKeyword, ExpressionSyntax @file, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LoadDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @loadKeyword ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LoadDirectiveTrivia(ExpressionSyntax @file, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LoadDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @awaitKeyword, ExpressionSyntax @usingKeyword, ExpressionSyntax @modifiers, ExpressionSyntax @declaration, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement2(ExpressionSyntax @modifiers, ExpressionSyntax @declaration, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement(ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement(ExpressionSyntax @awaitKeyword, ExpressionSyntax @usingKeyword, ExpressionSyntax @modifiers, ExpressionSyntax @declaration, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalDeclarationStatement(ExpressionSyntax @modifiers, ExpressionSyntax @declaration)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalDeclarationStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @declaration )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalFunctionStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalFunctionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalFunctionStatement1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalFunctionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalFunctionStatement2(ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalFunctionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalFunctionStatement(ExpressionSyntax @returnType, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalFunctionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LocalFunctionStatement(ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LocalFunctionStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LockStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @lockKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LockStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @lockKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LockStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LockStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LockStatement(ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LockStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax LockStatement(ExpressionSyntax @lockKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "LockStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lockKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MakeRefExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MakeRefExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MakeRefExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MakeRefExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MemberAccessExpression(ExpressionSyntax @kind, ExpressionSyntax @expression, ExpressionSyntax @operatorToken, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MemberAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MemberAccessExpression(ExpressionSyntax @kind, ExpressionSyntax @expression, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MemberAccessExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MemberBindingExpression(ExpressionSyntax @operatorToken, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MemberBindingExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MemberBindingExpression(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MemberBindingExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MethodDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MethodDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MethodDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MethodDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MethodDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @constraintClauses, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MethodDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MethodDeclaration(ExpressionSyntax @returnType, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MethodDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MissingToken(ExpressionSyntax @kind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MissingToken" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax MissingToken(ExpressionSyntax @leading, ExpressionSyntax @kind, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "MissingToken" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameColon(ExpressionSyntax @name, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameColon" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameColon(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameColon" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameEquals(ExpressionSyntax @name, ExpressionSyntax @equalsToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameEquals" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @equalsToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameEquals(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameEquals" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameMemberCref(ExpressionSyntax @name, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NameMemberCref(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NameMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NamespaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @namespaceKeyword, ExpressionSyntax @name, ExpressionSyntax @openBraceToken, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @namespaceKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NamespaceDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @name, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NamespaceDeclaration(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NamespaceDeclaration(ExpressionSyntax @name, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NamespaceDeclaration(ExpressionSyntax @namespaceKeyword, ExpressionSyntax @name, ExpressionSyntax @openBraceToken, ExpressionSyntax @externs, ExpressionSyntax @usings, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NamespaceDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @namespaceKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @externs ), 
				SyntaxFactory.Argument( @usings ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NodeOrTokenList()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NodeOrTokenList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NodeOrTokenList(params ExpressionSyntax[] @nodesAndTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NodeOrTokenList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @nodesAndTokens.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NullableDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @nullableKeyword, ExpressionSyntax @settingToken, ExpressionSyntax @targetToken, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NullableDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @nullableKeyword ), 
				SyntaxFactory.Argument( @settingToken ), 
				SyntaxFactory.Argument( @targetToken ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NullableDirectiveTrivia(ExpressionSyntax @settingToken, ExpressionSyntax @targetToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NullableDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @settingToken ), 
				SyntaxFactory.Argument( @targetToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NullableDirectiveTrivia(ExpressionSyntax @settingToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NullableDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @settingToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NullableType(ExpressionSyntax @elementType, ExpressionSyntax @questionToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NullableType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType ), 
				SyntaxFactory.Argument( @questionToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax NullableType(ExpressionSyntax @elementType)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "NullableType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ObjectCreationExpression(ExpressionSyntax @newKeyword, ExpressionSyntax @type, ExpressionSyntax @argumentList, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @newKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @argumentList ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ObjectCreationExpression(ExpressionSyntax @type, ExpressionSyntax @argumentList, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @argumentList ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ObjectCreationExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ObjectCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OmittedArraySizeExpression(ExpressionSyntax @omittedArraySizeExpressionToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OmittedArraySizeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @omittedArraySizeExpressionToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OmittedArraySizeExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OmittedArraySizeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OmittedTypeArgument(ExpressionSyntax @omittedTypeArgumentToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OmittedTypeArgument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @omittedTypeArgumentToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OmittedTypeArgument()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OmittedTypeArgument" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @operatorKeyword, ExpressionSyntax @operatorToken, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @operatorToken, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration2(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @operatorKeyword, ExpressionSyntax @operatorToken, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration(ExpressionSyntax @returnType, ExpressionSyntax @operatorToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @operatorToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @operatorKeyword, ExpressionSyntax @operatorToken, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @operatorToken, ExpressionSyntax @parameterList, ExpressionSyntax @body, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorMemberCref(ExpressionSyntax @operatorKeyword, ExpressionSyntax @operatorToken, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorKeyword ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorMemberCref(ExpressionSyntax @operatorToken, ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OperatorMemberCref(ExpressionSyntax @operatorToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OperatorMemberCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OrderByClause(ExpressionSyntax @orderByKeyword, ExpressionSyntax @orderings)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OrderByClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @orderByKeyword ), 
				SyntaxFactory.Argument( @orderings )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax OrderByClause(ExpressionSyntax @orderings)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "OrderByClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @orderings )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Ordering(ExpressionSyntax @kind, ExpressionSyntax @expression, ExpressionSyntax @ascendingOrDescendingKeyword)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Ordering" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @ascendingOrDescendingKeyword )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Ordering(ExpressionSyntax @kind, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Ordering" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Parameter(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @identifier, ExpressionSyntax @default)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Parameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @default )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Parameter(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Parameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParameterList(ExpressionSyntax @openParenToken, ExpressionSyntax @parameters, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedExpression(ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @parameterList, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @returnType, ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @returnType ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression(ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression(ExpressionSyntax @parameterList, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression1(ExpressionSyntax @asyncKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @arrowToken, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression2(ExpressionSyntax @modifiers, ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression2(ExpressionSyntax @asyncKeyword, ExpressionSyntax @parameterList, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression3(ExpressionSyntax @modifiers, ExpressionSyntax @parameterList, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedLambdaExpression(ExpressionSyntax @parameterList, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedPattern(ExpressionSyntax @openParenToken, ExpressionSyntax @pattern, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedPattern(ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedVariableDesignation(ExpressionSyntax @openParenToken, ExpressionSyntax @variables, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedVariableDesignation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @variables ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParenthesizedVariableDesignation(ExpressionSyntax @variables)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParenthesizedVariableDesignation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @variables )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseArgumentList(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseAttributeArgumentList(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseAttributeArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseBracketedArgumentList(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseBracketedArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseBracketedParameterList(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseBracketedParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseCompilationUnit(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseCompilationUnit" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseExpression(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseLeadingTrivia(ExpressionSyntax @text, ExpressionSyntax @offset)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseLeadingTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseMemberDeclaration(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseMemberDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseName(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseParameterList(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseStatement(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree1(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @diagnosticOptions, ExpressionSyntax @isGeneratedCode, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @diagnosticOptions ), 
				SyntaxFactory.Argument( @isGeneratedCode ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree2(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @encoding, ExpressionSyntax @diagnosticOptions, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @encoding ), 
				SyntaxFactory.Argument( @diagnosticOptions ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree1(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @diagnosticOptions, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @diagnosticOptions ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree2(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @encoding, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @encoding ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @encoding, ExpressionSyntax @diagnosticOptions, ExpressionSyntax @isGeneratedCode, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @encoding ), 
				SyntaxFactory.Argument( @diagnosticOptions ), 
				SyntaxFactory.Argument( @isGeneratedCode ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseSyntaxTree(ExpressionSyntax @text, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @cancellationToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseSyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @cancellationToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseToken(ExpressionSyntax @text, ExpressionSyntax @offset)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseToken" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseTokens(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @initialTokenPosition, ExpressionSyntax @options)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseTokens" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @initialTokenPosition ), 
				SyntaxFactory.Argument( @options )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseTrailingTrivia(ExpressionSyntax @text, ExpressionSyntax @offset)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseTrailingTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseTypeName(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseTypeName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ParseTypeName(ExpressionSyntax @text, ExpressionSyntax @offset, ExpressionSyntax @options, ExpressionSyntax @consumeFullText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ParseTypeName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @offset ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @consumeFullText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PointerType(ExpressionSyntax @elementType, ExpressionSyntax @asteriskToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PointerType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType ), 
				SyntaxFactory.Argument( @asteriskToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PointerType(ExpressionSyntax @elementType)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PointerType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elementType )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PositionalPatternClause(ExpressionSyntax @openParenToken, ExpressionSyntax @subpatterns, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PositionalPatternClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @subpatterns ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PositionalPatternClause(ExpressionSyntax @subpatterns)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PositionalPatternClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @subpatterns )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PostfixUnaryExpression(ExpressionSyntax @kind, ExpressionSyntax @operand, ExpressionSyntax @operatorToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PostfixUnaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @operand ), 
				SyntaxFactory.Argument( @operatorToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PostfixUnaryExpression(ExpressionSyntax @kind, ExpressionSyntax @operand)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PostfixUnaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @operand )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PragmaChecksumDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @pragmaKeyword, ExpressionSyntax @checksumKeyword, ExpressionSyntax @file, ExpressionSyntax @guid, ExpressionSyntax @bytes, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PragmaChecksumDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @pragmaKeyword ), 
				SyntaxFactory.Argument( @checksumKeyword ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @guid ), 
				SyntaxFactory.Argument( @bytes ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PragmaChecksumDirectiveTrivia(ExpressionSyntax @file, ExpressionSyntax @guid, ExpressionSyntax @bytes, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PragmaChecksumDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @guid ), 
				SyntaxFactory.Argument( @bytes ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PragmaWarningDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @pragmaKeyword, ExpressionSyntax @warningKeyword, ExpressionSyntax @disableOrRestoreKeyword, ExpressionSyntax @errorCodes, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PragmaWarningDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @pragmaKeyword ), 
				SyntaxFactory.Argument( @warningKeyword ), 
				SyntaxFactory.Argument( @disableOrRestoreKeyword ), 
				SyntaxFactory.Argument( @errorCodes ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PragmaWarningDirectiveTrivia(ExpressionSyntax @disableOrRestoreKeyword, ExpressionSyntax @errorCodes, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PragmaWarningDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @disableOrRestoreKeyword ), 
				SyntaxFactory.Argument( @errorCodes ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PragmaWarningDirectiveTrivia(ExpressionSyntax @disableOrRestoreKeyword, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PragmaWarningDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @disableOrRestoreKeyword ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PredefinedType(ExpressionSyntax @keyword)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PredefinedType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PrefixUnaryExpression(ExpressionSyntax @kind, ExpressionSyntax @operatorToken, ExpressionSyntax @operand)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PrefixUnaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @operand )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PrefixUnaryExpression(ExpressionSyntax @kind, ExpressionSyntax @operand)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PrefixUnaryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @operand )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PreprocessingMessage(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PreprocessingMessage" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PrimaryConstructorBaseType(ExpressionSyntax @type, ExpressionSyntax @argumentList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PrimaryConstructorBaseType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @argumentList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PrimaryConstructorBaseType(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PrimaryConstructorBaseType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList, ExpressionSyntax @expressionBody, ExpressionSyntax @initializer, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @initializer ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList, ExpressionSyntax @expressionBody, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList ), 
				SyntaxFactory.Argument( @expressionBody ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyDeclaration(ExpressionSyntax @type, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @type, ExpressionSyntax @explicitInterfaceSpecifier, ExpressionSyntax @identifier, ExpressionSyntax @accessorList)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @explicitInterfaceSpecifier ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @accessorList )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyPatternClause(ExpressionSyntax @openBraceToken, ExpressionSyntax @subpatterns, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyPatternClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @subpatterns ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax PropertyPatternClause(ExpressionSyntax @subpatterns)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "PropertyPatternClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @subpatterns )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QualifiedCref(ExpressionSyntax @container, ExpressionSyntax @dotToken, ExpressionSyntax @member)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QualifiedCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @container ), 
				SyntaxFactory.Argument( @dotToken ), 
				SyntaxFactory.Argument( @member )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QualifiedCref(ExpressionSyntax @container, ExpressionSyntax @member)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QualifiedCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @container ), 
				SyntaxFactory.Argument( @member )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QualifiedName(ExpressionSyntax @left, ExpressionSyntax @dotToken, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QualifiedName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @dotToken ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QualifiedName(ExpressionSyntax @left, ExpressionSyntax @right)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QualifiedName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @left ), 
				SyntaxFactory.Argument( @right )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QueryBody(ExpressionSyntax @clauses, ExpressionSyntax @selectOrGroup, ExpressionSyntax @continuation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QueryBody" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @clauses ), 
				SyntaxFactory.Argument( @selectOrGroup ), 
				SyntaxFactory.Argument( @continuation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QueryBody(ExpressionSyntax @selectOrGroup)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QueryBody" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @selectOrGroup )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QueryContinuation(ExpressionSyntax @intoKeyword, ExpressionSyntax @identifier, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QueryContinuation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @intoKeyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QueryContinuation(ExpressionSyntax @identifier, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QueryContinuation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax QueryExpression(ExpressionSyntax @fromClause, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "QueryExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @fromClause ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RangeExpression(ExpressionSyntax @leftOperand, ExpressionSyntax @operatorToken, ExpressionSyntax @rightOperand)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RangeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leftOperand ), 
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @rightOperand )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RangeExpression(ExpressionSyntax @leftOperand, ExpressionSyntax @rightOperand)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RangeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leftOperand ), 
				SyntaxFactory.Argument( @rightOperand )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RangeExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RangeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @classOrStructKeyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @classOrStructKeyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @kind, ExpressionSyntax @keyword, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @parameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @parameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecordDeclaration(ExpressionSyntax @keyword, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecordDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecursivePattern(ExpressionSyntax @type, ExpressionSyntax @positionalPatternClause, ExpressionSyntax @propertyPatternClause, ExpressionSyntax @designation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecursivePattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @positionalPatternClause ), 
				SyntaxFactory.Argument( @propertyPatternClause ), 
				SyntaxFactory.Argument( @designation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RecursivePattern()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RecursivePattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReferenceDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @referenceKeyword, ExpressionSyntax @file, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReferenceDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @referenceKeyword ), 
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReferenceDirectiveTrivia(ExpressionSyntax @file, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReferenceDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @file ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefExpression(ExpressionSyntax @refKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @refKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefType(ExpressionSyntax @refKeyword, ExpressionSyntax @readOnlyKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @refKeyword ), 
				SyntaxFactory.Argument( @readOnlyKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefType(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefType(ExpressionSyntax @refKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @refKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefTypeExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefTypeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefTypeExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefTypeExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefValueExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @comma, ExpressionSyntax @type, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefValueExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @comma ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RefValueExpression(ExpressionSyntax @expression, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RefValueExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RegionDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @regionKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RegionDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @regionKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RegionDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RegionDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax RelationalPattern(ExpressionSyntax @operatorToken, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "RelationalPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReturnStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @returnKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReturnStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @returnKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReturnStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReturnStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReturnStatement(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReturnStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ReturnStatement(ExpressionSyntax @returnKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ReturnStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @returnKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SelectClause(ExpressionSyntax @selectKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SelectClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @selectKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SelectClause(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SelectClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SeparatedList<TNode>()
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SeparatedList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SeparatedList1<TNode>(IEnumerable<ExpressionSyntax> @nodesAndTokens)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SeparatedList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( 
					SyntaxFactory.ArrayType( this.Type(typeof(SyntaxNodeOrToken)) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), 
					SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @nodesAndTokens ))
				)).NormalizeWhitespace()})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SeparatedList2<TNode>(IEnumerable<ExpressionSyntax> @nodes)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SeparatedList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( 
					SyntaxFactory.ArrayType( this.Type(typeof(TNode)) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), 
					SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @nodes ))
				)).NormalizeWhitespace()})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SeparatedList<TNode>(IEnumerable<ExpressionSyntax> @nodes, IEnumerable<ExpressionSyntax> @separators)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SeparatedList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( 
					SyntaxFactory.ArrayType( this.Type(typeof(TNode)) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), 
					SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @nodes ))
				)).NormalizeWhitespace(), 
				SyntaxFactory.Argument(SyntaxFactory.ArrayCreationExpression( 
					SyntaxFactory.ArrayType( this.Type(typeof(SyntaxToken)) ).WithRankSpecifiers(SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression() ) ) ) ), 
					SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( @separators ))
				)).NormalizeWhitespace()})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ShebangDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @exclamationToken, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ShebangDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @exclamationToken ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ShebangDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ShebangDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleBaseType(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleBaseType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @parameter, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression1(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @parameter, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression2(ExpressionSyntax @asyncKeyword, ExpressionSyntax @parameter, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression3(ExpressionSyntax @modifiers, ExpressionSyntax @parameter, ExpressionSyntax @arrowToken, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression(ExpressionSyntax @parameter)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameter )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression(ExpressionSyntax @parameter, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression1(ExpressionSyntax @asyncKeyword, ExpressionSyntax @parameter, ExpressionSyntax @arrowToken, ExpressionSyntax @body)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @asyncKeyword ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @arrowToken ), 
				SyntaxFactory.Argument( @body )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression2(ExpressionSyntax @modifiers, ExpressionSyntax @parameter, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SimpleLambdaExpression(ExpressionSyntax @parameter, ExpressionSyntax @block, ExpressionSyntax @expressionBody)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SimpleLambdaExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameter ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @expressionBody )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SingletonList<TNode>(ExpressionSyntax @node)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SingletonList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @node )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SingletonSeparatedList<TNode>(ExpressionSyntax @node)
			=> SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( "SingletonSeparatedList", this.Type(typeof(TNode)) ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @node )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SingleVariableDesignation(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SingleVariableDesignation" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SizeOfExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SizeOfExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SizeOfExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SizeOfExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SkippedTokensTrivia(ExpressionSyntax @tokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SkippedTokensTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @tokens )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SkippedTokensTrivia()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SkippedTokensTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StackAllocArrayCreationExpression(ExpressionSyntax @stackAllocKeyword, ExpressionSyntax @type, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stackAllocKeyword ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StackAllocArrayCreationExpression1(ExpressionSyntax @type, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StackAllocArrayCreationExpression2(ExpressionSyntax @stackAllocKeyword, ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @stackAllocKeyword ), 
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StackAllocArrayCreationExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StackAllocArrayCreationExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StructDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StructDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StructDeclaration(ExpressionSyntax @attributeLists, ExpressionSyntax @modifiers, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @members)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StructDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @members )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax StructDeclaration(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "StructDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Subpattern1(ExpressionSyntax @expressionColon, ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Subpattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expressionColon ), 
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Subpattern2(ExpressionSyntax @nameColon, ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Subpattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @nameColon ), 
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Subpattern(ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Subpattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpression(ExpressionSyntax @governingExpression, ExpressionSyntax @switchKeyword, ExpressionSyntax @openBraceToken, ExpressionSyntax @arms, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @governingExpression ), 
				SyntaxFactory.Argument( @switchKeyword ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @arms ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpression(ExpressionSyntax @governingExpression, ExpressionSyntax @arms)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @governingExpression ), 
				SyntaxFactory.Argument( @arms )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpression(ExpressionSyntax @governingExpression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @governingExpression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpressionArm(ExpressionSyntax @pattern, ExpressionSyntax @whenClause, ExpressionSyntax @equalsGreaterThanToken, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpressionArm" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @whenClause ), 
				SyntaxFactory.Argument( @equalsGreaterThanToken ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpressionArm(ExpressionSyntax @pattern, ExpressionSyntax @whenClause, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpressionArm" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @whenClause ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchExpressionArm(ExpressionSyntax @pattern, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchExpressionArm" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchSection(ExpressionSyntax @labels, ExpressionSyntax @statements)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchSection" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @labels ), 
				SyntaxFactory.Argument( @statements )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchSection()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchSection" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @switchKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @openBraceToken, ExpressionSyntax @sections, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @switchKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @sections ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchStatement(ExpressionSyntax @expression, ExpressionSyntax @sections)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @sections )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchStatement(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SwitchStatement(ExpressionSyntax @switchKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @openBraceToken, ExpressionSyntax @sections, ExpressionSyntax @closeBraceToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SwitchStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @switchKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @sections ), 
				SyntaxFactory.Argument( @closeBraceToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SyntaxTree(ExpressionSyntax @root, ExpressionSyntax @options, ExpressionSyntax @path, ExpressionSyntax @encoding)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SyntaxTree" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @root ), 
				SyntaxFactory.Argument( @options ), 
				SyntaxFactory.Argument( @path ), 
				SyntaxFactory.Argument( @encoding )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax SyntaxTrivia(ExpressionSyntax @kind, ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "SyntaxTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThisExpression(ExpressionSyntax @token)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThisExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @token )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThisExpression()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThisExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowExpression(ExpressionSyntax @throwKeyword, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @throwKeyword ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowExpression(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @throwKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @throwKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowStatement(ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax ThrowStatement(ExpressionSyntax @throwKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "ThrowStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @throwKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Token(ExpressionSyntax @kind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Token" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Token(ExpressionSyntax @leading, ExpressionSyntax @kind, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Token" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Token(ExpressionSyntax @leading, ExpressionSyntax @kind, ExpressionSyntax @text, ExpressionSyntax @valueText, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Token" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @valueText ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TokenList()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TokenList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TokenList1(ExpressionSyntax @token)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TokenList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @token )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TokenList2(params ExpressionSyntax[] @tokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TokenList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @tokens.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Trivia(ExpressionSyntax @node)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Trivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @node )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TriviaList()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TriviaList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TriviaList1(ExpressionSyntax @trivia)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TriviaList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @trivia )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TriviaList2(params ExpressionSyntax[] @trivias)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TriviaList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @trivias.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TryStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @tryKeyword, ExpressionSyntax @block, ExpressionSyntax @catches, ExpressionSyntax @finally)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TryStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @tryKeyword ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @catches ), 
				SyntaxFactory.Argument( @finally )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TryStatement1(ExpressionSyntax @attributeLists, ExpressionSyntax @block, ExpressionSyntax @catches, ExpressionSyntax @finally)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TryStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @catches ), 
				SyntaxFactory.Argument( @finally )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TryStatement2(ExpressionSyntax @tryKeyword, ExpressionSyntax @block, ExpressionSyntax @catches, ExpressionSyntax @finally)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TryStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @tryKeyword ), 
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @catches ), 
				SyntaxFactory.Argument( @finally )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TryStatement(ExpressionSyntax @catches)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TryStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @catches )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TryStatement(ExpressionSyntax @block, ExpressionSyntax @catches, ExpressionSyntax @finally)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TryStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @block ), 
				SyntaxFactory.Argument( @catches ), 
				SyntaxFactory.Argument( @finally )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleElement(ExpressionSyntax @type, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleElement(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleExpression(ExpressionSyntax @openParenToken, ExpressionSyntax @arguments, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @arguments ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleExpression(ExpressionSyntax @arguments)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arguments )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleType(ExpressionSyntax @openParenToken, ExpressionSyntax @elements, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @elements ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TupleType(ExpressionSyntax @elements)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TupleType" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @elements )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeArgumentList(ExpressionSyntax @lessThanToken, ExpressionSyntax @arguments, ExpressionSyntax @greaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanToken ), 
				SyntaxFactory.Argument( @arguments ), 
				SyntaxFactory.Argument( @greaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeArgumentList(ExpressionSyntax @arguments)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeArgumentList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @arguments )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeConstraint(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeConstraint" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeCref(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeCref" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeDeclaration(ExpressionSyntax @kind, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeDeclaration(ExpressionSyntax @kind, ExpressionSyntax @attributes, ExpressionSyntax @modifiers, ExpressionSyntax @keyword, ExpressionSyntax @identifier, ExpressionSyntax @typeParameterList, ExpressionSyntax @baseList, ExpressionSyntax @constraintClauses, ExpressionSyntax @openBraceToken, ExpressionSyntax @members, ExpressionSyntax @closeBraceToken, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributes ), 
				SyntaxFactory.Argument( @modifiers ), 
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @typeParameterList ), 
				SyntaxFactory.Argument( @baseList ), 
				SyntaxFactory.Argument( @constraintClauses ), 
				SyntaxFactory.Argument( @openBraceToken ), 
				SyntaxFactory.Argument( @members ), 
				SyntaxFactory.Argument( @closeBraceToken ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeOfExpression(ExpressionSyntax @keyword, ExpressionSyntax @openParenToken, ExpressionSyntax @type, ExpressionSyntax @closeParenToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeOfExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @keyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @closeParenToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeOfExpression(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeOfExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameter(ExpressionSyntax @attributeLists, ExpressionSyntax @varianceKeyword, ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @varianceKeyword ), 
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameter(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameter" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameterConstraintClause(ExpressionSyntax @whereKeyword, ExpressionSyntax @name, ExpressionSyntax @colonToken, ExpressionSyntax @constraints)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameterConstraintClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @whereKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @colonToken ), 
				SyntaxFactory.Argument( @constraints )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameterConstraintClause(ExpressionSyntax @name, ExpressionSyntax @constraints)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameterConstraintClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @constraints )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameterConstraintClause(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameterConstraintClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameterList(ExpressionSyntax @lessThanToken, ExpressionSyntax @parameters, ExpressionSyntax @greaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanToken ), 
				SyntaxFactory.Argument( @parameters ), 
				SyntaxFactory.Argument( @greaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypeParameterList(ExpressionSyntax @parameters)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypeParameterList" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameters )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax TypePattern(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "TypePattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnaryPattern(ExpressionSyntax @operatorToken, ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnaryPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @operatorToken ), 
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnaryPattern(ExpressionSyntax @pattern)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnaryPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @pattern )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UndefDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @undefKeyword, ExpressionSyntax @name, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UndefDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @undefKeyword ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UndefDirectiveTrivia(ExpressionSyntax @name, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UndefDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnsafeStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @unsafeKeyword, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnsafeStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @unsafeKeyword ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnsafeStatement1(ExpressionSyntax @attributeLists, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnsafeStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnsafeStatement2(ExpressionSyntax @unsafeKeyword, ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnsafeStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @unsafeKeyword ), 
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UnsafeStatement(ExpressionSyntax @block)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UnsafeStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @block )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingDirective(ExpressionSyntax @globalKeyword, ExpressionSyntax @usingKeyword, ExpressionSyntax @staticKeyword, ExpressionSyntax @alias, ExpressionSyntax @name, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @globalKeyword ), 
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @staticKeyword ), 
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingDirective(ExpressionSyntax @staticKeyword, ExpressionSyntax @alias, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @staticKeyword ), 
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingDirective(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingDirective(ExpressionSyntax @alias, ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingDirective(ExpressionSyntax @usingKeyword, ExpressionSyntax @staticKeyword, ExpressionSyntax @alias, ExpressionSyntax @name, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingDirective" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @staticKeyword ), 
				SyntaxFactory.Argument( @alias ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @awaitKeyword, ExpressionSyntax @usingKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @declaration, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @usingKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @awaitKeyword, ExpressionSyntax @usingKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @declaration, ExpressionSyntax @expression, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @awaitKeyword ), 
				SyntaxFactory.Argument( @usingKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax UsingStatement(ExpressionSyntax @declaration, ExpressionSyntax @expression, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "UsingStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @declaration ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VariableDeclaration(ExpressionSyntax @type, ExpressionSyntax @variables)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VariableDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type ), 
				SyntaxFactory.Argument( @variables )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VariableDeclaration(ExpressionSyntax @type)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VariableDeclaration" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @type )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VariableDeclarator(ExpressionSyntax @identifier, ExpressionSyntax @argumentList, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VariableDeclarator" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @argumentList ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VariableDeclarator(ExpressionSyntax @identifier)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VariableDeclarator" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @identifier )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VarPattern(ExpressionSyntax @varKeyword, ExpressionSyntax @designation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VarPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @varKeyword ), 
				SyntaxFactory.Argument( @designation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VarPattern(ExpressionSyntax @designation)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VarPattern" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @designation )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax VerbatimIdentifier(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @valueText, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "VerbatimIdentifier" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @valueText ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WarningDirectiveTrivia(ExpressionSyntax @hashToken, ExpressionSyntax @warningKeyword, ExpressionSyntax @endOfDirectiveToken, ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WarningDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @hashToken ), 
				SyntaxFactory.Argument( @warningKeyword ), 
				SyntaxFactory.Argument( @endOfDirectiveToken ), 
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WarningDirectiveTrivia(ExpressionSyntax @isActive)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WarningDirectiveTrivia" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isActive )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhenClause(ExpressionSyntax @whenKeyword, ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhenClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @whenKeyword ), 
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhenClause(ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhenClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhereClause(ExpressionSyntax @whereKeyword, ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhereClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @whereKeyword ), 
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhereClause(ExpressionSyntax @condition)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhereClause" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhileStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @whileKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhileStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @whileKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhileStatement(ExpressionSyntax @attributeLists, ExpressionSyntax @condition, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhileStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhileStatement(ExpressionSyntax @condition, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhileStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WhileStatement(ExpressionSyntax @whileKeyword, ExpressionSyntax @openParenToken, ExpressionSyntax @condition, ExpressionSyntax @closeParenToken, ExpressionSyntax @statement)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WhileStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @whileKeyword ), 
				SyntaxFactory.Argument( @openParenToken ), 
				SyntaxFactory.Argument( @condition ), 
				SyntaxFactory.Argument( @closeParenToken ), 
				SyntaxFactory.Argument( @statement )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Whitespace(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Whitespace" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax Whitespace(ExpressionSyntax @text, ExpressionSyntax @elastic)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "Whitespace" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @elastic )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WithExpression(ExpressionSyntax @expression, ExpressionSyntax @withKeyword, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WithExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @withKeyword ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax WithExpression(ExpressionSyntax @expression, ExpressionSyntax @initializer)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "WithExpression" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @initializer )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCDataSection(ExpressionSyntax @startCDataToken, ExpressionSyntax @textTokens, ExpressionSyntax @endCDataToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCDataSection" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @startCDataToken ), 
				SyntaxFactory.Argument( @textTokens ), 
				SyntaxFactory.Argument( @endCDataToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCDataSection(ExpressionSyntax @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCDataSection" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @textTokens )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlComment(ExpressionSyntax @lessThanExclamationMinusMinusToken, ExpressionSyntax @textTokens, ExpressionSyntax @minusMinusGreaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlComment" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanExclamationMinusMinusToken ), 
				SyntaxFactory.Argument( @textTokens ), 
				SyntaxFactory.Argument( @minusMinusGreaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlComment(ExpressionSyntax @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlComment" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @textTokens )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCrefAttribute(ExpressionSyntax @name, ExpressionSyntax @equalsToken, ExpressionSyntax @startQuoteToken, ExpressionSyntax @cref, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCrefAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @equalsToken ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @cref ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCrefAttribute(ExpressionSyntax @name, ExpressionSyntax @startQuoteToken, ExpressionSyntax @cref, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCrefAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @cref ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCrefAttribute(ExpressionSyntax @cref)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCrefAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlCrefAttribute(ExpressionSyntax @cref, ExpressionSyntax @quoteKind)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlCrefAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref ), 
				SyntaxFactory.Argument( @quoteKind )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElement(ExpressionSyntax @startTag, ExpressionSyntax @content, ExpressionSyntax @endTag)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @startTag ), 
				SyntaxFactory.Argument( @content ), 
				SyntaxFactory.Argument( @endTag )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElement1(ExpressionSyntax @startTag, ExpressionSyntax @endTag)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @startTag ), 
				SyntaxFactory.Argument( @endTag )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElement2(ExpressionSyntax @name, ExpressionSyntax @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @content )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElement3(ExpressionSyntax @localName, ExpressionSyntax @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @localName ), 
				SyntaxFactory.Argument( @content )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElementEndTag(ExpressionSyntax @lessThanSlashToken, ExpressionSyntax @name, ExpressionSyntax @greaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElementEndTag" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanSlashToken ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @greaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElementEndTag(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElementEndTag" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElementStartTag(ExpressionSyntax @lessThanToken, ExpressionSyntax @name, ExpressionSyntax @attributes, ExpressionSyntax @greaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElementStartTag" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanToken ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @attributes ), 
				SyntaxFactory.Argument( @greaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElementStartTag(ExpressionSyntax @name, ExpressionSyntax @attributes)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElementStartTag" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @attributes )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlElementStartTag(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlElementStartTag" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlEmptyElement(ExpressionSyntax @lessThanToken, ExpressionSyntax @name, ExpressionSyntax @attributes, ExpressionSyntax @slashGreaterThanToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlEmptyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @lessThanToken ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @attributes ), 
				SyntaxFactory.Argument( @slashGreaterThanToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlEmptyElement(ExpressionSyntax @name, ExpressionSyntax @attributes)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlEmptyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @attributes )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlEmptyElement1(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlEmptyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlEmptyElement2(ExpressionSyntax @localName)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlEmptyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @localName )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlEntity(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @value, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlEntity" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlExampleElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlExampleElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlExceptionElement(ExpressionSyntax @cref, params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlExceptionElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref )}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlMultiLineElement1(ExpressionSyntax @name, ExpressionSyntax @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlMultiLineElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @content )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlMultiLineElement2(ExpressionSyntax @localName, ExpressionSyntax @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlMultiLineElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @localName ), 
				SyntaxFactory.Argument( @content )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlName(ExpressionSyntax @prefix, ExpressionSyntax @localName)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @prefix ), 
				SyntaxFactory.Argument( @localName )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlName(ExpressionSyntax @localName)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlName" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @localName )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlNameAttribute(ExpressionSyntax @name, ExpressionSyntax @equalsToken, ExpressionSyntax @startQuoteToken, ExpressionSyntax @identifier, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlNameAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @equalsToken ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlNameAttribute(ExpressionSyntax @name, ExpressionSyntax @startQuoteToken, ExpressionSyntax @identifier, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlNameAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @identifier ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlNameAttribute(ExpressionSyntax @parameterName)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlNameAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterName )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlNewLine(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlNewLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlNullKeywordElement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlNullKeywordElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlParaElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlParaElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlParamElement(ExpressionSyntax @parameterName, params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlParamElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterName )}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlParamRefElement(ExpressionSyntax @parameterName)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlParamRefElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @parameterName )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlPermissionElement(ExpressionSyntax @cref, params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlPermissionElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref )}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlPlaceholderElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlPlaceholderElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlPrefix(ExpressionSyntax @prefix, ExpressionSyntax @colonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlPrefix" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @prefix ), 
				SyntaxFactory.Argument( @colonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlPrefix(ExpressionSyntax @prefix)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlPrefix" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @prefix )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlPreliminaryElement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlPreliminaryElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlProcessingInstruction(ExpressionSyntax @startProcessingInstructionToken, ExpressionSyntax @name, ExpressionSyntax @textTokens, ExpressionSyntax @endProcessingInstructionToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlProcessingInstruction" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @startProcessingInstructionToken ), 
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @textTokens ), 
				SyntaxFactory.Argument( @endProcessingInstructionToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlProcessingInstruction(ExpressionSyntax @name, ExpressionSyntax @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlProcessingInstruction" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @textTokens )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlProcessingInstruction(ExpressionSyntax @name)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlProcessingInstruction" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlRemarksElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlRemarksElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlReturnsElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlReturnsElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlSeeAlsoElement(ExpressionSyntax @cref)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlSeeAlsoElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlSeeAlsoElement(ExpressionSyntax @linkAddress, ExpressionSyntax @linkText)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlSeeAlsoElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @linkAddress ), 
				SyntaxFactory.Argument( @linkText )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlSeeElement(ExpressionSyntax @cref)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlSeeElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @cref )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlSummaryElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlSummaryElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlText1(params ExpressionSyntax[] @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @textTokens.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlText2(ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlText()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlText" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute(ExpressionSyntax @name, ExpressionSyntax @equalsToken, ExpressionSyntax @startQuoteToken, ExpressionSyntax @textTokens, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @equalsToken ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @textTokens ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute(ExpressionSyntax @name, ExpressionSyntax @startQuoteToken, ExpressionSyntax @textTokens, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @textTokens ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute1(ExpressionSyntax @name, ExpressionSyntax @startQuoteToken, ExpressionSyntax @endQuoteToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @startQuoteToken ), 
				SyntaxFactory.Argument( @endQuoteToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute2(ExpressionSyntax @name, ExpressionSyntax @quoteKind, ExpressionSyntax @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @quoteKind ), 
				SyntaxFactory.Argument( @textTokens )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute1(ExpressionSyntax @name, params ExpressionSyntax[] @textTokens)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name )}))				.AddArguments( @textTokens.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextAttribute2(ExpressionSyntax @name, ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextAttribute" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @name ), 
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextLiteral(ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextLiteral" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextLiteral(ExpressionSyntax @text, ExpressionSyntax @value)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextLiteral" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextLiteral(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @value, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextLiteral" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextNewLine(ExpressionSyntax @text)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextNewLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextNewLine(ExpressionSyntax @leading, ExpressionSyntax @text, ExpressionSyntax @value, ExpressionSyntax @trailing)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextNewLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @leading ), 
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @value ), 
				SyntaxFactory.Argument( @trailing )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlTextNewLine(ExpressionSyntax @text, ExpressionSyntax @continueXmlDocumentationComment)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlTextNewLine" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @text ), 
				SyntaxFactory.Argument( @continueXmlDocumentationComment )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlThreadSafetyElement()
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlThreadSafetyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlThreadSafetyElement(ExpressionSyntax @isStatic, ExpressionSyntax @isInstance)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlThreadSafetyElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @isStatic ), 
				SyntaxFactory.Argument( @isInstance )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax XmlValueElement(params ExpressionSyntax[] @content)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "XmlValueElement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{}))				.AddArguments( @content.Select( p => SyntaxFactory.Argument( p ) ).ToArray() )
);

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax YieldStatement(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @yieldKeyword, ExpressionSyntax @returnOrBreakKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "YieldStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @yieldKeyword ), 
				SyntaxFactory.Argument( @returnOrBreakKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax YieldStatement(ExpressionSyntax @kind, ExpressionSyntax @attributeLists, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "YieldStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @attributeLists ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax YieldStatement(ExpressionSyntax @kind, ExpressionSyntax @expression)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "YieldStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @expression )})));

		[ExcludeFromCodeCoverage]
		public InvocationExpressionSyntax YieldStatement(ExpressionSyntax @kind, ExpressionSyntax @yieldKeyword, ExpressionSyntax @returnOrBreakKeyword, ExpressionSyntax @expression, ExpressionSyntax @semicolonToken)
			=> SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( "YieldStatement" ), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>( new ArgumentSyntax[]{
				SyntaxFactory.Argument( @kind ), 
				SyntaxFactory.Argument( @yieldKeyword ), 
				SyntaxFactory.Argument( @returnOrBreakKeyword ), 
				SyntaxFactory.Argument( @expression ), 
				SyntaxFactory.Argument( @semicolonToken )})));

}
}
}
