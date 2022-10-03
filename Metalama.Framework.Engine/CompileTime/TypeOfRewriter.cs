// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CompileTime;

internal class TypeOfRewriter
{
    private readonly NameSyntax _compileTimeTypeName;

    public TypeOfRewriter( SyntaxGenerationContext syntaxGenerationContext )
    {
        this._compileTimeTypeName = (NameSyntax)
            syntaxGenerationContext.SyntaxGenerator.Type( syntaxGenerationContext.ReflectionMapper.GetTypeSymbol( typeof(CompileTimeType) ) );
    }

    public ExpressionSyntax RewriteTypeOf( ITypeSymbol typeSymbol, ExpressionSyntax? substitutions = null )
    {
        if ( typeSymbol is INamedTypeSymbol { IsUnboundGenericType: true } namedType
             && namedType.TypeArguments[0].Kind == SymbolKind.ErrorType )
        {
            // We have a case like typeof(Foo<>). We need to fix it here, otherwise later processing is incorrect.

            typeSymbol = namedType.OriginalDefinition;
        }

        var memberAccess =
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                this._compileTimeTypeName,
                IdentifierName( nameof(CompileTimeType.ResolveCompileTimeTypeOf) ) );

        var invocation = InvocationExpression(
            memberAccess,
            ArgumentList(
                SeparatedList(
                    new[]
                    {
                        Argument( SyntaxFactoryEx.LiteralExpression( typeSymbol.GetSymbolId().ToString() ) ),
                        Argument( substitutions ?? SyntaxFactoryEx.Null )
                    } ) ) );

        return invocation;
    }
}