using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class ArrayDynamicExpression : IDynamicExpression
    {
        private readonly ArrayBuilder _arrayBuilder;
        private readonly IType _itemType;

        public ArrayDynamicExpression( ArrayBuilder arrayBuilder, ICompilation compilation )
        {
            this._arrayBuilder = arrayBuilder;

            this._itemType = this._arrayBuilder.ItemType switch
            {
                IType ourType => ourType,
                Type reflectionType => compilation.TypeFactory.GetTypeByReflectionType( reflectionType ),
                _ => throw new AssertionFailedException()
            };

            this.ExpressionType = this._itemType.MakeArrayType();
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            var items = this._arrayBuilder.Items.Select( i => RuntimeExpression.FromValue( i, this.ExpressionType.Compilation ).Syntax ).ToArray();

            var generator = LanguageServiceFactory.CSharpSyntaxGenerator;

            var arrayCreation = generator.ArrayCreationExpression(
                generator.TypeExpression( this._itemType.GetSymbol() ),
                items );

            return new RuntimeExpression( arrayCreation, this.ExpressionType );
        }

        public bool IsAssignable => false;

        public IType ExpressionType { get; }
    }

    internal class InterpolatedStringDynamicExpression : IDynamicExpression
    {
        private readonly InterpolatedStringBuilder _builder;

        public InterpolatedStringDynamicExpression( InterpolatedStringBuilder builder, ICompilation compilation )
        {
            this._builder = builder;
            this.ExpressionType = compilation.TypeFactory.GetSpecialType( SpecialType.String );
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            List<InterpolatedStringContentSyntax> contents = new( this._builder.Items.Count );

            foreach ( var content in this._builder.Items )
            {
                switch ( content )
                {
                    case string text:
                        contents.Add(
                            SyntaxFactory.InterpolatedStringText(
                                SyntaxFactory.Token( default, SyntaxKind.InterpolatedStringTextToken, text, text, default ) ) );

                        break;

                    case InterpolatedStringBuilder.Token token:
                        contents.Add( SyntaxFactory.Interpolation( RuntimeExpression.FromValue( token.Expression, this.ExpressionType.Compilation ).Syntax ) );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            var expression = TemplateSyntaxFactory.RenderInterpolatedString(
                SyntaxFactory.InterpolatedStringExpression(
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringStartToken ),
                    SyntaxFactory.List( contents ),
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringEndToken ) ) );

            return new RuntimeExpression( expression, this.ExpressionType, false );
        }

        public bool IsAssignable => false;

        public IType ExpressionType { get; set; }
    }
}