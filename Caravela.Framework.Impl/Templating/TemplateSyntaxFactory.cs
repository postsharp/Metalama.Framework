// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    [Obfuscation(Exclude = true)]
    public static class TemplateSyntaxFactory
    {
        private static readonly SyntaxAnnotation _flattenBlockAnnotation = new SyntaxAnnotation( "flatten" );

        [ThreadStatic]
        private static TemplateExpansionContext? _expansionContext;

        internal static TemplateExpansionContext ExpansionContext => _expansionContext ?? throw new InvalidOperationException( "ExpansionContext cannot be null." );

        internal static void Initialize( TemplateExpansionContext expansionContext )
        {
            _expansionContext = expansionContext;
        }

        internal static void Close()
        {
            _expansionContext = null;
        }

        public static BlockSyntax WithFlattenBlockAnnotation( this BlockSyntax block ) => block.WithAdditionalAnnotations( _flattenBlockAnnotation );

        public static bool HasFlattenBlockAnnotation( this BlockSyntax block ) => block.HasAnnotation( _flattenBlockAnnotation );

        // ReSharper disable once UnusedMember.Global
        public static SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public static SyntaxKind BooleanKeyword( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        public static StatementSyntax TemplateReturnStatement( ExpressionSyntax? returnExpression ) => ExpansionContext.CreateReturnStatement( returnExpression );

        public static RuntimeExpression CreateDynamicMemberAccessExpression( IDynamicMember dynamicMember, string member )
        {
            if ( dynamicMember is IDynamicMemberDifferentiated metaMemberDifferentiated )
            {
                return metaMemberDifferentiated.CreateMemberAccessExpression( member );
            }

            return new( SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, dynamicMember.CreateExpression().Syntax, SyntaxFactory.IdentifierName( member ) ) );
        }

        public static SyntaxToken GetUniqueIdentifier( string hint ) =>
            SyntaxFactory.Identifier( ExpansionContext.LexicalScope.GetUniqueIdentifier( hint ) );
    }
}