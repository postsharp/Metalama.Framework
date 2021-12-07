// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    internal partial class AdvisedParameterList
    {
        private class ToArrayImpl : IUserExpression
        {
            private readonly AdvisedParameterList _parent;

            public ToArrayImpl( AdvisedParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression ToRunTimeExpression()
            {
                var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;
                var syntaxGenerator = syntaxGenerationContext.SyntaxGenerator;

                var array = (ExpressionSyntax) syntaxGenerator.ArrayCreationExpression(
                    syntaxGenerator.Type( SpecialType.System_Object ),
                    this._parent._parameters.Select(
                        p =>
                            p.RefKind.IsReadable()
                                ? SyntaxFactory.IdentifierName( p.Name )
                                : (SyntaxNode) syntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() ) ) );

                return new RuntimeExpression(
                    array,
                    this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) ),
                    syntaxGenerationContext );
            }

            public IType Type => this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) );

            bool IExpression.IsAssignable => false;

            object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
        }
    }
}