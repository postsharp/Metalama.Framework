// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal partial class AdvisedParameterList
    {
        private class ToArrayImpl : UserExpression
        {
            private readonly AdvisedParameterList _parent;

            public ToArrayImpl( AdvisedParameterList parent )
            {
                this._parent = parent;
            }

            protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
            {
                var syntaxGenerator = syntaxGenerationContext.SyntaxGenerator;

                return syntaxGenerator.ArrayCreationExpression(
                    syntaxGenerator.Type( SpecialType.System_Object ),
                    this._parent._parameters.SelectEnumerable(
                        p =>
                            p.RefKind.IsReadable()
                                ? SyntaxFactory.IdentifierName( p.Name )
                                : (SyntaxNode) syntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() ) ) );
            }

            public override IType Type => this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) );
        }
    }
}