// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private sealed class CompileTimeOnlyRewriter : SafeSyntaxRewriter
        {
            private readonly TemplateCompilerRewriter _parent;

            public CompileTimeOnlyRewriter( TemplateCompilerRewriter parent )
            {
                this._parent = parent;
            }

            public override SyntaxNode VisitTypeOfExpression( TypeOfExpressionSyntax node )
            {
                if ( this._parent._syntaxTreeAnnotationMap.GetSymbol( node.Type ) is ITypeSymbol typeSymbol )
                {
                    var typeId = typeSymbol.GetSerializableTypeId().Id;

                    return this._parent._typeOfRewriter.RewriteTypeOf(
                            typeSymbol,
                            this._parent.CreateTypeParameterSubstitutionDictionary(
                                nameof(TemplateTypeArgument.Type),
                                this._parent._dictionaryOfITypeType ) )
                        .WithAdditionalAnnotations( new SyntaxAnnotation( _rewrittenTypeOfAnnotation, typeId ) );
                }
                else
                {
                    return node;
                }
            }

            public bool TryRewriteProceedInvocation( InvocationExpressionSyntax node, out InvocationExpressionSyntax transformedNode )
            {
                var kind = this._parent._templateMemberClassifier.GetMetaMemberKind( node.Expression );

                if ( kind.IsAnyProceed() )
                {
                    var methodName = node.Expression switch
                    {
                        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                        IdentifierNameSyntax identifier => identifier.Identifier.Text,
                        _ => throw new AssertionFailedException( $"Don't know how to get the member name in {node.Expression.GetType().Name}" )
                    };

                    // ReSharper disable once RedundantSuppressNullableWarningExpression
                    transformedNode =
                        node.CopyAnnotationsTo(
                            InvocationExpression(
                                    this._parent._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.Proceed) ) )
                                .WithArgumentList( ArgumentList( SeparatedList( new[] { Argument( SyntaxFactoryEx.LiteralExpression( methodName ) ) } ) ) ) )!;

                    return true;
                }

                // meta.ProceedAsync().ConfigureAwait(false) is also treated like a Proceed() expression
                else if ( this._parent._syntaxTreeAnnotationMap.GetSymbol( node ).IsTaskConfigureAwait()
                          && node is
                          {
                              Expression: MemberAccessExpressionSyntax { Expression: InvocationExpressionSyntax innerInvocation },
                              ArgumentList.Arguments: [{ Expression: var expression }]
                          }
                          && this.TryRewriteProceedInvocation( innerInvocation, out var transformedInner ) )
                {
                    if ( expression is not LiteralExpressionSyntax literal )
                    {
                        this._parent.Report(
                            TemplatingDiagnosticDescriptors.OnlyLiteralArgumentInConfigureAwaitAfterProceedAsync.CreateRoslynDiagnostic(
                                expression.GetLocation(),
                                expression.ToString() ) );

                        transformedNode = node;

                        return false;
                    }

                    // *.ConfigureAwait( transformedInner, true/false )
                    transformedNode =
                        node.CopyAnnotationsTo(
                            InvocationExpression(
                                    this._parent._templateMetaSyntaxFactory.TemplateSyntaxFactoryMember( nameof(ITemplateSyntaxFactory.ConfigureAwait) ) )
                                .AddArgumentListArguments(
                                    Argument( transformedInner ),
                                    Argument( literal ) ) );

                    return true;
                }
                else
                {
                    transformedNode = node;

                    return false;
                }
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( this.TryRewriteProceedInvocation( node, out var transformedNode ) )
                {
                    return transformedNode;
                }
                else if ( node.IsNameOf() )
                {
                    var type = this._parent._syntaxTreeAnnotationMap.GetSymbol( node.ArgumentList.Arguments[0].Expression );

                    if ( type != null )
                    {
                        return SyntaxFactoryEx.LiteralExpression( type.Name );
                    }
                }

                return base.VisitInvocationExpression( node );
            }

            public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
            {
                var symbol = this._parent._syntaxTreeAnnotationMap.GetSymbol( node );

                if ( node.Identifier.IsKind( SyntaxKind.IdentifierToken )
                     && node is { IsVar: false, Parent: not (QualifiedNameSyntax or AliasQualifiedNameSyntax) } &&
                     !(node.Parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                       && node == memberAccessExpressionSyntax.Name) )
                {
                    // Fully qualifies simple identifiers.
                    if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                    {
                        return node.CopyAnnotationsTo( OurSyntaxGenerator.CompileTime.TypeOrNamespace( namespaceOrType ).WithTriviaFrom( node ) );
                    }
                    else if ( symbol is { IsStatic: true } && node.Parent is not MemberAccessExpressionSyntax && node.Parent is not AliasQualifiedNameSyntax )
                    {
                        switch ( symbol.Kind )
                        {
                            case SymbolKind.Field:
                            case SymbolKind.Property:
                            case SymbolKind.Event:
                            case SymbolKind.Method:
                                // We have an access to a field or method with a "using static", or a non-qualified static member access.
                                return
                                    node.CopyAnnotationsTo(
                                        MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                OurSyntaxGenerator.CompileTime.TypeOrNamespace( symbol.ContainingType ),
                                                IdentifierName( node.Identifier.Text ) )
                                            .WithTriviaFrom( node ) );
                        }
                    }
                }

                return node;
            }
        }
    }
}