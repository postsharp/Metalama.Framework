using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Compiles the source code of a template, annotated with <see cref="TemplateAnnotator"/>,
    /// to an executable template.
    /// </summary>
    internal sealed partial class TemplateCompilerRewriter : MetaSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;
        private string? _currentStatementListVariableName;
        private List<StatementSyntax>? _currentMetaStatementList;
        private int _nextStatementListId;

        public TemplateCompilerRewriter(SemanticAnnotationMap semanticAnnotationMap)
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        #region Pretty print

        protected override ExpressionSyntax TransformIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == "var")
            {
                // The simplified form does not work.
                return base.TransformIdentifierName(node);
            }
            else if (node.Identifier.Text == "dynamic")
            {
                // We change all dynamic into var in the template.
                return base.TransformIdentifierName(IdentifierName(Identifier("var")));
            }


            // The base implementation is very verbose, so we use this one:

            return
                node.CopyAnnotationsTo(
                    InvocationExpression(IdentifierName(nameof(IdentifierName))).WithArgumentList(ArgumentList(
                        SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                        {
                            Argument(this.CreateLiteralExpression(node.Identifier.Text))
                        }))))!;
        }


        protected override ExpressionSyntax TransformArgument(ArgumentSyntax node)
        {
            // The base implementation is very verbose, so we use this one:

            if (node.RefKindKeyword.Kind() == SyntaxKind.None)
            {
                return
                    node.CopyAnnotationsTo(
                        InvocationExpression(IdentifierName(nameof(Argument))).WithArgumentList(ArgumentList(
                            SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                            {
                                Argument(this.Transform(node.Expression))
                            }))))!;
            }
            else
            {
                return base.TransformArgument(node);
            }
        }

        #endregion

        /// <summary>
        /// Determines how a <see cref="SyntaxNode"/> should be transformed:
        /// <see cref="MetaSyntaxRewriter.TransformationKind.None"/> for compile-time code
        /// or <see cref="MetaSyntaxRewriter.TransformationKind.Transform"/> for run-time code.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override TransformationKind GetTransformationKind(SyntaxNode node)
        {
            if (node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly)
            {
                return TransformationKind.None;
            }

            // Look for annotation on the parent, but stop at 'if' and 'foreach' statements,
            // which have special interpretation.
            for (var parent = node.Parent;
                parent != null;
                parent = parent.Parent)
            {
                if (parent.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly)
                {
                    return parent is IfStatementSyntax || parent is ForEachStatementSyntax || parent is ElseClauseSyntax
                        ? TransformationKind.Transform
                        : TransformationKind.None;
                }
            }

            return TransformationKind.Transform;
        }


        /// <summary>
        /// Determines if a symbol represents a call to <c>proceed()</c>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsProceed(SyntaxNode node)
        {
            var symbol = this._semanticAnnotationMap.GetSymbol(node);
            if (symbol == null)
            {
                return false;
            }

            return symbol.GetAttributes().Any(a => a.AttributeClass.Name == nameof(ProceedAttribute));
        }


        /// <summary>
        /// Transforms an <see cref="ExpressionSyntax"/>, especially taking care of handling
        /// transitions between compile-time expressions and run-time expressions. At these transitions,
        /// compile-time expressions must be wrapped into literals.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override ExpressionSyntax TransformExpression(ExpressionSyntax expression)
        {
            switch (expression.Kind())
            {
                case SyntaxKind.DefaultExpression:
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.DefaultLiteralExpression:
                    // Don't transform default or null.
                    // When we do that, we can try to cast a dynamic 'default' or 'null' into a SyntaxFactory.
                    return expression;
            }

            // A local function that wraps the input `expression` into a LiteralExpression.
            ExpressionSyntax CreateLiteralExpressionFactory(SyntaxKind syntaxKind)
            {
                return InvocationExpression(IdentifierName(nameof(LiteralExpression))).WithArgumentList(ArgumentList(
                    SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(this.Transform(syntaxKind)),
                        Token(SyntaxKind.CommaToken),
                        Argument(InvocationExpression(IdentifierName(nameof(Literal))).WithArgumentList(ArgumentList(
                            SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                            {
                                Argument(expression)
                            }))))
                    })));
            }


            var type = this._semanticAnnotationMap.GetType(expression)!;

            if (type is IErrorTypeSymbol)
            {
                // There is a compile-time error. Return default.
                return LiteralExpression(
                    SyntaxKind.DefaultLiteralExpression,
                    Token(SyntaxKind.DefaultKeyword));
            }

            switch (type.Name)
            {
                case "dynamic":
                    if (this.IsProceed(expression))
                    {
                        // TODO: Emit a diagnostic. proceed() cannot be used as a general expression but only in 
                        // specifically supported statements, i.e. variable assignments and return.
                        throw new AssertionFailedException();
                    }

                    return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ParenthesizedExpression(
                                CastExpression(
                                    IdentifierName(nameof(IDynamicMetaMember)), expression)),
                            IdentifierName(nameof(IDynamicMetaMember.CreateExpression))));

                case "String":
                    return CreateLiteralExpressionFactory(SyntaxKind.StringLiteralExpression);

                case "Int32":
                case "Int16":
                case "Int64":
                case "UInt32":
                case "UInt16":
                case "UInt64":
                case "Byte":
                case "SByte":
                case nameof(Single):
                case nameof(Double):
                    return CreateLiteralExpressionFactory(SyntaxKind.NumericLiteralExpression);

                case nameof(Char):
                    return CreateLiteralExpressionFactory(SyntaxKind.CharacterLiteralExpression);

                default:
                    //TODO: emit an error. We don't know how to serialize this into syntax.
                    //TODO: pluggable syntax serializers must be called here.
                    return expression;
            }
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            this.Indent(3);

            // Generates a template method.
            
            // TODO: templates may support build-time parameters, which must to the compiled template method.
            
            // TODO: also compile templates for properties and so on.
            
            var body = (BlockSyntax) this.VisitBlock(node.Body, TransformationKind.None, true);

            var result = MethodDeclaration(
                    IdentifierName("SyntaxNode"),
                    Identifier(node.Identifier.Text + TemplateCompiler.TemplateMethodSuffix))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(body);

            this.Unindent(3);

            return result;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var transformationKind = this.GetTransformationKind(node);
            return this.VisitBlock(node, transformationKind, transformationKind == TransformationKind.Transform);
        }

        /// <summary>
        /// Transforms a block (according to a specified <see cref="MetaSyntaxRewriter.TransformationKind"/>)
        /// and specifies if the block should have its own <c>List&lt;StatementSyntax&gt;</c>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transformationKind"></param>
        /// <param name="withOwnList"><c>true</c> if the block should declare its own List of statements,
        /// <c>false</c> if it should reuse the one of the parent block.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private SyntaxNode VisitBlock(BlockSyntax node, TransformationKind transformationKind, bool withOwnList)
        {
            if (withOwnList)
            {
                using (this.UseStatementList($"__s{++this._nextStatementListId}", new List<StatementSyntax>()))
                {
                    // List<StatementSyntax> statements = new List<StatementSyntax>(); 
                    this._currentMetaStatementList!.Add(LocalDeclarationStatement(
                        VariableDeclaration(GenericName(Identifier("List"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(nameof(StatementSyntax))))))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                            Identifier(this._currentStatementListVariableName!))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                ObjectCreationExpression(
                                                    GenericName(
                                                            Identifier("List"))
                                                        .WithTypeArgumentList(
                                                            TypeArgumentList(
                                                                SingletonSeparatedList<TypeSyntax>(
                                                                    IdentifierName(nameof(StatementSyntax))))),
                                                    ArgumentList(),
                                                    default
                                                )))))).WithLeadingTrivia(this.GetIndentation()));

                    var metaStatements = this.ToMetaStatements(node.Statements);


                    this._currentMetaStatementList.AddRange(metaStatements);

                    if (transformationKind == TransformationKind.Transform)
                    {
                        // return statements.ToArray();
                        this._currentMetaStatementList.Add(
                            ReturnStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(this._currentStatementListVariableName!),
                                            IdentifierName("ToArray"))))
                                .WithLeadingTrivia(this.GetIndentation())
                        );


                        // Block( Func<StatementSyntax[]>( delegate { ... } )
                        return this.DeepIndent(InvocationExpression(IdentifierName(nameof(Block))).WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                                {
                                    Argument(InvocationExpression(
                                        ObjectCreationExpression(
                                                GenericName(
                                                        Identifier("Func"))
                                                    .WithTypeArgumentList(
                                                        TypeArgumentList(
                                                            SingletonSeparatedList<TypeSyntax>(
                                                                ArrayType(
                                                                        IdentifierName(nameof(StatementSyntax)))
                                                                    .WithRankSpecifiers(
                                                                        SingletonList(
                                                                            ArrayRankSpecifier(
                                                                                SingletonSeparatedList<ExpressionSyntax
                                                                                >(
                                                                                    OmittedArraySizeExpression()))))
                                                            ))))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList(
                                                        Argument(
                                                            AnonymousMethodExpression()
                                                                .WithBody(Block(this._currentMetaStatementList)
                                                                    .AddNoDeepIndentAnnotation())))))))
                                }))));
                    }
                    else
                    {
                        this._currentMetaStatementList.Add(ReturnStatement(
                            InvocationExpression(IdentifierName(nameof(Block)))
                                .WithArgumentList(ArgumentList(
                                    SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                                    {
                                        Argument(IdentifierName(this._currentStatementListVariableName!))
                                    })))).WithLeadingTrivia(this.GetIndentation()));


                        return Block(this._currentMetaStatementList);
                    }
                }
            }
            else
            {
                if (transformationKind == TransformationKind.Transform)
                {
                    // withOwnList must be true.
                    throw new AssertionFailedException();
                }

                var metaStatements = this.ToMetaStatements(node.Statements);

                // Add the statements to the parent list.
                this._currentMetaStatementList!.AddRange(metaStatements);

                // Returns an empty block intentionally.
                return Block();
            }
        }


        private IEnumerable<StatementSyntax> ToMetaStatements(in SyntaxList<StatementSyntax> statements)
            => statements.Select(this.ToMetaStatement);

        /// <summary>
        /// Transforms a source statement into a statement that instantiates this statement.
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        private StatementSyntax ToMetaStatement(StatementSyntax statement)
        {
            if (statement is BlockSyntax block)
            {
                return Block(this.ToMetaStatements(block.Statements));
            }


            var transformedStatement = this.Visit(statement);

            if (transformedStatement is StatementSyntax statementSyntax)
            {
                // The statement is already build-time code so there is nothing to transform.
                return statementSyntax.WithLeadingTrivia(this.GetIndentation());
            }
            else if (transformedStatement is ExpressionSyntax expressionSyntax)
            {
                // The statement is run-time code and has been transformed into an expression
                // creating the StatementSyntax.

                var statementComment = NormalizeSpace(statement.ToString());


                if (statementComment.Length > 120)
                {
                    statementComment = statementComment.Substring(0, 117) + "...";
                }

                var leadingTrivia = TriviaList(CarriageReturnLineFeed).AddRange(this.GetIndentation())
                    .Add(Comment("// " + statementComment)).Add(CarriageReturnLineFeed).AddRange(this.GetIndentation());
                var trailingTrivia = TriviaList(CarriageReturnLineFeed, CarriageReturnLineFeed);


                // statements.Add( expression )
                var add = ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(this._currentStatementListVariableName!),
                                IdentifierName("Add")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(expressionSyntax)))));

                return add.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        private static string NormalizeSpace(string statementComment)
        {
            // TODO: Replace this with something more GC-friendly.
            
            statementComment = statementComment.Replace('\n', ' ').Replace('\r', ' ');

            while (true)
            {
                var old = statementComment;
                statementComment = statementComment.Replace("  ", " ");
                if (old == statementComment)
                {
                    return statementComment;
                }
            }
        }

        public override SyntaxNode VisitInterpolation(InterpolationSyntax node)
        {
            if (node.Expression.GetScopeFromAnnotation() != SymbolDeclarationScope.CompileTimeOnly &&
                this._semanticAnnotationMap.GetType(node.Expression)!.Kind != SymbolKind.DynamicType)
            {
                return this.DeepIndent(InvocationExpression(IdentifierName(nameof(InterpolatedStringText)))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                            {
                                Argument(
                                    InvocationExpression(
                                            IdentifierName(nameof(Token)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                                                {
                                                    Argument(LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                                                        Token(SyntaxKind.DefaultKeyword))),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(this.Transform(SyntaxKind.InterpolatedStringTextToken)),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(node.Expression),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(node.Expression),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                                                        Token(SyntaxKind.DefaultKeyword))),
                                                })))
                                )
                            }))));
            }
            else
            {
                var transformedInterpolation = base.VisitInterpolation(node);
                return transformedInterpolation;
            }
        }


        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            if (this.GetTransformationKind(node) == TransformationKind.Transform)
            {
                // Run-time if. Just serialize to syntax.
                return this.TransformIfStatement(node);
            }
            else
            {
                var transformedStatement = this.ToMetaStatement(node.Statement);
                var transformedElseStatement = node.Else != null ? this.ToMetaStatement(node.Else.Statement) : null;
                return IfStatement(node.Condition, transformedStatement,
                    (transformedElseStatement != null ? ElseClause(transformedElseStatement) : null));
            }
        }


        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (this.GetTransformationKind(node) == TransformationKind.Transform)
            {
                // Run-time foreach. Just serialize to syntax.
                return this.TransformForEachStatement(node);
            }

            this.Indent();

            StatementSyntax statement;
            switch (node.Statement)
            {
                case BlockSyntax block:
                    var metaStatements = this.ToMetaStatements(block.Statements);
                    statement = Block(metaStatements);
                    break;

                default:
                    statement = this.ToMetaStatement(node.Statement);
                    break;
            }

            this.Unindent();

            return ForEachStatement(node.Type, node.Identifier, node.Expression, statement);
        }


        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var proceedAssignments =
                node.Declaration.Variables
                    .Where(n => n.Initializer != null && this.IsProceed(n.Initializer.Value))
                    .ToList();

            if (proceedAssignments.Count == 0)
            {
                return base.VisitLocalDeclarationStatement(node);
            }
            else if (proceedAssignments.Count > 1)
            {
                throw new AssertionFailedException();
            }
            else
            {
                var returnVariableName = proceedAssignments[0].Identifier.Text;

                var createBlock = InvocationExpression(IdentifierName(nameof(Block)))
                    .WithArgumentList(ArgumentList(
                        SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                        {
                            // Declare variable.
                            Argument(
                                InvocationExpression(
                                        IdentifierName("LocalDeclarationStatement"))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                InvocationExpression(
                                                                        IdentifierName("VariableDeclaration"))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList(
                                                                                Argument(
                                                                                    InvocationExpression(
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            ParenthesizedExpression(
                                                                                                CastExpression(
                                                                                                    IdentifierName(
                                                                                                        nameof(
                                                                                                            IProceedImpl
                                                                                                        )),
                                                                                                    proceedAssignments[
                                                                                                            0]
                                                                                                        .Initializer
                                                                                                        .Value)),
                                                                                            IdentifierName(
                                                                                                nameof(IProceedImpl
                                                                                                    .CreateTypeSyntax
                                                                                                ))),
                                                                                        ArgumentList()))))),
                                                                IdentifierName("WithVariables")))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(
                                                                        InvocationExpression(
                                                                                IdentifierName(
                                                                                    "SingletonSeparatedList"))
                                                                            .WithArgumentList(
                                                                                ArgumentList(
                                                                                    SingletonSeparatedList(
                                                                                        Argument(
                                                                                            InvocationExpression(
                                                                                                    IdentifierName(
                                                                                                        "VariableDeclarator"))
                                                                                                .WithArgumentList(
                                                                                                    ArgumentList(
                                                                                                        SingletonSeparatedList(
                                                                                                            Argument(
                                                                                                                InvocationExpression(
                                                                                                                        IdentifierName(
                                                                                                                            "Identifier"))
                                                                                                                    .WithArgumentList(
                                                                                                                        ArgumentList(
                                                                                                                            SingletonSeparatedList(
                                                                                                                                Argument(
                                                                                                                                    this
                                                                                                                                        .CreateLiteralExpression(
                                                                                                                                            returnVariableName)))))))))))))))))))))
                            ),
                            Token(SyntaxKind.CommaToken),
                            // Inject call to Proceed
                            Argument(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ParenthesizedExpression(
                                            CastExpression(
                                                IdentifierName(nameof(IProceedImpl)),
                                                proceedAssignments[0].Initializer.Value)),
                                        IdentifierName(nameof(IProceedImpl.CreateAssignStatement))),
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                                            {
                                                Argument(this.CreateLiteralExpression(returnVariableName)),
                                            }
                                        ))
                                )
                            )
                        })));

                createBlock = this.DeepIndent(createBlock);

                // Annotate the block for removal.
                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        createBlock,
                        IdentifierName(nameof(TemplateHelper.WithFlattenBlockAnnotation))));
            }
        }


        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (node.Expression != null && this.IsProceed(node.Expression))
            {
                var expressionType = this._semanticAnnotationMap.GetType(node.Expression);
                if (expressionType == null)
                {
                    // We need the expression type.
                    throw new Exception();
                }


                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastExpression(
                                IdentifierName(nameof(IProceedImpl)), node.Expression)),
                        IdentifierName(nameof(IProceedImpl.CreateReturnStatement))),
                    ArgumentList());
            }
            else
            {
                return base.VisitReturnStatement(node);
            }
        }

        public override bool VisitIntoStructuredTrivia => false;

        private StatementListCookie UseStatementList(string variableName, List<StatementSyntax> metaStatementList)
        {
            var cookie = new StatementListCookie(this, this._currentStatementListVariableName!,
                this._currentMetaStatementList!);
            this._currentStatementListVariableName = variableName;
            this._currentMetaStatementList = metaStatementList;
            return cookie;
        }
    }
}