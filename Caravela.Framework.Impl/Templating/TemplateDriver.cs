// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateDriver
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly ISymbol _sourceTemplateSymbol;
        private readonly MethodInfo _templateMethod;
        private readonly AspectClass _aspectClass;

        public TemplateDriver( IServiceProvider serviceProvider, AspectClass aspectClass, ISymbol sourceTemplateSymbol, MethodInfo compiledTemplateMethodInfo )
        {
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
            this._sourceTemplateSymbol = sourceTemplateSymbol;
            this._templateMethod = compiledTemplateMethodInfo ?? throw new ArgumentNullException( nameof(compiledTemplateMethodInfo) );
            this._aspectClass = aspectClass;
        }

        public bool TryExpandDeclaration(
            TemplateExpansionContext templateExpansionContext,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out BlockSyntax? block )
        {
            Invariant.Assert( templateExpansionContext.DiagnosticSink.DefaultScope != null );

            var errorCountBefore = templateExpansionContext.DiagnosticSink.ErrorCount;

            using ( TemplateSyntaxFactory.WithContext( templateExpansionContext ) )
            using ( meta.WithContext( templateExpansionContext.MetaApi ) )
            {
                SyntaxNode output;

                using ( DiagnosticContext.WithDefaultLocation( templateExpansionContext.DiagnosticSink.DefaultScope.DiagnosticLocation ) )
                {
                    try
                    {
                        output = this._userCodeInvoker.Invoke(
                            () => (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, Array.Empty<object>() ) );
                    }
                    catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
                    {
                        // The most probably reason we could have a exception here is that the user template has an error.

                        var userException = ex.InnerException;

                        var stackTrace = new StackTrace( ex.InnerException, true );

                        var location = this.GetSourceCodeLocation( stackTrace )
                                       ?? this._sourceTemplateSymbol.GetDiagnosticLocation()
                                       ?? Location.None;

                        diagnosticAdder.Report(
                            TemplatingDiagnosticDescriptors.ExceptionInTemplate.CreateDiagnostic(
                                location,
                                (this._sourceTemplateSymbol,
                                 templateExpansionContext.MetaApi.Declaration,
                                 userException.GetType().Name,
                                 userException.Message.TrimEnd( "." )) ) );

                        block = null;

                        return false;
                    }
                }

                var errorCountAfter = templateExpansionContext.DiagnosticSink.ErrorCount;

                block = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

                block = block.NormalizeWhitespace();

                // We add generated-code annotations to the statements and not to the block itself so that the brackets don't get colored.
                block = block.AddGeneratedCodeAnnotation();

                return errorCountAfter == errorCountBefore;
            }
        }

        private Location? GetSourceCodeLocation( StackTrace stackTrace )
        {
            if ( this._aspectClass == null! )
            {
                // We are in the template test and there is no aspect class. 
                return null;
            }

            if ( this._aspectClass.Project == null )
            {
                // We should not get here because a null project is only for the abstract classes of the framework.
                throw new AssertionFailedException();
            }

            // TODO: This method needs to be rewritten. Ideally, the PDB would be mapped to the source file, it would not be necessary
            // to perform the mapping here.

            // Get the syntax tree where the exception happened.
            var frame =
                stackTrace
                    .GetFrames()
                    .Where( f => f.GetFileName() != null )
                    .Select( f => (Frame: f, File: this._aspectClass.Project.FindCodeFileFromTransformedPath( f.GetFileName() )) )
                    .FirstOrDefault( i => i.File != null );

            if ( frame.File == null )
            {
                return null;
            }

            // Check if we have a location map for this file anyway.
            var textMap = this._aspectClass.Project.GetTextMap( frame.Frame.GetFileName() );

            if ( textMap == null )
            {
                return null;
            }

            var transformedFileFullPath = Path.Combine( this._aspectClass.Project.Directory, frame.File.TransformedPath );

            var transformedText = SourceText.From( File.ReadAllText( transformedFileFullPath ) );

            // Find the node in the syntax tree.
            var textLines = transformedText.Lines;
            var lineNumber = frame.Frame.GetFileLineNumber();

            if ( lineNumber == 0 )
            {
                return null;
            }

            var columnNumber = frame.Frame.GetFileColumnNumber();

            if ( lineNumber > textLines.Count )
            {
                return null;
            }

            var textLine = textLines[lineNumber - 1];

            if ( columnNumber > textLine.End )
            {
                return null;
            }

            var position = textLine.Start + columnNumber - 1;

            var transformedTree = CSharpSyntaxTree.ParseText( transformedText );

            var node = transformedTree.GetRoot().FindNode( TextSpan.FromBounds( position, position ) );
            node = FindPotentialExceptionSource( node );

            if ( node != null )
            {
                var targetLocation = node.GetLocation();
                var sourceLocation = textMap.GetSourceLocation( targetLocation.SourceSpan );

                return sourceLocation ?? targetLocation;
            }
            else
            {
                // TODO: We could report the whole line here.
                return Location.None;
            }

            // Finds a parent node that is a potential source of exception.
            static SyntaxNode? FindPotentialExceptionSource( SyntaxNode? node )
                => node switch
                {
                    null => null,
                    TypeSyntax type => FindPotentialExceptionSource( type.Parent ),
                    ExpressionSyntax expression => expression,
                    StatementSyntax statement => statement,
                    _ => FindPotentialExceptionSource( node.Parent )
                };
        }
    }
}