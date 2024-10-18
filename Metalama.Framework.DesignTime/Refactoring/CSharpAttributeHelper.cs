// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Metalama.Framework.DesignTime.Refactoring
{
    internal static class CSharpAttributeHelper
    {
        public static async ValueTask<SyntaxNode?> AddAttributeAsync(
            SyntaxNode oldRoot,
            SyntaxNode? oldNode,
            AttributeDescription attribute,
            SyntaxGenerationContext context,
            CancellationToken cancellationToken )
        {
            // target syntax node doesn't exist anymore, nothing to be done here
            if ( oldNode == null )
            {
                return oldRoot;
            }

            if ( oldNode.IsKind( SyntaxKind.VariableDeclarator ) && oldNode.Parent is { Parent: FieldDeclarationSyntax fieldDeclarationSyntax } )
            {
                oldNode = fieldDeclarationSyntax;
            }

            SyntaxNode newRoot;

            if ( !oldNode.IsKind( SyntaxKind.CompilationUnit ) )
            {
                var newNode = AddAttribute( oldNode, attribute );

                if ( newNode == null )
                {
                    return null;
                }

                newRoot = oldRoot.ReplaceNode( oldNode, newNode );
            }
            else
            {
                // Plain text appending works well for GlobalAspects.cs but for more complicated cases (we don't have them at this moment)
                // it could broke something.
                // It is not easy or maybe even possible to add an attribute with correct leading trivia and preserve compilation unit trivia at
                // the same time. More investigation is needed.
                newRoot = await CSharpSyntaxTree.ParseText( oldRoot.ToFullString() + "\r\n" + CreateAttributeSourceCode( attribute, forAssembly: true ) )
                    .GetRootAsync( cancellationToken );
            }

            foreach ( var ns in attribute.Imports )
            {
                if ( string.IsNullOrEmpty( ns ) )
                {
                    continue;
                }

                if ( await newRoot.SyntaxTree.GetRootAsync( cancellationToken ) is CompilationUnitSyntax newUnit )
                {
                    if ( newUnit.Usings.All( u => u.Name.ToString() != ns ) )
                    {
                        newRoot =
                            newUnit.AddUsings(
                                SyntaxFactory.UsingDirective( SyntaxFactory.IdentifierName( ns ).WithLeadingTrivia( SyntaxFactory.ElasticSpace ) )
                                    .WithTrailingTrivia( context.ElasticEndOfLineTriviaList )
                                    .WithAdditionalAnnotations( Formatter.Annotation ) );
                    }
                }
            }

            return newRoot;
        }

        private static SyntaxNode? AddAttribute( SyntaxNode oldNode, AttributeDescription attribute )
        {
            var newNode = oldNode.WithoutLeadingTrivia();

            var attributeList = CreateAttributeSyntax( attribute )
                .WithAdditionalAnnotations( Formatter.Annotation );

            switch ( oldNode.Kind() )
            {
                case SyntaxKind.MethodDeclaration:
                    newNode = ((MethodDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.DestructorDeclaration:
                    newNode = ((DestructorDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.ConstructorDeclaration:
                    newNode = ((ConstructorDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.InterfaceDeclaration:
                    newNode = ((InterfaceDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.DelegateDeclaration:
                    newNode = ((DelegateDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.EnumDeclaration:
                    newNode = ((EnumDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.ClassDeclaration:
                    newNode = ((ClassDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.StructDeclaration:
                    newNode = ((StructDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.Parameter:
                    newNode = ((ParameterSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.PropertyDeclaration:
                    newNode = ((PropertyDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.EventDeclaration:
                    newNode = ((EventDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    newNode = ((AccessorDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.OperatorDeclaration:
                    newNode = ((OperatorDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.ConversionOperatorDeclaration:
                    newNode = ((ConversionOperatorDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.IndexerDeclaration:
                    newNode = ((IndexerDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.FieldDeclaration:
                    newNode = ((FieldDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                case SyntaxKind.EventFieldDeclaration:
                    newNode = ((EventFieldDeclarationSyntax) newNode).AddAttributeLists( attributeList );

                    break;

                default:
                    return null;
            }

            return newNode.WithLeadingTrivia( oldNode.GetLeadingTrivia() );
        }

        // TODO #29087
        /*
        internal static async ValueTask<Solution> AddAssemblyAttributeAsync(
            [Required] Solution currentSolution,
            [Required] string targetFilePath,
            [Required] AttributeDescription attribute,
            CancellationToken cancellationToken )
        {
            // In case of a multi-targeted project, we pick one arbitrarily.
            // The others get reloaded automatically since they share the same source file.
            var documentId = currentSolution.GetDocumentIdsWithFilePath( targetFilePath ).FirstOrDefault();

            async ValueTask<SyntaxNode> CreateNewRootAsync( SyntaxNode oldRoot )
            {
                var newRoot = await AddAttributeAsync( oldRoot, oldRoot, attribute, cancellationToken );
                newRoot = Formatter.Format( newRoot, Formatter.Annotation, currentSolution.Workspace );

                return newRoot;
            }

            if ( documentId == null )
            {
                // Handle new document that is unknown to Roslyn at this point.
                var tree = CSharpSyntaxTree.ParseText( File.ReadAllText( targetFilePath ) );

                SyntaxNode oldRoot = tree.GetCompilationUnitRoot();
                var newRoot = await CreateNewRootAsync( oldRoot );

                using ( var writer = new StreamWriter( targetFilePath ) )
                {
                    newRoot.WriteTo( writer );
                }

                return currentSolution;
            }
            else
            {
                var currentDocument = currentSolution.GetDocument( documentId );

                var oldRoot = (CompilationUnitSyntax) await currentDocument.GetSyntaxRootAsync( cancellationToken );
                var newRoot = await CreateNewRootAsync( oldRoot );

                var newSolution = currentSolution.WithDocumentSyntaxRoot( currentDocument.Id, newRoot );

                return newSolution;
            }
        }
        */

        public static async ValueTask<Solution> AddAttributeAsync(
            Document document,
            ISymbol symbol,
            AttributeDescription attribute,
            SyntaxGenerationContext context,
            CancellationToken cancellationToken )
        {
            var currentSolution = document.Project.Solution;
            var oldRoot = (CompilationUnitSyntax?) await document.GetSyntaxRootAsync( cancellationToken );

            if ( oldRoot == null )
            {
                // Error.
                return document.Project.Solution;
            }

            var oldNode = await symbol.DeclaringSyntaxReferences.Single( r => r.SyntaxTree == oldRoot.SyntaxTree ).GetSyntaxAsync( cancellationToken );

            var newRoot = await AddAttributeAsync( oldRoot, oldNode, attribute, context, cancellationToken );

            if ( newRoot == null )
            {
                // Error.
                return document.Project.Solution;
            }

            newRoot = Formatter.Format( newRoot, Formatter.Annotation, currentSolution.Workspace );

            var newSolution = currentSolution.WithDocumentSyntaxRoot( document.Id, newRoot );

            return newSolution;
        }

        private static string CreateAttributeSourceCode( AttributeDescription attribute, bool forAssembly )
        {
            using var stringBuilderHandle = StringBuilderPool.Default.Allocate();
            var stringBuilder = stringBuilderHandle.Value;

            stringBuilder.Append( '[' );

            if ( forAssembly )
            {
                stringBuilder.Append( "assembly: " );
            }

            stringBuilder.Append( attribute.Name );

            IList<(string Name, string Value)> properties = attribute.Properties;

            var arguments = attribute.Arguments.Concat( properties.Select( property => $"{property.Name}={property.Value}" ) )
                .ToArray();

            if ( arguments.Any() )
            {
                stringBuilder.Append( '(' );
                stringBuilder.Append( string.Join( ", ", arguments ) );
                stringBuilder.Append( ')' );
            }

            stringBuilder.Append( ']' );

            return stringBuilder.ToString();
        }

        private static AttributeListSyntax CreateAttributeSyntax( AttributeDescription attribute )
        {
            var root = CSharpSyntaxTree
                .ParseText( CreateAttributeSourceCode( attribute, false ) )
                .GetRoot();

            var attributeSyntax = ((IncompleteMemberSyntax) root.ChildNodes().First())
                .AttributeLists.First()
                .Attributes.First();

            return SyntaxFactory.AttributeList( SyntaxFactory.SeparatedList( new[] { attributeSyntax } ) );
        }
    }
}