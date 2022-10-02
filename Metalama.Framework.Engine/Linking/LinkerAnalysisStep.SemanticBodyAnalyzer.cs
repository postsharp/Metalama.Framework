﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Analyzes bodies of intermediate symbol semantics.
        /// </summary>
        private class BodyAnalyzer
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly PartialCompilation _intermediateCompilation;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _reachableSemantics;

            public BodyAnalyzer(
                IServiceProvider serviceProvider,
                PartialCompilation intermediateCompilation,
                IReadOnlyList<IntermediateSymbolSemantic> reachableSemantics )
            {
                this._serviceProvider = serviceProvider;
                this._intermediateCompilation = intermediateCompilation;
                this._reachableSemantics = reachableSemantics;
            }

            internal async Task<IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult>> RunAsync(
                CancellationToken cancellationToken )
            {
                var results = new ConcurrentDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult>();

                void AnalyzeSemantic( IntermediateSymbolSemantic semantic )
                {
                    switch ( semantic.Kind )
                    {
                        case IntermediateSymbolSemanticKind.Final:
                            return;

                        case IntermediateSymbolSemanticKind.Base:
                            switch ( semantic.Symbol )
                            {
                                case IMethodSymbol:
                                    results[semantic.ToTyped<IMethodSymbol>()] =
                                        new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                                    break;

                                case IPropertySymbol propertySymbol:
                                    if ( propertySymbol.GetMethod != null )
                                    {
                                        results[semantic.WithSymbol( propertySymbol.GetMethod )] =
                                            new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );
                                    }

                                    if ( propertySymbol.SetMethod != null )
                                    {
                                        results[semantic.WithSymbol( propertySymbol.SetMethod )] =
                                            new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );
                                    }

                                    break;

                                case IEventSymbol @eventSymbol:
                                    if ( @eventSymbol.AddMethod != null )
                                    {
                                        results[semantic.WithSymbol( @eventSymbol.AddMethod )] =
                                            new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );
                                    }

                                    if ( @eventSymbol.RemoveMethod != null )
                                    {
                                        results[semantic.WithSymbol( @eventSymbol.RemoveMethod )] =
                                            new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );
                                    }

                                    break;

                                case IFieldSymbol:
                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }

                            break;

                        default:
                            switch ( semantic.Symbol )
                            {
                                case IMethodSymbol methodSymbol:
                                    results[semantic.ToTyped<IMethodSymbol>()] = this.Analyze( methodSymbol );

                                    break;

                                case IPropertySymbol propertySymbol:
                                    if ( propertySymbol.GetMethod != null )
                                    {
                                        results[semantic.WithSymbol( propertySymbol.GetMethod )] = this.Analyze( propertySymbol.GetMethod );
                                    }

                                    if ( propertySymbol.SetMethod != null )
                                    {
                                        results[semantic.WithSymbol( propertySymbol.SetMethod )] = this.Analyze( propertySymbol.SetMethod );
                                    }

                                    break;

                                case IEventSymbol @eventSymbol:
                                    if ( @eventSymbol.AddMethod != null )
                                    {
                                        results[semantic.WithSymbol( @eventSymbol.AddMethod )] = this.Analyze( @eventSymbol.AddMethod );
                                    }

                                    if ( @eventSymbol.RemoveMethod != null )
                                    {
                                        results[semantic.WithSymbol( @eventSymbol.RemoveMethod )] = this.Analyze( @eventSymbol.RemoveMethod );
                                    }

                                    break;

                                case IFieldSymbol:
                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }

                            break;
                    }
                }

                var taskScheduler = this._serviceProvider.GetRequiredService<ITaskScheduler>();
                await taskScheduler.RunInParallelAsync( this._reachableSemantics, AnalyzeSemantic, cancellationToken );

                return results;
            }

            private SemanticBodyAnalysisResult Analyze( IMethodSymbol symbol )
            {
                var declaration = symbol.GetPrimaryDeclaration().AssertNotNull();
                var semanticModel = this._intermediateCompilation.Compilation.GetSemanticModel( declaration.SyntaxTree );

                var body = GetDeclarationBody( declaration );

                switch ( body )
                {
                    case BlockSyntax rootBlock:
                        var rootBlockCfa = semanticModel.AnalyzeControlFlow( rootBlock );
                        var exitFlowingStatements = new HashSet<StatementSyntax>();
                        var returnStatementProperties = new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>();

                        var blocksWithReturnBeforeUsingLocal = GetBlocksWithReturnBeforeUsingLocal( rootBlock, rootBlockCfa.ReturnStatements );

                        // Get all statements that flow to exit (blocks, ifs, trys, etc.).
                        DiscoverExitFlowingStatements( rootBlock, exitFlowingStatements );

                        // Go through all return statements.
                        foreach ( var returnStatement in rootBlockCfa.ReturnStatements.OfType<ReturnStatementSyntax>() )
                        {
                            switch ( returnStatement )
                            {
                                case { Parent: BlockSyntax parentBlock }:
                                    AddIfExitFlowing( parentBlock, false );

                                    break;

                                case { Parent: IfStatementSyntax ifStatement }:
                                    AddIfExitFlowing( ifStatement, false );

                                    break;

                                case { Parent: ElseClauseSyntax { Parent: IfStatementSyntax ifStatement } }:
                                    AddIfExitFlowing( ifStatement, false );

                                    break;

                                case { Parent: SwitchSectionSyntax { Parent: SwitchStatementSyntax switchStatement } }:
                                    AddIfExitFlowing( switchStatement, true );

                                    break;

                                case { Parent: LockStatementSyntax lockStatement }:
                                    AddIfExitFlowing( lockStatement, false );

                                    break;

                                case { Parent: FixedStatementSyntax fixedStatement }:
                                    AddIfExitFlowing( fixedStatement, false );

                                    break;

                                case { Parent: LabeledStatementSyntax labeledStatement }:
                                    AddIfExitFlowing( labeledStatement, false );

                                    break;

                                case { Parent: UsingStatementSyntax usingStatement }:
                                    AddIfExitFlowing( usingStatement, false );

                                    break;

                                default:
                                    returnStatementProperties.Add( returnStatement, new ReturnStatementProperties( false, false ) );

                                    break;
                            }

                            void AddIfExitFlowing( StatementSyntax controlStatement, bool replaceByBreakIfOmmitted )
                            {
                                if ( exitFlowingStatements.Contains( controlStatement ) )
                                {
                                    // Return statement is in blockless IfStatement that is exit-flowing.
                                    returnStatementProperties.Add( returnStatement, new ReturnStatementProperties( true, replaceByBreakIfOmmitted ) );
                                }
                                else
                                {
                                    returnStatementProperties.Add( returnStatement, new ReturnStatementProperties( false, false ) );
                                }
                            }
                        }

                        return new SemanticBodyAnalysisResult( returnStatementProperties, rootBlockCfa.EndPointIsReachable, blocksWithReturnBeforeUsingLocal );

                    case ArrowExpressionClauseSyntax:
                        return new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                    case MethodDeclarationSyntax { Body: null, ExpressionBody: null }:
                        return new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                    case AccessorDeclarationSyntax { Body: null, ExpressionBody: null }:
                        return new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                    case VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax } }:
                        return new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                    case ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } }:
                        return new SemanticBodyAnalysisResult( new Dictionary<ReturnStatementSyntax, ReturnStatementProperties>(), false, Array.Empty<BlockSyntax>() );

                    default:
                        throw new AssertionFailedException();
                }

                static void DiscoverExitFlowingStatements( StatementSyntax statement, HashSet<StatementSyntax> exitFlowingStatements )
                {
                    /* Start with the root block or the last statement of a block.
                     * This method adds the following statements that flow to the end-point of the method:
                     *   * Blocks.
                     *   * If statements.
                     *   * Try statements.
                     *   * Switch statements.
                     *   * Lock statements.
                     *   * Fixed statements.
                     *   * Using statements.
                     *   * Labeled statements.
                     */

                    switch ( statement )
                    {
                        case ReturnStatementSyntax returnStatement:
                            exitFlowingStatements.Add( returnStatement );

                            break;

                        case BlockSyntax block:
                            exitFlowingStatements.Add( block );

                            ProcessStatementList( block.Statements );

                            break;

                        case IfStatementSyntax ifStatement:
                            // It is necessary to track if statements because return can be directly under the if/else instead of in a block.
                            exitFlowingStatements.Add( ifStatement );

                            DiscoverExitFlowingStatements( ifStatement.Statement, exitFlowingStatements );

                            if ( ifStatement.Else != null )
                            {
                                DiscoverExitFlowingStatements( ifStatement.Else.Statement, exitFlowingStatements );
                            }

                            break;

                        case SwitchStatementSyntax switchStatement:
                            // It is necessary to track switch statements because return can be directly under one of the sections.
                            exitFlowingStatements.Add( switchStatement );

                            foreach ( var section in switchStatement.Sections )
                            {
                                if ( section.Statements.Count > 0 )
                                {
                                    ProcessStatementList( section.Statements );
                                }
                            }

                            break;

                        case LockStatementSyntax lockStatement:
                            // It is necessary to track fixed statements because return can be directly under it.
                            exitFlowingStatements.Add( lockStatement );

                            DiscoverExitFlowingStatements( lockStatement.Statement, exitFlowingStatements );

                            break;

                        case FixedStatementSyntax fixedStatement:
                            // It is necessary to track fixed statements because return can be directly under it.
                            exitFlowingStatements.Add( fixedStatement );

                            DiscoverExitFlowingStatements( fixedStatement.Statement, exitFlowingStatements );

                            break;

                        case CheckedStatementSyntax checkedStatement:
                            DiscoverExitFlowingStatements( checkedStatement.Block, exitFlowingStatements );

                            break;

                        case LabeledStatementSyntax labeledStatement:
                            // It is necessary to track labeled statements because return can be directly under it.
                            exitFlowingStatements.Add( labeledStatement );

                            DiscoverExitFlowingStatements( labeledStatement.Statement, exitFlowingStatements );

                            break;

                        case UnsafeStatementSyntax unsafeStatement:
                            DiscoverExitFlowingStatements( unsafeStatement.Block, exitFlowingStatements );

                            break;

                        case UsingStatementSyntax usingStatement:
                            // It is necessary to track using statements because return can be directly under it.
                            exitFlowingStatements.Add( usingStatement );

                            DiscoverExitFlowingStatements( usingStatement.Statement, exitFlowingStatements );

                            break;

                        case TryStatementSyntax tryStatement:
                            DiscoverExitFlowingStatements( tryStatement.Block, exitFlowingStatements );

                            foreach ( var catchClause in tryStatement.Catches )
                            {
                                DiscoverExitFlowingStatements( catchClause.Block, exitFlowingStatements );
                            }

                            if ( tryStatement.Finally != null )
                            {
                                DiscoverExitFlowingStatements( tryStatement.Finally.Block, exitFlowingStatements );
                            }

                            break;
                    }

                    void ProcessStatementList( IReadOnlyList<StatementSyntax> statements )
                    {
                        if ( statements.Count > 0 )
                        {
                            // Only the last statement (excluding local function declarations) flows to the exit.

                            StatementSyntax? lastNonIgnoredStatement = null;

                            for ( var i = statements.Count - 1; i >= 0; i-- )
                            {
                                if ( statements[i] is not LocalFunctionStatementSyntax )
                                {
                                    lastNonIgnoredStatement = statements[i];

                                    break;
                                }
                            }

                            if ( lastNonIgnoredStatement != null )
                            {
                                DiscoverExitFlowingStatements( lastNonIgnoredStatement, exitFlowingStatements );
                            }
                        }
                    }
                }

                static SyntaxNode GetDeclarationBody( SyntaxNode declaration )
                    => declaration switch
                    {
                        MethodDeclarationSyntax methodDecl => methodDecl.Body ?? (SyntaxNode?) methodDecl.ExpressionBody ?? methodDecl,
                        DestructorDeclarationSyntax destructorDecl => (SyntaxNode?) destructorDecl.Body ?? destructorDecl.ExpressionBody.AssertNotNull(),
                        OperatorDeclarationSyntax operatorDecl => (SyntaxNode?) operatorDecl.Body ?? operatorDecl.ExpressionBody.AssertNotNull(),
                        ConversionOperatorDeclarationSyntax conversionOperatorDecl => (SyntaxNode?) conversionOperatorDecl.Body
                                                                                      ?? conversionOperatorDecl.ExpressionBody.AssertNotNull(),
                        AccessorDeclarationSyntax accessorDecl => accessorDecl.Body ?? (SyntaxNode?) accessorDecl.ExpressionBody ?? accessorDecl,
                        VariableDeclaratorSyntax declarator => declarator,
                        ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause,
                        ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } } recordParameter => recordParameter,
                        _ => throw new AssertionFailedException()
                    };
            }

            private static IReadOnlyList<BlockSyntax> GetBlocksWithReturnBeforeUsingLocal( BlockSyntax rootBlock, IReadOnlyList<SyntaxNode> returnStatements )
            {
                var statementsContainingReturnStatement = new HashSet<StatementSyntax>();

                foreach(var returnStatement in returnStatements)
                {
                    Mark( returnStatement );

                    void Mark(SyntaxNode node)
                    {
                        if ( node is StatementSyntax statement )
                        {
                            if ( statementsContainingReturnStatement.Add( statement ) && statement != rootBlock )
                            {
                                // Process recursively unvisited statement that is not the root block.
                                Mark( statement.Parent );
                            }
                        }
                        else
                        {
                            // Process recursively the parent of a non-statement.
                            Mark( node.Parent );
                        }
                    }
                }

                if (statementsContainingReturnStatement.Count == 0)
                {
                    return Array.Empty<BlockSyntax>();
                }

                var blocksWithUsingLocalAfterReturn = new List<BlockSyntax>();

                // Process every block that contained a return statement.
                foreach(var block in statementsContainingReturnStatement.OfType<BlockSyntax>())
                {
                    var encounteredStatementContainingReturnStatement = false;

                    foreach(var statement in block.Statements)
                    {
                        if ( statementsContainingReturnStatement.Contains(statement))
                        {
                            encounteredStatementContainingReturnStatement = true;
                        }

                        if (statement is LocalDeclarationStatementSyntax localDeclarationStatement && localDeclarationStatement.UsingKeyword != default
                            && encounteredStatementContainingReturnStatement )
                        {
                            blocksWithUsingLocalAfterReturn.Add( block );
                            break;
                        }
                    }
                }

                return blocksWithUsingLocalAfterReturn;
            }
        }
    }
}