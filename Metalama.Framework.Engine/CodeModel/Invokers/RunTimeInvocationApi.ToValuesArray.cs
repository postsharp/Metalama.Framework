// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.Invokers;

internal partial class RunTimeInvocationApi
{
    public object ToValuesArray( IParameterList parameters ) => new ToArrayExpression( parameters );

    private sealed class ToArrayExpression : UserExpression
    {
        private readonly ParameterList _parent;

        public ToArrayExpression( IParameterList parent )
        {
            this._parent = (ParameterList?) parent;
        }

        public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var syntaxGenerator = syntaxGenerationContext.SyntaxGenerator;

            return syntaxGenerator.ArrayCreationExpression(
                syntaxGenerator.Type( SpecialType.System_Object ),
                this._parent.SelectAsEnumerable(
                    p =>
                        RefKindExtensions.IsReadable( p.RefKind )
                            ? SyntaxFactory.IdentifierName( (string) p.Name )
                            : (SyntaxNode) syntaxGenerator.DefaultExpression( SymbolExtensions.GetSymbol( (IType) p.Type ) ) ) );
        }

        public override IType Type => this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) );
    }
}