// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        /// <summary>
        /// Represents a context in which meta-statements are emitted. A context is composed of a list of meta statements
        /// and a dictionary mapping template symbols (typically local variables and local methods) to the name of the
        /// template variable that contains the name of the corresponding run-time symbol (which is determined at run time).
        ///
        /// It also defines <see cref="StatementListVariableName"/>, which is the name of the template variable that contains
        /// the current list of statement. There is typically one list per run-time block.
        /// </summary>
        private class MetaContext
        {
            private readonly Dictionary<ISymbol, SyntaxToken> _generatedSymbolNameLocals;

            private MetaContext( string statementListVariableName, Dictionary<ISymbol, SyntaxToken> generatedSymbolNameLocals )
            {
                this.StatementListVariableName = statementListVariableName;
                this._generatedSymbolNameLocals = generatedSymbolNameLocals;
                this.Statements = new();
            }
            
            /// <summary>
            /// Creates a child <see cref="MetaContext"/> that corresponds to a run-time block, so it has its own
            /// <see cref="StatementListVariableName"/>.
            /// </summary>
            /// <param name="parentContext">The parent context, or <c>null</c> if we are building the root context.</param>
            /// <returns></returns>
            public static MetaContext CreateForRunTimeBlock( MetaContext? parentContext, string statementListVariableName )
            {
                var lexicalScope = parentContext?._generatedSymbolNameLocals ?? new Dictionary<ISymbol, SyntaxToken>();
                return new MetaContext( statementListVariableName, lexicalScope );
            }
            
            /// <summary>
            /// Creates a child <see cref="MetaContext"/> copies everything from the parent context but has its own
            /// list of statements.
            /// </summary>
            /// <param name="parentContext"></param>
            /// <returns></returns>
            public static MetaContext CreateHelperContext( MetaContext parentContext )
            {
                return new MetaContext( parentContext.StatementListVariableName, parentContext._generatedSymbolNameLocals );
            }

            /// <summary>
            /// Creates a child <see cref="MetaContext"/> that corresponds to a new build-time block (lexical scope).
            /// Symbols defined in the child scope are not defined in the parent scope.
            /// </summary>
            /// <param name="parentContext"></param>
            /// <returns></returns>
            public static MetaContext CreateForBuildTimeBlock( MetaContext parentContext )
            {
                // Build-time blocks are currenty without effect because the dictionary maps resolved symbols, and not symbol
                // names. Two declaration of variables with the same name are still different symbols, so we don't strictly
                // need to split the dictionaries.
                // However, we're keeping this method for completeness and clarity.
                
                var lexicalScope = new Dictionary<ISymbol, SyntaxToken>(parentContext._generatedSymbolNameLocals);
                
                return new MetaContext( parentContext.StatementListVariableName, lexicalScope );
            }

            /// <summary>
            /// Gets the name of the template variable (a List{StatementSyntax}) in which the statements for the current run-time block are being
            /// stored.
            /// </summary>
            public string StatementListVariableName { get; }
            
            /// <summary>
            /// Gets the list of meta-statements in the current context.
            /// </summary>
            public List<StatementSyntax> Statements { get; }

            /// <summary>
            /// Gets the name of the compiled template variable containing the name of the run-time variable corresponding
            /// to a given source template symbol (typically a local variable or a local method), if such name has been defined before.
            /// </summary>
            /// <param name="symbol"></param>
            /// <param name="templateVariableName"></param>
            /// <returns></returns>
            public bool TryGetGeneratedSymbolLocal( ISymbol symbol, out SyntaxToken templateVariableName )
                => this._generatedSymbolNameLocals.TryGetValue( symbol, out templateVariableName );

            /// <summary>
            /// Maps a local template symbol (typically a local variable or a local function of the source template) to
            /// the name of the compiled template variable that contains the run-time name of the symbol. 
            /// </summary>
            /// <param name="identifierSymbol"></param>
            /// <param name="templateVariableName"></param>
            public void AddGeneratedSymbolLocal( ISymbol identifierSymbol, SyntaxToken templateVariableName )
            {
                this._generatedSymbolNameLocals.Add( identifierSymbol, templateVariableName );
            }
        }
    }
}