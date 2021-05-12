// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        /// <summary>
        /// Generates code that invokes members of the <see cref="TemplateSyntaxFactory"/> class.
        /// </summary>
        private class TemplateMetaSyntaxFactoryImpl
        {
            private readonly MetaSyntaxFactoryImpl _metaSyntaxFactory;

            public TemplateMetaSyntaxFactoryImpl( MetaSyntaxFactoryImpl metaSyntaxFactory )
            {
                this._metaSyntaxFactory = metaSyntaxFactory;
            }

            /// <summary>
            /// Generates a <see cref="MemberAccessExpressionSyntax"/> that represents the fully-qualified name
            /// of a method of the <see cref="TemplateSyntaxFactory"/> class.
            /// </summary>
            /// <param name="name">Name of the method.</param>
            /// <returns></returns>
            public MemberAccessExpressionSyntax TemplateSyntaxFactoryMember( string name )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._metaSyntaxFactory.Type( typeof(TemplateSyntaxFactory) ),
                    SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericTemplateSyntaxFactoryMember( string name, params TypeSyntax[] genericParameters )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._metaSyntaxFactory.Type( typeof(TemplateSyntaxFactory) ),
                    SyntaxFactory.GenericName( SyntaxFactory.Identifier( name ) )
                        .WithTypeArgumentList( SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( genericParameters ) ) ) );

            /// <summary>
            /// Generates a call to the <see cref="TemplateSyntaxFactory.GetUniqueIdentifier"/> method.
            /// </summary>
            /// <param name="hint"></param>
            /// <returns></returns>
            public ExpressionSyntax GetUniqueIdentifier( string hint )
                => SyntaxFactory.InvocationExpression( this.TemplateSyntaxFactoryMember( nameof(TemplateSyntaxFactory.GetUniqueIdentifier) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( hint ) ) } ) ) );

            public ExpressionSyntax Location( Location? location )
            {
                if ( location == null )
                {
                    return SyntaxFactoryEx.Null;
                }

                var filePath = SyntaxFactoryEx.LiteralExpression( location.SourceTree?.FilePath );
                var start = SyntaxFactoryEx.LiteralExpression( location.SourceSpan.Start );
                var end = SyntaxFactoryEx.LiteralExpression( location.SourceSpan.End );
                var lineSpan = location.GetLineSpan();

                // Location.Create(
                //     filePath,
                //     TextSpan.FromBounds( start, end ),
                //     new LinePositionSpan( new LinePosition( startLine, startChar ), new LinePosition( endLine, endChar ) ) );

                var invocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this._metaSyntaxFactory.Type( typeof(Location) ),
                            SyntaxFactory.IdentifierName( "Create" ) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument( filePath ),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    this._metaSyntaxFactory.Type( typeof(TextSpan) ),
                                                    SyntaxFactory.IdentifierName( "FromBounds" ) ) )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList(
                                                        new[] { SyntaxFactory.Argument( start ), SyntaxFactory.Argument( end ) } ) ) ) ),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.ObjectCreationExpression( this._metaSyntaxFactory.Type( typeof(LinePositionSpan) ) )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList(
                                                        new[]
                                                        {
                                                            SyntaxFactory.Argument( this.LinePosition( lineSpan.StartLinePosition ) ),
                                                            SyntaxFactory.Argument( this.LinePosition( lineSpan.EndLinePosition ) )
                                                        } ) ) ) )
                                } ) ) );

                return invocation;
            }

            public ObjectCreationExpressionSyntax LinePosition( LinePosition linePosition )
                => this.LinePosition( SyntaxFactoryEx.LiteralExpression( linePosition.Line ), SyntaxFactoryEx.LiteralExpression( linePosition.Character ) );

            public ObjectCreationExpressionSyntax LinePosition( ExpressionSyntax startLine, ExpressionSyntax startChar )
                => SyntaxFactory.ObjectCreationExpression( this._metaSyntaxFactory.Type( typeof(LinePosition) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList( new[] { SyntaxFactory.Argument( startLine ), SyntaxFactory.Argument( startChar ) } ) ) );
        }
    }
}