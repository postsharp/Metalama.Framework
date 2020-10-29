using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela..Framework.Impl.Templating
{
	partial class MetaSyntaxRewriter
	{
		public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AccessorDeclaration( node.Kind(), node.AttributeLists, node.Modifiers, node.Keyword, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformAccessorDeclaration( node );
				default: 
					return base.VisitAccessorDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformAccessorDeclaration( AccessorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AccessorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAccessorList( AccessorListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AccessorList( node.OpenBraceToken, node.Accessors, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformAccessorList( node );
				default: 
					return base.VisitAccessorList( node );
			}
		}
		protected virtual ExpressionSyntax TransformAccessorList( AccessorListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AccessorList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Accessors)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAliasQualifiedName( AliasQualifiedNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AliasQualifiedName( node.Alias, node.ColonColonToken, node.Name);
				case TransformationKind.Transform: 
					return this.TransformAliasQualifiedName( node );
				default: 
					return base.VisitAliasQualifiedName( node );
			}
		}
		protected virtual ExpressionSyntax TransformAliasQualifiedName( AliasQualifiedNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AliasQualifiedName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Alias)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAnonymousMethodExpression( AnonymousMethodExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AnonymousMethodExpression( node.AsyncKeyword, node.DelegateKeyword, node.ParameterList, node.Block, node.ExpressionBody);
				case TransformationKind.Transform: 
					return this.TransformAnonymousMethodExpression( node );
				default: 
					return base.VisitAnonymousMethodExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformAnonymousMethodExpression( AnonymousMethodExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AnonymousMethodExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AsyncKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AnonymousObjectCreationExpression( node.NewKeyword, node.OpenBraceToken, node.Initializers, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformAnonymousObjectCreationExpression( node );
				default: 
					return base.VisitAnonymousObjectCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformAnonymousObjectCreationExpression( AnonymousObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AnonymousObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AnonymousObjectMemberDeclarator( node.NameEquals, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformAnonymousObjectMemberDeclarator( node );
				default: 
					return base.VisitAnonymousObjectMemberDeclarator( node );
			}
		}
		protected virtual ExpressionSyntax TransformAnonymousObjectMemberDeclarator( AnonymousObjectMemberDeclaratorSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AnonymousObjectMemberDeclarator))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameEquals)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArgument( ArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Argument( node.NameColon, node.RefKindKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformArgument( node );
				default: 
					return base.VisitArgument( node );
			}
		}
		protected virtual ExpressionSyntax TransformArgument( ArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Argument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.RefKindKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArgumentList( ArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ArgumentList( node.OpenParenToken, node.Arguments, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformArgumentList( node );
				default: 
					return base.VisitArgumentList( node );
			}
		}
		protected virtual ExpressionSyntax TransformArgumentList( ArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArrayCreationExpression( ArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ArrayCreationExpression( node.NewKeyword, node.Type, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformArrayCreationExpression( node );
				default: 
					return base.VisitArrayCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformArrayCreationExpression( ArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArrayRankSpecifier( ArrayRankSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ArrayRankSpecifier( node.OpenBracketToken, node.Sizes, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformArrayRankSpecifier( node );
				default: 
					return base.VisitArrayRankSpecifier( node );
			}
		}
		protected virtual ExpressionSyntax TransformArrayRankSpecifier( ArrayRankSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ArrayRankSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Sizes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArrayType( ArrayTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ArrayType( node.ElementType, node.RankSpecifiers);
				case TransformationKind.Transform: 
					return this.TransformArrayType( node );
				default: 
					return base.VisitArrayType( node );
			}
		}
		protected virtual ExpressionSyntax TransformArrayType( ArrayTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ArrayType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.RankSpecifiers)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ArrowExpressionClause( node.ArrowToken, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformArrowExpressionClause( node );
				default: 
					return base.VisitArrowExpressionClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformArrowExpressionClause( ArrowExpressionClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ArrowExpressionClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAssignmentExpression( AssignmentExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AssignmentExpression( node.Kind(), node.Left, node.OperatorToken, node.Right);
				case TransformationKind.Transform: 
					return this.TransformAssignmentExpression( node );
				default: 
					return base.VisitAssignmentExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformAssignmentExpression( AssignmentExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AssignmentExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAttribute( AttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Attribute( node.Name, node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformAttribute( node );
				default: 
					return base.VisitAttribute( node );
			}
		}
		protected virtual ExpressionSyntax TransformAttribute( AttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Attribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAttributeArgument( AttributeArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AttributeArgument( node.NameEquals, node.NameColon, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformAttributeArgument( node );
				default: 
					return base.VisitAttributeArgument( node );
			}
		}
		protected virtual ExpressionSyntax TransformAttributeArgument( AttributeArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AttributeArgument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameEquals)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.NameColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAttributeArgumentList( AttributeArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AttributeArgumentList( node.OpenParenToken, node.Arguments, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformAttributeArgumentList( node );
				default: 
					return base.VisitAttributeArgumentList( node );
			}
		}
		protected virtual ExpressionSyntax TransformAttributeArgumentList( AttributeArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AttributeArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAttributeList( AttributeListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AttributeList( node.OpenBracketToken, node.Target, node.Attributes, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformAttributeList( node );
				default: 
					return base.VisitAttributeList( node );
			}
		}
		protected virtual ExpressionSyntax TransformAttributeList( AttributeListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AttributeList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Target)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAttributeTargetSpecifier( AttributeTargetSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AttributeTargetSpecifier( node.Identifier, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformAttributeTargetSpecifier( node );
				default: 
					return base.VisitAttributeTargetSpecifier( node );
			}
		}
		protected virtual ExpressionSyntax TransformAttributeTargetSpecifier( AttributeTargetSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AttributeTargetSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitAwaitExpression( AwaitExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return AwaitExpression( node.AwaitKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformAwaitExpression( node );
				default: 
					return base.VisitAwaitExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformAwaitExpression( AwaitExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(AwaitExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBadDirectiveTrivia( BadDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BadDirectiveTrivia( node.HashToken, node.Identifier, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformBadDirectiveTrivia( node );
				default: 
					return base.VisitBadDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformBadDirectiveTrivia( BadDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BadDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBaseExpression( BaseExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BaseExpression( node.Token);
				case TransformationKind.Transform: 
					return this.TransformBaseExpression( node );
				default: 
					return base.VisitBaseExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformBaseExpression( BaseExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BaseExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBaseList( BaseListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BaseList( node.ColonToken, node.Types);
				case TransformationKind.Transform: 
					return this.TransformBaseList( node );
				default: 
					return base.VisitBaseList( node );
			}
		}
		protected virtual ExpressionSyntax TransformBaseList( BaseListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BaseList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Types)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBinaryExpression( BinaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BinaryExpression( node.Kind(), node.Left, node.OperatorToken, node.Right);
				case TransformationKind.Transform: 
					return this.TransformBinaryExpression( node );
				default: 
					return base.VisitBinaryExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformBinaryExpression( BinaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BinaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBinaryPattern( BinaryPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BinaryPattern( node.Kind(), node.Left, node.OperatorToken, node.Right);
				case TransformationKind.Transform: 
					return this.TransformBinaryPattern( node );
				default: 
					return base.VisitBinaryPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformBinaryPattern( BinaryPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BinaryPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBlock( BlockSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Block( node.AttributeLists, node.OpenBraceToken, node.Statements, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformBlock( node );
				default: 
					return base.VisitBlock( node );
			}
		}
		protected virtual ExpressionSyntax TransformBlock( BlockSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Block))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statements)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBracketedArgumentList( BracketedArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BracketedArgumentList( node.OpenBracketToken, node.Arguments, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformBracketedArgumentList( node );
				default: 
					return base.VisitBracketedArgumentList( node );
			}
		}
		protected virtual ExpressionSyntax TransformBracketedArgumentList( BracketedArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BracketedArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBracketedParameterList( BracketedParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BracketedParameterList( node.OpenBracketToken, node.Parameters, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformBracketedParameterList( node );
				default: 
					return base.VisitBracketedParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformBracketedParameterList( BracketedParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BracketedParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitBreakStatement( BreakStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return BreakStatement( node.AttributeLists, node.BreakKeyword, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformBreakStatement( node );
				default: 
					return base.VisitBreakStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformBreakStatement( BreakStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(BreakStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BreakKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CasePatternSwitchLabel( node.Keyword, node.Pattern, node.WhenClause, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformCasePatternSwitchLabel( node );
				default: 
					return base.VisitCasePatternSwitchLabel( node );
			}
		}
		protected virtual ExpressionSyntax TransformCasePatternSwitchLabel( CasePatternSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CasePatternSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhenClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCaseSwitchLabel( CaseSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CaseSwitchLabel( node.Keyword, node.Value, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformCaseSwitchLabel( node );
				default: 
					return base.VisitCaseSwitchLabel( node );
			}
		}
		protected virtual ExpressionSyntax TransformCaseSwitchLabel( CaseSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CaseSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCastExpression( CastExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CastExpression( node.OpenParenToken, node.Type, node.CloseParenToken, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformCastExpression( node );
				default: 
					return base.VisitCastExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformCastExpression( CastExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CastExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCatchClause( CatchClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CatchClause( node.CatchKeyword, node.Declaration, node.Filter, node.Block);
				case TransformationKind.Transform: 
					return this.TransformCatchClause( node );
				default: 
					return base.VisitCatchClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformCatchClause( CatchClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CatchClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.CatchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Filter)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCatchDeclaration( CatchDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CatchDeclaration( node.OpenParenToken, node.Type, node.Identifier, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformCatchDeclaration( node );
				default: 
					return base.VisitCatchDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformCatchDeclaration( CatchDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CatchDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCatchFilterClause( CatchFilterClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CatchFilterClause( node.WhenKeyword, node.OpenParenToken, node.FilterExpression, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformCatchFilterClause( node );
				default: 
					return base.VisitCatchFilterClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformCatchFilterClause( CatchFilterClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CatchFilterClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhenKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.FilterExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ClassDeclaration( node.AttributeLists, node.Modifiers, node.Keyword, node.Identifier, node.TypeParameterList, node.BaseList, node.ConstraintClauses, node.OpenBraceToken, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformClassDeclaration( node );
				default: 
					return base.VisitClassDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformClassDeclaration( ClassDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ClassDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitClassOrStructConstraint( ClassOrStructConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ClassOrStructConstraint( node.Kind(), node.ClassOrStructKeyword, node.QuestionToken);
				case TransformationKind.Transform: 
					return this.TransformClassOrStructConstraint( node );
				default: 
					return base.VisitClassOrStructConstraint( node );
			}
		}
		protected virtual ExpressionSyntax TransformClassOrStructConstraint( ClassOrStructConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ClassOrStructConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ClassOrStructKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CompilationUnit( node.Externs, node.Usings, node.AttributeLists, node.Members, node.EndOfFileToken);
				case TransformationKind.Transform: 
					return this.TransformCompilationUnit( node );
				default: 
					return base.VisitCompilationUnit( node );
			}
		}
		protected virtual ExpressionSyntax TransformCompilationUnit( CompilationUnitSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CompilationUnit))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Externs)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Usings)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfFileToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConditionalAccessExpression( node.Expression, node.OperatorToken, node.WhenNotNull);
				case TransformationKind.Transform: 
					return this.TransformConditionalAccessExpression( node );
				default: 
					return base.VisitConditionalAccessExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformConditionalAccessExpression( ConditionalAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConditionalAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhenNotNull)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConditionalExpression( ConditionalExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConditionalExpression( node.Condition, node.QuestionToken, node.WhenTrue, node.ColonToken, node.WhenFalse);
				case TransformationKind.Transform: 
					return this.TransformConditionalExpression( node );
				default: 
					return base.VisitConditionalExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformConditionalExpression( ConditionalExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConditionalExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhenTrue)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhenFalse)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConstantPattern( ConstantPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConstantPattern( node.Expression);
				case TransformationKind.Transform: 
					return this.TransformConstantPattern( node );
				default: 
					return base.VisitConstantPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformConstantPattern( ConstantPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConstantPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConstructorConstraint( ConstructorConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConstructorConstraint( node.NewKeyword, node.OpenParenToken, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformConstructorConstraint( node );
				default: 
					return base.VisitConstructorConstraint( node );
			}
		}
		protected virtual ExpressionSyntax TransformConstructorConstraint( ConstructorConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConstructorConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConstructorDeclaration( node.AttributeLists, node.Modifiers, node.Identifier, node.ParameterList, node.Initializer, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformConstructorDeclaration( node );
				default: 
					return base.VisitConstructorDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformConstructorDeclaration( ConstructorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConstructorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConstructorInitializer( ConstructorInitializerSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConstructorInitializer( node.Kind(), node.ColonToken, node.ThisOrBaseKeyword, node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformConstructorInitializer( node );
				default: 
					return base.VisitConstructorInitializer( node );
			}
		}
		protected virtual ExpressionSyntax TransformConstructorInitializer( ConstructorInitializerSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConstructorInitializer))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ThisOrBaseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitContinueStatement( ContinueStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ContinueStatement( node.AttributeLists, node.ContinueKeyword, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformContinueStatement( node );
				default: 
					return base.VisitContinueStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformContinueStatement( ContinueStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ContinueStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ContinueKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConversionOperatorDeclaration( node.AttributeLists, node.Modifiers, node.ImplicitOrExplicitKeyword, node.OperatorKeyword, node.Type, node.ParameterList, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformConversionOperatorDeclaration( node );
				default: 
					return base.VisitConversionOperatorDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConversionOperatorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ImplicitOrExplicitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitConversionOperatorMemberCref( ConversionOperatorMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ConversionOperatorMemberCref( node.ImplicitOrExplicitKeyword, node.OperatorKeyword, node.Type, node.Parameters);
				case TransformationKind.Transform: 
					return this.TransformConversionOperatorMemberCref( node );
				default: 
					return base.VisitConversionOperatorMemberCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformConversionOperatorMemberCref( ConversionOperatorMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ConversionOperatorMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ImplicitOrExplicitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCrefBracketedParameterList( CrefBracketedParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CrefBracketedParameterList( node.OpenBracketToken, node.Parameters, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformCrefBracketedParameterList( node );
				default: 
					return base.VisitCrefBracketedParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformCrefBracketedParameterList( CrefBracketedParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CrefBracketedParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCrefParameter( CrefParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CrefParameter( node.RefKindKeyword, node.Type);
				case TransformationKind.Transform: 
					return this.TransformCrefParameter( node );
				default: 
					return base.VisitCrefParameter( node );
			}
		}
		protected virtual ExpressionSyntax TransformCrefParameter( CrefParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CrefParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKindKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCrefParameterList( CrefParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CrefParameterList( node.OpenParenToken, node.Parameters, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformCrefParameterList( node );
				default: 
					return base.VisitCrefParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformCrefParameterList( CrefParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CrefParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDeclarationExpression( DeclarationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DeclarationExpression( node.Type, node.Designation);
				case TransformationKind.Transform: 
					return this.TransformDeclarationExpression( node );
				default: 
					return base.VisitDeclarationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformDeclarationExpression( DeclarationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DeclarationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDeclarationPattern( DeclarationPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DeclarationPattern( node.Type, node.Designation);
				case TransformationKind.Transform: 
					return this.TransformDeclarationPattern( node );
				default: 
					return base.VisitDeclarationPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformDeclarationPattern( DeclarationPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DeclarationPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDefaultConstraint( DefaultConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DefaultConstraint( node.DefaultKeyword);
				case TransformationKind.Transform: 
					return this.TransformDefaultConstraint( node );
				default: 
					return base.VisitDefaultConstraint( node );
			}
		}
		protected virtual ExpressionSyntax TransformDefaultConstraint( DefaultConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DefaultConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.DefaultKeyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDefaultExpression( DefaultExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DefaultExpression( node.Keyword, node.OpenParenToken, node.Type, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformDefaultExpression( node );
				default: 
					return base.VisitDefaultExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformDefaultExpression( DefaultExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DefaultExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDefaultSwitchLabel( DefaultSwitchLabelSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DefaultSwitchLabel( node.Keyword, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformDefaultSwitchLabel( node );
				default: 
					return base.VisitDefaultSwitchLabel( node );
			}
		}
		protected virtual ExpressionSyntax TransformDefaultSwitchLabel( DefaultSwitchLabelSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DefaultSwitchLabel))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDefineDirectiveTrivia( DefineDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DefineDirectiveTrivia( node.HashToken, node.DefineKeyword, node.Name, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformDefineDirectiveTrivia( node );
				default: 
					return base.VisitDefineDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformDefineDirectiveTrivia( DefineDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DefineDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DefineKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDelegateDeclaration( DelegateDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DelegateDeclaration( node.AttributeLists, node.Modifiers, node.DelegateKeyword, node.ReturnType, node.Identifier, node.TypeParameterList, node.ParameterList, node.ConstraintClauses, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformDelegateDeclaration( node );
				default: 
					return base.VisitDelegateDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformDelegateDeclaration( DelegateDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DelegateDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDestructorDeclaration( DestructorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DestructorDeclaration( node.AttributeLists, node.Modifiers, node.TildeToken, node.Identifier, node.ParameterList, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformDestructorDeclaration( node );
				default: 
					return base.VisitDestructorDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformDestructorDeclaration( DestructorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DestructorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TildeToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDiscardDesignation( DiscardDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DiscardDesignation( node.UnderscoreToken);
				case TransformationKind.Transform: 
					return this.TransformDiscardDesignation( node );
				default: 
					return base.VisitDiscardDesignation( node );
			}
		}
		protected virtual ExpressionSyntax TransformDiscardDesignation( DiscardDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DiscardDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.UnderscoreToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDiscardPattern( DiscardPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DiscardPattern( node.UnderscoreToken);
				case TransformationKind.Transform: 
					return this.TransformDiscardPattern( node );
				default: 
					return base.VisitDiscardPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformDiscardPattern( DiscardPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DiscardPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.UnderscoreToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDocumentationCommentTrivia( DocumentationCommentTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DocumentationCommentTrivia( node.Kind(), node.Content, node.EndOfComment);
				case TransformationKind.Transform: 
					return this.TransformDocumentationCommentTrivia( node );
				default: 
					return base.VisitDocumentationCommentTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformDocumentationCommentTrivia( DocumentationCommentTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DocumentationCommentTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Content)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfComment)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitDoStatement( DoStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return DoStatement( node.AttributeLists, node.DoKeyword, node.Statement, node.WhileKeyword, node.OpenParenToken, node.Condition, node.CloseParenToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformDoStatement( node );
				default: 
					return base.VisitDoStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformDoStatement( DoStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(DoStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhileKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitElementAccessExpression( ElementAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ElementAccessExpression( node.Expression, node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformElementAccessExpression( node );
				default: 
					return base.VisitElementAccessExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformElementAccessExpression( ElementAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ElementAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitElementBindingExpression( ElementBindingExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ElementBindingExpression( node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformElementBindingExpression( node );
				default: 
					return base.VisitElementBindingExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformElementBindingExpression( ElementBindingExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ElementBindingExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitElifDirectiveTrivia( ElifDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ElifDirectiveTrivia( node.HashToken, node.ElifKeyword, node.Condition, node.EndOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
				case TransformationKind.Transform: 
					return this.TransformElifDirectiveTrivia( node );
				default: 
					return base.VisitElifDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformElifDirectiveTrivia( ElifDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ElifDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ElifKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConditionValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitElseClause( ElseClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ElseClause( node.ElseKeyword, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformElseClause( node );
				default: 
					return base.VisitElseClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformElseClause( ElseClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ElseClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ElseDirectiveTrivia( node.HashToken, node.ElseKeyword, node.EndOfDirectiveToken, node.IsActive, node.BranchTaken);
				case TransformationKind.Transform: 
					return this.TransformElseDirectiveTrivia( node );
				default: 
					return base.VisitElseDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformElseDirectiveTrivia( ElseDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ElseDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ElseKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEmptyStatement( EmptyStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EmptyStatement( node.AttributeLists, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformEmptyStatement( node );
				default: 
					return base.VisitEmptyStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformEmptyStatement( EmptyStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EmptyStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EndIfDirectiveTrivia( node.HashToken, node.EndIfKeyword, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformEndIfDirectiveTrivia( node );
				default: 
					return base.VisitEndIfDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EndIfDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndIfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEndRegionDirectiveTrivia( EndRegionDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EndRegionDirectiveTrivia( node.HashToken, node.EndRegionKeyword, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformEndRegionDirectiveTrivia( node );
				default: 
					return base.VisitEndRegionDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformEndRegionDirectiveTrivia( EndRegionDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EndRegionDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndRegionKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEnumDeclaration( EnumDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EnumDeclaration( node.AttributeLists, node.Modifiers, node.EnumKeyword, node.Identifier, node.BaseList, node.OpenBraceToken, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformEnumDeclaration( node );
				default: 
					return base.VisitEnumDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformEnumDeclaration( EnumDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EnumDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EnumKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEnumMemberDeclaration( EnumMemberDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EnumMemberDeclaration( node.AttributeLists, node.Modifiers, node.Identifier, node.EqualsValue);
				case TransformationKind.Transform: 
					return this.TransformEnumMemberDeclaration( node );
				default: 
					return base.VisitEnumMemberDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformEnumMemberDeclaration( EnumMemberDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EnumMemberDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEqualsValueClause( EqualsValueClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EqualsValueClause( node.EqualsToken, node.Value);
				case TransformationKind.Transform: 
					return this.TransformEqualsValueClause( node );
				default: 
					return base.VisitEqualsValueClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformEqualsValueClause( EqualsValueClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EqualsValueClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitErrorDirectiveTrivia( ErrorDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ErrorDirectiveTrivia( node.HashToken, node.ErrorKeyword, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformErrorDirectiveTrivia( node );
				default: 
					return base.VisitErrorDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformErrorDirectiveTrivia( ErrorDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ErrorDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ErrorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEventDeclaration( EventDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EventDeclaration( node.AttributeLists, node.Modifiers, node.EventKeyword, node.Type, node.ExplicitInterfaceSpecifier, node.Identifier, node.AccessorList, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformEventDeclaration( node );
				default: 
					return base.VisitEventDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformEventDeclaration( EventDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EventDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EventKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return EventFieldDeclaration( node.AttributeLists, node.Modifiers, node.EventKeyword, node.Declaration, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformEventFieldDeclaration( node );
				default: 
					return base.VisitEventFieldDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformEventFieldDeclaration( EventFieldDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(EventFieldDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EventKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitExplicitInterfaceSpecifier( ExplicitInterfaceSpecifierSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ExplicitInterfaceSpecifier( node.Name, node.DotToken);
				case TransformationKind.Transform: 
					return this.TransformExplicitInterfaceSpecifier( node );
				default: 
					return base.VisitExplicitInterfaceSpecifier( node );
			}
		}
		protected virtual ExpressionSyntax TransformExplicitInterfaceSpecifier( ExplicitInterfaceSpecifierSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ExplicitInterfaceSpecifier))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitExpressionStatement( ExpressionStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ExpressionStatement( node.AttributeLists, node.Expression, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformExpressionStatement( node );
				default: 
					return base.VisitExpressionStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformExpressionStatement( ExpressionStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ExpressionStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitExternAliasDirective( ExternAliasDirectiveSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ExternAliasDirective( node.ExternKeyword, node.AliasKeyword, node.Identifier, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformExternAliasDirective( node );
				default: 
					return base.VisitExternAliasDirective( node );
			}
		}
		protected virtual ExpressionSyntax TransformExternAliasDirective( ExternAliasDirectiveSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ExternAliasDirective))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ExternKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AliasKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFieldDeclaration( FieldDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FieldDeclaration( node.AttributeLists, node.Modifiers, node.Declaration, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformFieldDeclaration( node );
				default: 
					return base.VisitFieldDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformFieldDeclaration( FieldDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FieldDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFinallyClause( FinallyClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FinallyClause( node.FinallyKeyword, node.Block);
				case TransformationKind.Transform: 
					return this.TransformFinallyClause( node );
				default: 
					return base.VisitFinallyClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformFinallyClause( FinallyClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FinallyClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FinallyKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFixedStatement( FixedStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FixedStatement( node.AttributeLists, node.FixedKeyword, node.OpenParenToken, node.Declaration, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformFixedStatement( node );
				default: 
					return base.VisitFixedStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformFixedStatement( FixedStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FixedStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.FixedKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitForEachStatement( ForEachStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ForEachStatement( node.AttributeLists, node.AwaitKeyword, node.ForEachKeyword, node.OpenParenToken, node.Type, node.Identifier, node.InKeyword, node.Expression, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformForEachStatement( node );
				default: 
					return base.VisitForEachStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformForEachStatement( ForEachStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ForEachStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ForEachKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitForEachVariableStatement( ForEachVariableStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ForEachVariableStatement( node.AttributeLists, node.AwaitKeyword, node.ForEachKeyword, node.OpenParenToken, node.Variable, node.InKeyword, node.Expression, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformForEachVariableStatement( node );
				default: 
					return base.VisitForEachVariableStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformForEachVariableStatement( ForEachVariableStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ForEachVariableStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ForEachKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Variable)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitForStatement( ForStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ForStatement( node.AttributeLists, node.ForKeyword, node.OpenParenToken, node.Declaration, node.Initializers, node.FirstSemicolonToken, node.Condition, node.SecondSemicolonToken, node.Incrementors, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformForStatement( node );
				default: 
					return base.VisitForStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformForStatement( ForStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ForStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ForKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.FirstSemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SecondSemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Incrementors)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFromClause( FromClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FromClause( node.FromKeyword, node.Type, node.Identifier, node.InKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformFromClause( node );
				default: 
					return base.VisitFromClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformFromClause( FromClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FromClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FromKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerCallingConvention( FunctionPointerCallingConventionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerCallingConvention( node.ManagedOrUnmanagedKeyword, node.UnmanagedCallingConventionList);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerCallingConvention( node );
				default: 
					return base.VisitFunctionPointerCallingConvention( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerCallingConvention( FunctionPointerCallingConventionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerCallingConvention))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ManagedOrUnmanagedKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.UnmanagedCallingConventionList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerParameter( FunctionPointerParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerParameter( node.AttributeLists, node.Modifiers, node.Type);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerParameter( node );
				default: 
					return base.VisitFunctionPointerParameter( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerParameter( FunctionPointerParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerParameterList( FunctionPointerParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerParameterList( node.LessThanToken, node.Parameters, node.GreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerParameterList( node );
				default: 
					return base.VisitFunctionPointerParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerParameterList( FunctionPointerParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerType( FunctionPointerTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerType( node.DelegateKeyword, node.AsteriskToken, node.CallingConvention, node.ParameterList);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerType( node );
				default: 
					return base.VisitFunctionPointerType( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerType( FunctionPointerTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.DelegateKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AsteriskToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CallingConvention)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerUnmanagedCallingConvention( FunctionPointerUnmanagedCallingConventionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerUnmanagedCallingConvention( node.Name);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerUnmanagedCallingConvention( node );
				default: 
					return base.VisitFunctionPointerUnmanagedCallingConvention( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerUnmanagedCallingConvention( FunctionPointerUnmanagedCallingConventionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerUnmanagedCallingConvention))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitFunctionPointerUnmanagedCallingConventionList( FunctionPointerUnmanagedCallingConventionListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return FunctionPointerUnmanagedCallingConventionList( node.OpenBracketToken, node.CallingConventions, node.CloseBracketToken);
				case TransformationKind.Transform: 
					return this.TransformFunctionPointerUnmanagedCallingConventionList( node );
				default: 
					return base.VisitFunctionPointerUnmanagedCallingConventionList( node );
			}
		}
		protected virtual ExpressionSyntax TransformFunctionPointerUnmanagedCallingConventionList( FunctionPointerUnmanagedCallingConventionListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(FunctionPointerUnmanagedCallingConventionList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CallingConventions)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitGenericName( GenericNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return GenericName( node.Identifier, node.TypeArgumentList);
				case TransformationKind.Transform: 
					return this.TransformGenericName( node );
				default: 
					return base.VisitGenericName( node );
			}
		}
		protected virtual ExpressionSyntax TransformGenericName( GenericNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(GenericName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitGlobalStatement( GlobalStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return GlobalStatement( node.AttributeLists, node.Modifiers, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformGlobalStatement( node );
				default: 
					return base.VisitGlobalStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformGlobalStatement( GlobalStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(GlobalStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitGotoStatement( GotoStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return GotoStatement( node.Kind(), node.AttributeLists, node.GotoKeyword, node.CaseOrDefaultKeyword, node.Expression, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformGotoStatement( node );
				default: 
					return base.VisitGotoStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformGotoStatement( GotoStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(GotoStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GotoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CaseOrDefaultKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitGroupClause( GroupClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return GroupClause( node.GroupKeyword, node.GroupExpression, node.ByKeyword, node.ByExpression);
				case TransformationKind.Transform: 
					return this.TransformGroupClause( node );
				default: 
					return base.VisitGroupClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformGroupClause( GroupClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(GroupClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.GroupKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GroupExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ByKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ByExpression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCheckedExpression( CheckedExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CheckedExpression( node.Kind(), node.Keyword, node.OpenParenToken, node.Expression, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformCheckedExpression( node );
				default: 
					return base.VisitCheckedExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformCheckedExpression( CheckedExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CheckedExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitCheckedStatement( CheckedStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return CheckedStatement( node.Kind(), node.AttributeLists, node.Keyword, node.Block);
				case TransformationKind.Transform: 
					return this.TransformCheckedStatement( node );
				default: 
					return base.VisitCheckedStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformCheckedStatement( CheckedStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(CheckedStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IdentifierName( node.Identifier);
				case TransformationKind.Transform: 
					return this.TransformIdentifierName( node );
				default: 
					return base.VisitIdentifierName( node );
			}
		}
		protected virtual ExpressionSyntax TransformIdentifierName( IdentifierNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IdentifierName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IfDirectiveTrivia( node.HashToken, node.IfKeyword, node.Condition, node.EndOfDirectiveToken, node.IsActive, node.BranchTaken, node.ConditionValue);
				case TransformationKind.Transform: 
					return this.TransformIfDirectiveTrivia( node );
				default: 
					return base.VisitIfDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformIfDirectiveTrivia( IfDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IfDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BranchTaken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConditionValue)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIfStatement( IfStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IfStatement( node.AttributeLists, node.IfKeyword, node.OpenParenToken, node.Condition, node.CloseParenToken, node.Statement, node.Else);
				case TransformationKind.Transform: 
					return this.TransformIfStatement( node );
				default: 
					return base.VisitIfStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformIfStatement( IfStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IfStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IfKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Else)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitImplicitArrayCreationExpression( ImplicitArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ImplicitArrayCreationExpression( node.NewKeyword, node.OpenBracketToken, node.Commas, node.CloseBracketToken, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformImplicitArrayCreationExpression( node );
				default: 
					return base.VisitImplicitArrayCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformImplicitArrayCreationExpression( ImplicitArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ImplicitArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Commas)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitImplicitElementAccess( ImplicitElementAccessSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ImplicitElementAccess( node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformImplicitElementAccess( node );
				default: 
					return base.VisitImplicitElementAccess( node );
			}
		}
		protected virtual ExpressionSyntax TransformImplicitElementAccess( ImplicitElementAccessSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ImplicitElementAccess))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ImplicitObjectCreationExpression( node.NewKeyword, node.ArgumentList, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformImplicitObjectCreationExpression( node );
				default: 
					return base.VisitImplicitObjectCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ImplicitObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitImplicitStackAllocArrayCreationExpression( ImplicitStackAllocArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ImplicitStackAllocArrayCreationExpression( node.StackAllocKeyword, node.OpenBracketToken, node.CloseBracketToken, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformImplicitStackAllocArrayCreationExpression( node );
				default: 
					return base.VisitImplicitStackAllocArrayCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformImplicitStackAllocArrayCreationExpression( ImplicitStackAllocArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ImplicitStackAllocArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StackAllocKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBracketToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIncompleteMember( IncompleteMemberSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IncompleteMember( node.AttributeLists, node.Modifiers, node.Type);
				case TransformationKind.Transform: 
					return this.TransformIncompleteMember( node );
				default: 
					return base.VisitIncompleteMember( node );
			}
		}
		protected virtual ExpressionSyntax TransformIncompleteMember( IncompleteMemberSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IncompleteMember))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIndexerDeclaration( IndexerDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IndexerDeclaration( node.AttributeLists, node.Modifiers, node.Type, node.ExplicitInterfaceSpecifier, node.ThisKeyword, node.ParameterList, node.AccessorList, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformIndexerDeclaration( node );
				default: 
					return base.VisitIndexerDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformIndexerDeclaration( IndexerDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IndexerDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ThisKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIndexerMemberCref( IndexerMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IndexerMemberCref( node.ThisKeyword, node.Parameters);
				case TransformationKind.Transform: 
					return this.TransformIndexerMemberCref( node );
				default: 
					return base.VisitIndexerMemberCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformIndexerMemberCref( IndexerMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IndexerMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ThisKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInitializerExpression( InitializerExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InitializerExpression( node.Kind(), node.OpenBraceToken, node.Expressions, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformInitializerExpression( node );
				default: 
					return base.VisitInitializerExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformInitializerExpression( InitializerExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InitializerExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expressions)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InterfaceDeclaration( node.AttributeLists, node.Modifiers, node.Keyword, node.Identifier, node.TypeParameterList, node.BaseList, node.ConstraintClauses, node.OpenBraceToken, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformInterfaceDeclaration( node );
				default: 
					return base.VisitInterfaceDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterfaceDeclaration( InterfaceDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InterfaceDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterpolatedStringExpression( InterpolatedStringExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InterpolatedStringExpression( node.StringStartToken, node.Contents, node.StringEndToken);
				case TransformationKind.Transform: 
					return this.TransformInterpolatedStringExpression( node );
				default: 
					return base.VisitInterpolatedStringExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterpolatedStringExpression( InterpolatedStringExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InterpolatedStringExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StringStartToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Contents)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.StringEndToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterpolatedStringText( InterpolatedStringTextSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InterpolatedStringText( node.TextToken);
				case TransformationKind.Transform: 
					return this.TransformInterpolatedStringText( node );
				default: 
					return base.VisitInterpolatedStringText( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterpolatedStringText( InterpolatedStringTextSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InterpolatedStringText))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.TextToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterpolation( InterpolationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Interpolation( node.OpenBraceToken, node.Expression, node.AlignmentClause, node.FormatClause, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformInterpolation( node );
				default: 
					return base.VisitInterpolation( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterpolation( InterpolationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Interpolation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AlignmentClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.FormatClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterpolationAlignmentClause( InterpolationAlignmentClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InterpolationAlignmentClause( node.CommaToken, node.Value);
				case TransformationKind.Transform: 
					return this.TransformInterpolationAlignmentClause( node );
				default: 
					return base.VisitInterpolationAlignmentClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterpolationAlignmentClause( InterpolationAlignmentClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InterpolationAlignmentClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.CommaToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Value)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInterpolationFormatClause( InterpolationFormatClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InterpolationFormatClause( node.ColonToken, node.FormatStringToken);
				case TransformationKind.Transform: 
					return this.TransformInterpolationFormatClause( node );
				default: 
					return base.VisitInterpolationFormatClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformInterpolationFormatClause( InterpolationFormatClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InterpolationFormatClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.FormatStringToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitInvocationExpression( InvocationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return InvocationExpression( node.Expression, node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformInvocationExpression( node );
				default: 
					return base.VisitInvocationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformInvocationExpression( InvocationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(InvocationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitIsPatternExpression( IsPatternExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return IsPatternExpression( node.Expression, node.IsKeyword, node.Pattern);
				case TransformationKind.Transform: 
					return this.TransformIsPatternExpression( node );
				default: 
					return base.VisitIsPatternExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformIsPatternExpression( IsPatternExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(IsPatternExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitJoinClause( JoinClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return JoinClause( node.JoinKeyword, node.Type, node.Identifier, node.InKeyword, node.InExpression, node.OnKeyword, node.LeftExpression, node.EqualsKeyword, node.RightExpression, node.Into);
				case TransformationKind.Transform: 
					return this.TransformJoinClause( node );
				default: 
					return base.VisitJoinClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformJoinClause( JoinClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(JoinClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.JoinKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.InKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.InExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OnKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.LeftExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.RightExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Into)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitJoinIntoClause( JoinIntoClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return JoinIntoClause( node.IntoKeyword, node.Identifier);
				case TransformationKind.Transform: 
					return this.TransformJoinIntoClause( node );
				default: 
					return base.VisitJoinIntoClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformJoinIntoClause( JoinIntoClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(JoinIntoClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.IntoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLabeledStatement( LabeledStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LabeledStatement( node.AttributeLists, node.Identifier, node.ColonToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformLabeledStatement( node );
				default: 
					return base.VisitLabeledStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformLabeledStatement( LabeledStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LabeledStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLetClause( LetClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LetClause( node.LetKeyword, node.Identifier, node.EqualsToken, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformLetClause( node );
				default: 
					return base.VisitLetClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformLetClause( LetClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LetClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LetKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLineDirectiveTrivia( LineDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LineDirectiveTrivia( node.HashToken, node.LineKeyword, node.Line, node.File, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformLineDirectiveTrivia( node );
				default: 
					return base.VisitLineDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformLineDirectiveTrivia( LineDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LineDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.LineKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Line)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLiteralExpression( LiteralExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LiteralExpression( node.Kind(), node.Token);
				case TransformationKind.Transform: 
					return this.TransformLiteralExpression( node );
				default: 
					return base.VisitLiteralExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformLiteralExpression( LiteralExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LiteralExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLoadDirectiveTrivia( LoadDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LoadDirectiveTrivia( node.HashToken, node.LoadKeyword, node.File, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformLoadDirectiveTrivia( node );
				default: 
					return base.VisitLoadDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformLoadDirectiveTrivia( LoadDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LoadDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.LoadKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LocalDeclarationStatement( node.AttributeLists, node.AwaitKeyword, node.UsingKeyword, node.Modifiers, node.Declaration, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformLocalDeclarationStatement( node );
				default: 
					return base.VisitLocalDeclarationStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformLocalDeclarationStatement( LocalDeclarationStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LocalDeclarationStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LocalFunctionStatement( node.AttributeLists, node.Modifiers, node.ReturnType, node.Identifier, node.TypeParameterList, node.ParameterList, node.ConstraintClauses, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformLocalFunctionStatement( node );
				default: 
					return base.VisitLocalFunctionStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformLocalFunctionStatement( LocalFunctionStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LocalFunctionStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitLockStatement( LockStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return LockStatement( node.AttributeLists, node.LockKeyword, node.OpenParenToken, node.Expression, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformLockStatement( node );
				default: 
					return base.VisitLockStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformLockStatement( LockStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(LockStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.LockKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitMakeRefExpression( MakeRefExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return MakeRefExpression( node.Keyword, node.OpenParenToken, node.Expression, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformMakeRefExpression( node );
				default: 
					return base.VisitMakeRefExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformMakeRefExpression( MakeRefExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(MakeRefExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return MemberAccessExpression( node.Kind(), node.Expression, node.OperatorToken, node.Name);
				case TransformationKind.Transform: 
					return this.TransformMemberAccessExpression( node );
				default: 
					return base.VisitMemberAccessExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformMemberAccessExpression( MemberAccessExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(MemberAccessExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return MemberBindingExpression( node.OperatorToken, node.Name);
				case TransformationKind.Transform: 
					return this.TransformMemberBindingExpression( node );
				default: 
					return base.VisitMemberBindingExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformMemberBindingExpression( MemberBindingExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(MemberBindingExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return MethodDeclaration( node.AttributeLists, node.Modifiers, node.ReturnType, node.ExplicitInterfaceSpecifier, node.Identifier, node.TypeParameterList, node.ParameterList, node.ConstraintClauses, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformMethodDeclaration( node );
				default: 
					return base.VisitMethodDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformMethodDeclaration( MethodDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(MethodDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNameColon( NameColonSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NameColon( node.Name, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformNameColon( node );
				default: 
					return base.VisitNameColon( node );
			}
		}
		protected virtual ExpressionSyntax TransformNameColon( NameColonSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NameColon))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNameEquals( NameEqualsSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NameEquals( node.Name, node.EqualsToken);
				case TransformationKind.Transform: 
					return this.TransformNameEquals( node );
				default: 
					return base.VisitNameEquals( node );
			}
		}
		protected virtual ExpressionSyntax TransformNameEquals( NameEqualsSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NameEquals))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNameMemberCref( NameMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NameMemberCref( node.Name, node.Parameters);
				case TransformationKind.Transform: 
					return this.TransformNameMemberCref( node );
				default: 
					return base.VisitNameMemberCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformNameMemberCref( NameMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NameMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NamespaceDeclaration( node.AttributeLists, node.Modifiers, node.NamespaceKeyword, node.Name, node.OpenBraceToken, node.Externs, node.Usings, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformNamespaceDeclaration( node );
				default: 
					return base.VisitNamespaceDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformNamespaceDeclaration( NamespaceDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NamespaceDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.NamespaceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Externs)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Usings)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNullableDirectiveTrivia( NullableDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NullableDirectiveTrivia( node.HashToken, node.NullableKeyword, node.SettingToken, node.TargetToken, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformNullableDirectiveTrivia( node );
				default: 
					return base.VisitNullableDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformNullableDirectiveTrivia( NullableDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NullableDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.NullableKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SettingToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TargetToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitNullableType( NullableTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return NullableType( node.ElementType, node.QuestionToken);
				case TransformationKind.Transform: 
					return this.TransformNullableType( node );
				default: 
					return base.VisitNullableType( node );
			}
		}
		protected virtual ExpressionSyntax TransformNullableType( NullableTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(NullableType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.QuestionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ObjectCreationExpression( node.NewKeyword, node.Type, node.ArgumentList, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformObjectCreationExpression( node );
				default: 
					return base.VisitObjectCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformObjectCreationExpression( ObjectCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ObjectCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NewKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOmittedArraySizeExpression( OmittedArraySizeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return OmittedArraySizeExpression( node.OmittedArraySizeExpressionToken);
				case TransformationKind.Transform: 
					return this.TransformOmittedArraySizeExpression( node );
				default: 
					return base.VisitOmittedArraySizeExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformOmittedArraySizeExpression( OmittedArraySizeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(OmittedArraySizeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OmittedArraySizeExpressionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOmittedTypeArgument( OmittedTypeArgumentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return OmittedTypeArgument( node.OmittedTypeArgumentToken);
				case TransformationKind.Transform: 
					return this.TransformOmittedTypeArgument( node );
				default: 
					return base.VisitOmittedTypeArgument( node );
			}
		}
		protected virtual ExpressionSyntax TransformOmittedTypeArgument( OmittedTypeArgumentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(OmittedTypeArgument))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OmittedTypeArgumentToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOperatorDeclaration( OperatorDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return OperatorDeclaration( node.AttributeLists, node.Modifiers, node.ReturnType, node.OperatorKeyword, node.OperatorToken, node.ParameterList, node.Body, node.ExpressionBody, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformOperatorDeclaration( node );
				default: 
					return base.VisitOperatorDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformOperatorDeclaration( OperatorDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(OperatorDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOperatorMemberCref( OperatorMemberCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return OperatorMemberCref( node.OperatorKeyword, node.OperatorToken, node.Parameters);
				case TransformationKind.Transform: 
					return this.TransformOperatorMemberCref( node );
				default: 
					return base.VisitOperatorMemberCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformOperatorMemberCref( OperatorMemberCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(OperatorMemberCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOrderByClause( OrderByClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return OrderByClause( node.OrderByKeyword, node.Orderings);
				case TransformationKind.Transform: 
					return this.TransformOrderByClause( node );
				default: 
					return base.VisitOrderByClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformOrderByClause( OrderByClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(OrderByClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OrderByKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Orderings)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitOrdering( OrderingSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Ordering( node.Kind(), node.Expression, node.AscendingOrDescendingKeyword);
				case TransformationKind.Transform: 
					return this.TransformOrdering( node );
				default: 
					return base.VisitOrdering( node );
			}
		}
		protected virtual ExpressionSyntax TransformOrdering( OrderingSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Ordering))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AscendingOrDescendingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParameter( ParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Parameter( node.AttributeLists, node.Modifiers, node.Type, node.Identifier, node.Default);
				case TransformationKind.Transform: 
					return this.TransformParameter( node );
				default: 
					return base.VisitParameter( node );
			}
		}
		protected virtual ExpressionSyntax TransformParameter( ParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Parameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Default)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParameterList( ParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ParameterList( node.OpenParenToken, node.Parameters, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformParameterList( node );
				default: 
					return base.VisitParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformParameterList( ParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParenthesizedExpression( ParenthesizedExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ParenthesizedExpression( node.OpenParenToken, node.Expression, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformParenthesizedExpression( node );
				default: 
					return base.VisitParenthesizedExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformParenthesizedExpression( ParenthesizedExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ParenthesizedExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ParenthesizedLambdaExpression( node.AsyncKeyword, node.ParameterList, node.ArrowToken, node.Block, node.ExpressionBody);
				case TransformationKind.Transform: 
					return this.TransformParenthesizedLambdaExpression( node );
				default: 
					return base.VisitParenthesizedLambdaExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformParenthesizedLambdaExpression( ParenthesizedLambdaExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ParenthesizedLambdaExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AsyncKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParenthesizedPattern( ParenthesizedPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ParenthesizedPattern( node.OpenParenToken, node.Pattern, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformParenthesizedPattern( node );
				default: 
					return base.VisitParenthesizedPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformParenthesizedPattern( ParenthesizedPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ParenthesizedPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitParenthesizedVariableDesignation( ParenthesizedVariableDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ParenthesizedVariableDesignation( node.OpenParenToken, node.Variables, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformParenthesizedVariableDesignation( node );
				default: 
					return base.VisitParenthesizedVariableDesignation( node );
			}
		}
		protected virtual ExpressionSyntax TransformParenthesizedVariableDesignation( ParenthesizedVariableDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ParenthesizedVariableDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Variables)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPointerType( PointerTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PointerType( node.ElementType, node.AsteriskToken);
				case TransformationKind.Transform: 
					return this.TransformPointerType( node );
				default: 
					return base.VisitPointerType( node );
			}
		}
		protected virtual ExpressionSyntax TransformPointerType( PointerTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PointerType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ElementType)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AsteriskToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPositionalPatternClause( PositionalPatternClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PositionalPatternClause( node.OpenParenToken, node.Subpatterns, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformPositionalPatternClause( node );
				default: 
					return base.VisitPositionalPatternClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformPositionalPatternClause( PositionalPatternClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PositionalPatternClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Subpatterns)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPostfixUnaryExpression( PostfixUnaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PostfixUnaryExpression( node.Kind(), node.Operand, node.OperatorToken);
				case TransformationKind.Transform: 
					return this.TransformPostfixUnaryExpression( node );
				default: 
					return base.VisitPostfixUnaryExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformPostfixUnaryExpression( PostfixUnaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PostfixUnaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Operand)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPragmaChecksumDirectiveTrivia( PragmaChecksumDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PragmaChecksumDirectiveTrivia( node.HashToken, node.PragmaKeyword, node.ChecksumKeyword, node.File, node.Guid, node.Bytes, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformPragmaChecksumDirectiveTrivia( node );
				default: 
					return base.VisitPragmaChecksumDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformPragmaChecksumDirectiveTrivia( PragmaChecksumDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PragmaChecksumDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.PragmaKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ChecksumKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Guid)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Bytes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PragmaWarningDirectiveTrivia( node.HashToken, node.PragmaKeyword, node.WarningKeyword, node.DisableOrRestoreKeyword, node.ErrorCodes, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformPragmaWarningDirectiveTrivia( node );
				default: 
					return base.VisitPragmaWarningDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PragmaWarningDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.PragmaKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WarningKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DisableOrRestoreKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ErrorCodes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPredefinedType( PredefinedTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PredefinedType( node.Keyword);
				case TransformationKind.Transform: 
					return this.TransformPredefinedType( node );
				default: 
					return base.VisitPredefinedType( node );
			}
		}
		protected virtual ExpressionSyntax TransformPredefinedType( PredefinedTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PredefinedType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPrefixUnaryExpression( PrefixUnaryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PrefixUnaryExpression( node.Kind(), node.OperatorToken, node.Operand);
				case TransformationKind.Transform: 
					return this.TransformPrefixUnaryExpression( node );
				default: 
					return base.VisitPrefixUnaryExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformPrefixUnaryExpression( PrefixUnaryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PrefixUnaryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Operand)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PrimaryConstructorBaseType( node.Type, node.ArgumentList);
				case TransformationKind.Transform: 
					return this.TransformPrimaryConstructorBaseType( node );
				default: 
					return base.VisitPrimaryConstructorBaseType( node );
			}
		}
		protected virtual ExpressionSyntax TransformPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PrimaryConstructorBaseType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPropertyDeclaration( PropertyDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PropertyDeclaration( node.AttributeLists, node.Modifiers, node.Type, node.ExplicitInterfaceSpecifier, node.Identifier, node.AccessorList, node.ExpressionBody, node.Initializer, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformPropertyDeclaration( node );
				default: 
					return base.VisitPropertyDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformPropertyDeclaration( PropertyDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PropertyDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExplicitInterfaceSpecifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AccessorList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitPropertyPatternClause( PropertyPatternClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return PropertyPatternClause( node.OpenBraceToken, node.Subpatterns, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformPropertyPatternClause( node );
				default: 
					return base.VisitPropertyPatternClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformPropertyPatternClause( PropertyPatternClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(PropertyPatternClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Subpatterns)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitQualifiedCref( QualifiedCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return QualifiedCref( node.Container, node.DotToken, node.Member);
				case TransformationKind.Transform: 
					return this.TransformQualifiedCref( node );
				default: 
					return base.VisitQualifiedCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformQualifiedCref( QualifiedCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(QualifiedCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Container)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Member)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitQualifiedName( QualifiedNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return QualifiedName( node.Left, node.DotToken, node.Right);
				case TransformationKind.Transform: 
					return this.TransformQualifiedName( node );
				default: 
					return base.VisitQualifiedName( node );
			}
		}
		protected virtual ExpressionSyntax TransformQualifiedName( QualifiedNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(QualifiedName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Left)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.DotToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Right)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitQueryBody( QueryBodySyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return QueryBody( node.Clauses, node.SelectOrGroup, node.Continuation);
				case TransformationKind.Transform: 
					return this.TransformQueryBody( node );
				default: 
					return base.VisitQueryBody( node );
			}
		}
		protected virtual ExpressionSyntax TransformQueryBody( QueryBodySyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(QueryBody))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Clauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SelectOrGroup)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Continuation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitQueryContinuation( QueryContinuationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return QueryContinuation( node.IntoKeyword, node.Identifier, node.Body);
				case TransformationKind.Transform: 
					return this.TransformQueryContinuation( node );
				default: 
					return base.VisitQueryContinuation( node );
			}
		}
		protected virtual ExpressionSyntax TransformQueryContinuation( QueryContinuationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(QueryContinuation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.IntoKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitQueryExpression( QueryExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return QueryExpression( node.FromClause, node.Body);
				case TransformationKind.Transform: 
					return this.TransformQueryExpression( node );
				default: 
					return base.VisitQueryExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformQueryExpression( QueryExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(QueryExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.FromClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Body)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRangeExpression( RangeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RangeExpression( node.LeftOperand, node.OperatorToken, node.RightOperand);
				case TransformationKind.Transform: 
					return this.TransformRangeExpression( node );
				default: 
					return base.VisitRangeExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformRangeExpression( RangeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RangeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LeftOperand)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.RightOperand)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RecordDeclaration( node.AttributeLists, node.Modifiers, node.Keyword, node.Identifier, node.TypeParameterList, node.ParameterList, node.BaseList, node.ConstraintClauses, node.OpenBraceToken, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformRecordDeclaration( node );
				default: 
					return base.VisitRecordDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformRecordDeclaration( RecordDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RecordDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRecursivePattern( RecursivePatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RecursivePattern( node.Type, node.PositionalPatternClause, node.PropertyPatternClause, node.Designation);
				case TransformationKind.Transform: 
					return this.TransformRecursivePattern( node );
				default: 
					return base.VisitRecursivePattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformRecursivePattern( RecursivePatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RecursivePattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.PositionalPatternClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.PropertyPatternClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitReferenceDirectiveTrivia( ReferenceDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ReferenceDirectiveTrivia( node.HashToken, node.ReferenceKeyword, node.File, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformReferenceDirectiveTrivia( node );
				default: 
					return base.VisitReferenceDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformReferenceDirectiveTrivia( ReferenceDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ReferenceDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReferenceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.File)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRefExpression( RefExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RefExpression( node.RefKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformRefExpression( node );
				default: 
					return base.VisitRefExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformRefExpression( RefExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RefExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRefType( RefTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RefType( node.RefKeyword, node.ReadOnlyKeyword, node.Type);
				case TransformationKind.Transform: 
					return this.TransformRefType( node );
				default: 
					return base.VisitRefType( node );
			}
		}
		protected virtual ExpressionSyntax TransformRefType( RefTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RefType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.RefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReadOnlyKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRefTypeExpression( RefTypeExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RefTypeExpression( node.Keyword, node.OpenParenToken, node.Expression, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformRefTypeExpression( node );
				default: 
					return base.VisitRefTypeExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformRefTypeExpression( RefTypeExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RefTypeExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRefValueExpression( RefValueExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RefValueExpression( node.Keyword, node.OpenParenToken, node.Expression, node.Comma, node.Type, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformRefValueExpression( node );
				default: 
					return base.VisitRefValueExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformRefValueExpression( RefValueExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RefValueExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Comma)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRegionDirectiveTrivia( RegionDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RegionDirectiveTrivia( node.HashToken, node.RegionKeyword, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformRegionDirectiveTrivia( node );
				default: 
					return base.VisitRegionDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformRegionDirectiveTrivia( RegionDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RegionDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.RegionKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitRelationalPattern( RelationalPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return RelationalPattern( node.OperatorToken, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformRelationalPattern( node );
				default: 
					return base.VisitRelationalPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformRelationalPattern( RelationalPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(RelationalPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitReturnStatement( ReturnStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ReturnStatement( node.AttributeLists, node.ReturnKeyword, node.Expression, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformReturnStatement( node );
				default: 
					return base.VisitReturnStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformReturnStatement( ReturnStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ReturnStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSelectClause( SelectClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SelectClause( node.SelectKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformSelectClause( node );
				default: 
					return base.VisitSelectClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformSelectClause( SelectClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SelectClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.SelectKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitShebangDirectiveTrivia( ShebangDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ShebangDirectiveTrivia( node.HashToken, node.ExclamationToken, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformShebangDirectiveTrivia( node );
				default: 
					return base.VisitShebangDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformShebangDirectiveTrivia( ShebangDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ShebangDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExclamationToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSimpleBaseType( SimpleBaseTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SimpleBaseType( node.Type);
				case TransformationKind.Transform: 
					return this.TransformSimpleBaseType( node );
				default: 
					return base.VisitSimpleBaseType( node );
			}
		}
		protected virtual ExpressionSyntax TransformSimpleBaseType( SimpleBaseTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SimpleBaseType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSimpleLambdaExpression( SimpleLambdaExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SimpleLambdaExpression( node.AsyncKeyword, node.Parameter, node.ArrowToken, node.Block, node.ExpressionBody);
				case TransformationKind.Transform: 
					return this.TransformSimpleLambdaExpression( node );
				default: 
					return base.VisitSimpleLambdaExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformSimpleLambdaExpression( SimpleLambdaExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SimpleLambdaExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AsyncKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameter)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArrowToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ExpressionBody)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSingleVariableDesignation( SingleVariableDesignationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SingleVariableDesignation( node.Identifier);
				case TransformationKind.Transform: 
					return this.TransformSingleVariableDesignation( node );
				default: 
					return base.VisitSingleVariableDesignation( node );
			}
		}
		protected virtual ExpressionSyntax TransformSingleVariableDesignation( SingleVariableDesignationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SingleVariableDesignation))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSizeOfExpression( SizeOfExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SizeOfExpression( node.Keyword, node.OpenParenToken, node.Type, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformSizeOfExpression( node );
				default: 
					return base.VisitSizeOfExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformSizeOfExpression( SizeOfExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SizeOfExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSkippedTokensTrivia( SkippedTokensTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SkippedTokensTrivia( node.Tokens);
				case TransformationKind.Transform: 
					return this.TransformSkippedTokensTrivia( node );
				default: 
					return base.VisitSkippedTokensTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformSkippedTokensTrivia( SkippedTokensTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SkippedTokensTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Tokens)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitStackAllocArrayCreationExpression( StackAllocArrayCreationExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return StackAllocArrayCreationExpression( node.StackAllocKeyword, node.Type, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformStackAllocArrayCreationExpression( node );
				default: 
					return base.VisitStackAllocArrayCreationExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformStackAllocArrayCreationExpression( StackAllocArrayCreationExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(StackAllocArrayCreationExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StackAllocKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return StructDeclaration( node.AttributeLists, node.Modifiers, node.Keyword, node.Identifier, node.TypeParameterList, node.BaseList, node.ConstraintClauses, node.OpenBraceToken, node.Members, node.CloseBraceToken, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformStructDeclaration( node );
				default: 
					return base.VisitStructDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformStructDeclaration( StructDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(StructDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Modifiers)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TypeParameterList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.BaseList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ConstraintClauses)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Members)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSubpattern( SubpatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return Subpattern( node.NameColon, node.Pattern);
				case TransformationKind.Transform: 
					return this.TransformSubpattern( node );
				default: 
					return base.VisitSubpattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformSubpattern( SubpatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(Subpattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.NameColon)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSwitchExpression( SwitchExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SwitchExpression( node.GoverningExpression, node.SwitchKeyword, node.OpenBraceToken, node.Arms, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformSwitchExpression( node );
				default: 
					return base.VisitSwitchExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformSwitchExpression( SwitchExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SwitchExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.GoverningExpression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SwitchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arms)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSwitchExpressionArm( SwitchExpressionArmSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SwitchExpressionArm( node.Pattern, node.WhenClause, node.EqualsGreaterThanToken, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformSwitchExpressionArm( node );
				default: 
					return base.VisitSwitchExpressionArm( node );
			}
		}
		protected virtual ExpressionSyntax TransformSwitchExpressionArm( SwitchExpressionArmSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SwitchExpressionArm))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhenClause)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSwitchSection( SwitchSectionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SwitchSection( node.Labels, node.Statements);
				case TransformationKind.Transform: 
					return this.TransformSwitchSection( node );
				default: 
					return base.VisitSwitchSection( node );
			}
		}
		protected virtual ExpressionSyntax TransformSwitchSection( SwitchSectionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SwitchSection))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Labels)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statements)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitSwitchStatement( SwitchStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return SwitchStatement( node.AttributeLists, node.SwitchKeyword, node.OpenParenToken, node.Expression, node.CloseParenToken, node.OpenBraceToken, node.Sections, node.CloseBraceToken);
				case TransformationKind.Transform: 
					return this.TransformSwitchStatement( node );
				default: 
					return base.VisitSwitchStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformSwitchStatement( SwitchStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(SwitchStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SwitchKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Sections)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseBraceToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitThisExpression( ThisExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ThisExpression( node.Token);
				case TransformationKind.Transform: 
					return this.TransformThisExpression( node );
				default: 
					return base.VisitThisExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformThisExpression( ThisExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ThisExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Token)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitThrowExpression( ThrowExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ThrowExpression( node.ThrowKeyword, node.Expression);
				case TransformationKind.Transform: 
					return this.TransformThrowExpression( node );
				default: 
					return base.VisitThrowExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformThrowExpression( ThrowExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ThrowExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.ThrowKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitThrowStatement( ThrowStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return ThrowStatement( node.AttributeLists, node.ThrowKeyword, node.Expression, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformThrowStatement( node );
				default: 
					return base.VisitThrowStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformThrowStatement( ThrowStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(ThrowStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ThrowKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTryStatement( TryStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TryStatement( node.AttributeLists, node.TryKeyword, node.Block, node.Catches, node.Finally);
				case TransformationKind.Transform: 
					return this.TransformTryStatement( node );
				default: 
					return base.VisitTryStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformTryStatement( TryStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TryStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TryKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Catches)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Finally)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTupleElement( TupleElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TupleElement( node.Type, node.Identifier);
				case TransformationKind.Transform: 
					return this.TransformTupleElement( node );
				default: 
					return base.VisitTupleElement( node );
			}
		}
		protected virtual ExpressionSyntax TransformTupleElement( TupleElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TupleElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTupleExpression( TupleExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TupleExpression( node.OpenParenToken, node.Arguments, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformTupleExpression( node );
				default: 
					return base.VisitTupleExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformTupleExpression( TupleExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TupleExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTupleType( TupleTypeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TupleType( node.OpenParenToken, node.Elements, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformTupleType( node );
				default: 
					return base.VisitTupleType( node );
			}
		}
		protected virtual ExpressionSyntax TransformTupleType( TupleTypeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TupleType))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Elements)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeArgumentList( TypeArgumentListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeArgumentList( node.LessThanToken, node.Arguments, node.GreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformTypeArgumentList( node );
				default: 
					return base.VisitTypeArgumentList( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeArgumentList( TypeArgumentListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeArgumentList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Arguments)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeConstraint( TypeConstraintSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeConstraint( node.Type);
				case TransformationKind.Transform: 
					return this.TransformTypeConstraint( node );
				default: 
					return base.VisitTypeConstraint( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeConstraint( TypeConstraintSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeConstraint))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeCref( TypeCrefSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeCref( node.Type);
				case TransformationKind.Transform: 
					return this.TransformTypeCref( node );
				default: 
					return base.VisitTypeCref( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeCref( TypeCrefSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeCref))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeOfExpression( TypeOfExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeOfExpression( node.Keyword, node.OpenParenToken, node.Type, node.CloseParenToken);
				case TransformationKind.Transform: 
					return this.TransformTypeOfExpression( node );
				default: 
					return base.VisitTypeOfExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeOfExpression( TypeOfExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeOfExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Keyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeParameter( TypeParameterSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeParameter( node.AttributeLists, node.VarianceKeyword, node.Identifier);
				case TransformationKind.Transform: 
					return this.TransformTypeParameter( node );
				default: 
					return base.VisitTypeParameter( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeParameter( TypeParameterSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeParameter))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.VarianceKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeParameterConstraintClause( TypeParameterConstraintClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeParameterConstraintClause( node.WhereKeyword, node.Name, node.ColonToken, node.Constraints);
				case TransformationKind.Transform: 
					return this.TransformTypeParameterConstraintClause( node );
				default: 
					return base.VisitTypeParameterConstraintClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeParameterConstraintClause( TypeParameterConstraintClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeParameterConstraintClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhereKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Constraints)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypeParameterList( TypeParameterListSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypeParameterList( node.LessThanToken, node.Parameters, node.GreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformTypeParameterList( node );
				default: 
					return base.VisitTypeParameterList( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypeParameterList( TypeParameterListSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypeParameterList))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Parameters)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitTypePattern( TypePatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return TypePattern( node.Type);
				case TransformationKind.Transform: 
					return this.TransformTypePattern( node );
				default: 
					return base.VisitTypePattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformTypePattern( TypePatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(TypePattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitUnaryPattern( UnaryPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return UnaryPattern( node.OperatorToken, node.Pattern);
				case TransformationKind.Transform: 
					return this.TransformUnaryPattern( node );
				default: 
					return base.VisitUnaryPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformUnaryPattern( UnaryPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(UnaryPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.OperatorToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Pattern)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitUndefDirectiveTrivia( UndefDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return UndefDirectiveTrivia( node.HashToken, node.UndefKeyword, node.Name, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformUndefDirectiveTrivia( node );
				default: 
					return base.VisitUndefDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformUndefDirectiveTrivia( UndefDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(UndefDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.UndefKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitUnsafeStatement( UnsafeStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return UnsafeStatement( node.AttributeLists, node.UnsafeKeyword, node.Block);
				case TransformationKind.Transform: 
					return this.TransformUnsafeStatement( node );
				default: 
					return base.VisitUnsafeStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformUnsafeStatement( UnsafeStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(UnsafeStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.UnsafeKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Block)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitUsingDirective( UsingDirectiveSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return UsingDirective( node.UsingKeyword, node.StaticKeyword, node.Alias, node.Name, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformUsingDirective( node );
				default: 
					return base.VisitUsingDirective( node );
			}
		}
		protected virtual ExpressionSyntax TransformUsingDirective( UsingDirectiveSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(UsingDirective))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.StaticKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Alias)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitUsingStatement( UsingStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return UsingStatement( node.AttributeLists, node.AwaitKeyword, node.UsingKeyword, node.OpenParenToken, node.Declaration, node.Expression, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformUsingStatement( node );
				default: 
					return base.VisitUsingStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformUsingStatement( UsingStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(UsingStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AwaitKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.UsingKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Declaration)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitVariableDeclaration( VariableDeclarationSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return VariableDeclaration( node.Type, node.Variables);
				case TransformationKind.Transform: 
					return this.TransformVariableDeclaration( node );
				default: 
					return base.VisitVariableDeclaration( node );
			}
		}
		protected virtual ExpressionSyntax TransformVariableDeclaration( VariableDeclarationSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(VariableDeclaration))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Type)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Variables)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitVariableDeclarator( VariableDeclaratorSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return VariableDeclarator( node.Identifier, node.ArgumentList, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformVariableDeclarator( node );
				default: 
					return base.VisitVariableDeclarator( node );
			}
		}
		protected virtual ExpressionSyntax TransformVariableDeclarator( VariableDeclaratorSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(VariableDeclarator))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ArgumentList)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitVarPattern( VarPatternSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return VarPattern( node.VarKeyword, node.Designation);
				case TransformationKind.Transform: 
					return this.TransformVarPattern( node );
				default: 
					return base.VisitVarPattern( node );
			}
		}
		protected virtual ExpressionSyntax TransformVarPattern( VarPatternSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(VarPattern))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.VarKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Designation)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitWarningDirectiveTrivia( WarningDirectiveTriviaSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return WarningDirectiveTrivia( node.HashToken, node.WarningKeyword, node.EndOfDirectiveToken, node.IsActive);
				case TransformationKind.Transform: 
					return this.TransformWarningDirectiveTrivia( node );
				default: 
					return base.VisitWarningDirectiveTrivia( node );
			}
		}
		protected virtual ExpressionSyntax TransformWarningDirectiveTrivia( WarningDirectiveTriviaSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(WarningDirectiveTrivia))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.HashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WarningKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndOfDirectiveToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.IsActive)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitWhenClause( WhenClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return WhenClause( node.WhenKeyword, node.Condition);
				case TransformationKind.Transform: 
					return this.TransformWhenClause( node );
				default: 
					return base.VisitWhenClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformWhenClause( WhenClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(WhenClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhenKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitWhereClause( WhereClauseSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return WhereClause( node.WhereKeyword, node.Condition);
				case TransformationKind.Transform: 
					return this.TransformWhereClause( node );
				default: 
					return base.VisitWhereClause( node );
			}
		}
		protected virtual ExpressionSyntax TransformWhereClause( WhereClauseSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(WhereClause))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.WhereKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitWhileStatement( WhileStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return WhileStatement( node.AttributeLists, node.WhileKeyword, node.OpenParenToken, node.Condition, node.CloseParenToken, node.Statement);
				case TransformationKind.Transform: 
					return this.TransformWhileStatement( node );
				default: 
					return base.VisitWhileStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformWhileStatement( WhileStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(WhileStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WhileKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.OpenParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Condition)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.CloseParenToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Statement)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitWithExpression( WithExpressionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return WithExpression( node.Expression, node.WithKeyword, node.Initializer);
				case TransformationKind.Transform: 
					return this.TransformWithExpression( node );
				default: 
					return base.VisitWithExpression( node );
			}
		}
		protected virtual ExpressionSyntax TransformWithExpression( WithExpressionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(WithExpression))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.WithKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Initializer)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlCDataSection( XmlCDataSectionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlCDataSection( node.StartCDataToken, node.TextTokens, node.EndCDataToken);
				case TransformationKind.Transform: 
					return this.TransformXmlCDataSection( node );
				default: 
					return base.VisitXmlCDataSection( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlCDataSection( XmlCDataSectionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlCDataSection))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartCDataToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndCDataToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlComment( XmlCommentSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlComment( node.LessThanExclamationMinusMinusToken, node.TextTokens, node.MinusMinusGreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformXmlComment( node );
				default: 
					return base.VisitXmlComment( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlComment( XmlCommentSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlComment))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanExclamationMinusMinusToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.MinusMinusGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlCrefAttribute( XmlCrefAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlCrefAttribute( node.Name, node.EqualsToken, node.StartQuoteToken, node.Cref, node.EndQuoteToken);
				case TransformationKind.Transform: 
					return this.TransformXmlCrefAttribute( node );
				default: 
					return base.VisitXmlCrefAttribute( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlCrefAttribute( XmlCrefAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlCrefAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Cref)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlElement( XmlElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlElement( node.StartTag, node.Content, node.EndTag);
				case TransformationKind.Transform: 
					return this.TransformXmlElement( node );
				default: 
					return base.VisitXmlElement( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlElement( XmlElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartTag)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Content)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndTag)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlElementEndTag( XmlElementEndTagSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlElementEndTag( node.LessThanSlashToken, node.Name, node.GreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformXmlElementEndTag( node );
				default: 
					return base.VisitXmlElementEndTag( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlElementEndTag( XmlElementEndTagSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlElementEndTag))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanSlashToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlElementStartTag( XmlElementStartTagSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlElementStartTag( node.LessThanToken, node.Name, node.Attributes, node.GreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformXmlElementStartTag( node );
				default: 
					return base.VisitXmlElementStartTag( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlElementStartTag( XmlElementStartTagSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlElementStartTag))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.GreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlEmptyElement( XmlEmptyElementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlEmptyElement( node.LessThanToken, node.Name, node.Attributes, node.SlashGreaterThanToken);
				case TransformationKind.Transform: 
					return this.TransformXmlEmptyElement( node );
				default: 
					return base.VisitXmlEmptyElement( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlEmptyElement( XmlEmptyElementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlEmptyElement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.LessThanToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Attributes)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SlashGreaterThanToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlName( XmlNameSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlName( node.Prefix, node.LocalName);
				case TransformationKind.Transform: 
					return this.TransformXmlName( node );
				default: 
					return base.VisitXmlName( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlName( XmlNameSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlName))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Prefix)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.LocalName)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlNameAttribute( XmlNameAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlNameAttribute( node.Name, node.EqualsToken, node.StartQuoteToken, node.Identifier, node.EndQuoteToken);
				case TransformationKind.Transform: 
					return this.TransformXmlNameAttribute( node );
				default: 
					return base.VisitXmlNameAttribute( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlNameAttribute( XmlNameAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlNameAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Identifier)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlPrefix( XmlPrefixSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlPrefix( node.Prefix, node.ColonToken);
				case TransformationKind.Transform: 
					return this.TransformXmlPrefix( node );
				default: 
					return base.VisitXmlPrefix( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlPrefix( XmlPrefixSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlPrefix))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Prefix)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ColonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlProcessingInstruction( XmlProcessingInstructionSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlProcessingInstruction( node.StartProcessingInstructionToken, node.Name, node.TextTokens, node.EndProcessingInstructionToken);
				case TransformationKind.Transform: 
					return this.TransformXmlProcessingInstruction( node );
				default: 
					return base.VisitXmlProcessingInstruction( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlProcessingInstruction( XmlProcessingInstructionSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlProcessingInstruction))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.StartProcessingInstructionToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndProcessingInstructionToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlText( XmlTextSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlText( node.TextTokens);
				case TransformationKind.Transform: 
					return this.TransformXmlText( node );
				default: 
					return base.VisitXmlText( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlText( XmlTextSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlText))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitXmlTextAttribute( XmlTextAttributeSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return XmlTextAttribute( node.Name, node.EqualsToken, node.StartQuoteToken, node.TextTokens, node.EndQuoteToken);
				case TransformationKind.Transform: 
					return this.TransformXmlTextAttribute( node );
				default: 
					return base.VisitXmlTextAttribute( node );
			}
		}
		protected virtual ExpressionSyntax TransformXmlTextAttribute( XmlTextAttributeSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(XmlTextAttribute))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Name)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EqualsToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.StartQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.TextTokens)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.EndQuoteToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
		public override SyntaxNode VisitYieldStatement( YieldStatementSyntax node )
		{
			switch ( this.GetTransformationKind( node ) ) 
			{
				case TransformationKind.Clone: 
					return YieldStatement( node.Kind(), node.AttributeLists, node.YieldKeyword, node.ReturnOrBreakKeyword, node.Expression, node.SemicolonToken);
				case TransformationKind.Transform: 
					return this.TransformYieldStatement( node );
				default: 
					return base.VisitYieldStatement( node );
			}
		}
		protected virtual ExpressionSyntax TransformYieldStatement( YieldStatementSyntax node)
		{
			this.Indent();
			var result = InvocationExpression(IdentifierName(nameof(YieldStatement))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
			Argument(this.Transform(node.Kind())).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.AttributeLists)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.YieldKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.ReturnOrBreakKeyword)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.Expression)).WithLeadingTrivia(this.GetIndentation()),
			Token(SyntaxKind.CommaToken).WithTrailingTrivia(this.GetLineBreak()),
			Argument(this.Transform(node.SemicolonToken)).WithLeadingTrivia(this.GetIndentation()),
			})));
			this.Unindent();
			return result;
		}
}
}
