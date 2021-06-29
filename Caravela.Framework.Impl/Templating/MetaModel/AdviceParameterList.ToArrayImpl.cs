// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdviceParameterList
    {
        private class ToArrayImpl : IDynamicExpression
        {
            private readonly AdviceParameterList _parent;

            public ToArrayImpl( AdviceParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression? CreateExpression( string? expressionText, Location? location = null )
            {
                var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

                var array = (ExpressionSyntax) syntaxGenerator.ArrayCreationExpression(
                    syntaxGenerator.TypeExpression( SpecialType.System_Object ),
                    this._parent._parameters.Select(
                        p =>
                            p.IsOut()
                                ? (SyntaxNode) syntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() )
                                : SyntaxFactory.IdentifierName( p.Name ) ) );

                return new RuntimeExpression(
                    array,
                    this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) ) );
            }
        }
    }
}