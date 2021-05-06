// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateDriver
    {
        private readonly ISymbol _sourceTemplateSymbol;
        private readonly MethodInfo _templateMethod;
        private readonly AspectClassMetadata _aspectClass;

        public TemplateDriver( AspectClassMetadata aspectClass, ISymbol sourceTemplateSymbol, MethodInfo compiledTemplateMethodInfo )
        {
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

            // TODO: support target declaration other than a method.
            if ( templateExpansionContext.TargetDeclaration is not IMethod targetMethod )
            {
                throw new NotImplementedException();
            }

            var templateContext = new TemplateContextImpl(
                targetMethod,
                targetMethod.DeclaringType!,
                templateExpansionContext.Compilation,
                templateExpansionContext.DiagnosticSink );

            using ( TemplateSyntaxFactory.WithContext( templateExpansionContext ) )
            using ( TemplateContext.WithContext( templateContext, templateExpansionContext.ProceedImplementation ) )
            {
                SyntaxNode output;

                using ( DiagnosticContext.WithDefaultLocation( templateExpansionContext.DiagnosticSink.DefaultScope.DiagnosticLocation ) )
                {
                    try
                    {
                        output = (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, Array.Empty<object>() );
                    }
                    catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
                    {
                        // The most probably reason we could have a exception here is that the user template has an error.

                        Exception userException = ex.InnerException;

                        var stackTrace = new StackTrace( ex.InnerException, true );

                        var location = this.GetSourceCodeLocation( stackTrace )
                                       ?? this._sourceTemplateSymbol.GetDiagnosticLocation()
                                       ?? Location.None;

                        diagnosticAdder.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.ExceptionInTemplate.CreateDiagnostic(
                                location,
                                (this._sourceTemplateSymbol, templateExpansionContext.TargetDeclaration, userException.GetType().Name,
                                 userException.ToString()) ) );

                        block = null;

                        return false;
                    }
                }

                block = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

                return true;
            }
        }

        private Location? GetSourceCodeLocation( StackTrace stackTrace )
        {
            // TODO: This method needs to be rewritten. Ideally, the PDB would be mapped to the source file, it would not be necessary
            // to perform the mapping here.

            // Get the syntax tree where the exception happened.
            var frame =
                stackTrace
                    .GetFrames()
                    .Where( f => f.GetFileName() != null )
                    .Select( f => (Frame: f, SyntaxTree: this._aspectClass.Project.FindSyntaxTree( f.GetFileName() )) )
                    .FirstOrDefault( i => i.SyntaxTree != null );

            if ( frame.SyntaxTree == null )
            {
                return null;
            }

            // Check if we have a location map for this file anyway.
            var textMap = this._aspectClass.Project.GetTextMap( frame.Frame.GetFileName() );

            if ( textMap == null )
            {
                return null;
            }

            // Find the node in the syntax tree.
            var textLines = frame.SyntaxTree.GetText().Lines;
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

            var node = frame.SyntaxTree.GetRoot().FindNode( TextSpan.FromBounds( position, position ) );
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