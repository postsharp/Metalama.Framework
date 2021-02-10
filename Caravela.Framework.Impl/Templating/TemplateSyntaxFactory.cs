using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    public static class TemplateSyntaxFactory
    {
        private static readonly SyntaxAnnotation _flattenBlockAnnotation = new SyntaxAnnotation( "flatten" );

        [ThreadStatic]
        private static ITemplateExpansionContext? _expansionContext;

        internal static ITemplateExpansionContext ExpansionContext => _expansionContext ?? throw new InvalidOperationException( "ExpansionContext cannot be null." );

        internal static void Initialize( ITemplateExpansionContext expansionContext )
        {
            _expansionContext = expansionContext;
        }

        internal static void Close()
        {
            _expansionContext = null;
        }

        public static BlockSyntax WithFlattenBlockAnnotation( this BlockSyntax block ) =>
        block.WithAdditionalAnnotations( _flattenBlockAnnotation );

        public static bool HasFlattenBlockAnnotation( this BlockSyntax block ) =>
            block.HasAnnotation( _flattenBlockAnnotation );

        // ReSharper disable once UnusedMember.Global
        public static SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public static SyntaxKind BooleanKeyword( bool value ) =>
            value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        public static StatementSyntax TemplateReturnStatement( ExpressionSyntax? returnExpression ) =>
            ExpansionContext.CreateReturnStatement( returnExpression );

        public static IDisposable OpenTemplateLexicalScope() =>
            ExpansionContext.CurrentLexicalScope.OpenNestedScope();

        public static SyntaxToken TemplateDeclaratorIdentifier( string text ) =>
            ExpansionContext.CurrentLexicalScope.DefineIdentifier( text );

        public static IdentifierNameSyntax TemplateIdentifierName( string name ) =>
            ExpansionContext.CurrentLexicalScope.CreateIdentifierName( name );
    }
}