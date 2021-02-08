using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Caches the <see cref="SemanticModel"/> of a syntax tree annotations (<see cref="SyntaxAnnotation"/>)
    /// so that the <see cref="SemanticModel"/> does not need to re-evaluated everything the syntax tree
    /// has changes that don't affect symbols. A syntax tree can be annotated using <see cref="AnnotateTree"/>
    /// and the symbols can then be retrieved using <see cref="GetAssignments"/>, <see cref="GetSymbol"/>,
    /// <see cref="GetType"/> and <see cref="GetDeclaredSymbol"/>. Additionally, this class indexes assignments
    /// of local variables. This is accessible from the <see cref="GetAssignments"/> method.
    /// </summary>
    internal sealed class SemanticAnnotationMap
    {
        private readonly Dictionary<ISymbol,SyntaxAnnotation> _declaredSymbolToAnnotationMap = new();
        private readonly Dictionary<SyntaxAnnotation,ISymbol> _annotationToDeclaredSymbolMap = new();
        private readonly Dictionary<ISymbol,SyntaxAnnotation> _symbolToAnnotationMap = new();
        private readonly Dictionary<SyntaxAnnotation,ISymbol> _annotationToSymbolMap = new();
        private readonly Dictionary<ITypeSymbol, SyntaxAnnotation> _typeToAnnotationMap = new();
        private readonly Dictionary<SyntaxAnnotation, ITypeSymbol> _annotationToTypeMap = new ();
        private readonly Dictionary<ILocalSymbol,SyntaxAnnotation> _localToAssignmentMap = new();

        private int _nextId;

        internal static readonly ImmutableList<string> AnnotationKinds = ImmutableList.Create( "local", "symbol", "declared", "type" );

        /// <summary>
        /// Annotates a syntax tree with annotations that can later be resolved using the get methods of this class.
        /// </summary>
        /// <param name="root">Root of the syntax tree.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> for <paramref name="root"/>.</param>
        /// <returns>The annotated syntax tree.</returns>
        public SyntaxNode AnnotateTree( SyntaxNode root, SemanticModel semanticModel )
        {
            var rewriter = new AnnotatingRewriter( semanticModel, this );
            return rewriter.Visit( root )!;
        }

        private SyntaxAnnotation GetLocalAssignmentAllocation( ILocalSymbol local ) => this._localToAssignmentMap[local];

        /// <summary>
        /// Get the annotated node for an original node.
        /// </summary>
        /// <param name="originalNode">The original, untransformed node.</param>
        /// <param name="transformedNode">A copy of <paramref name="originalNode"/> where the children nodes have been transformed.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <returns><paramref name="transformedNode"/> extended with relevant annotations, if any.</returns>
        private SyntaxNode GetAnnotatedNode( SyntaxNode originalNode, SyntaxNode transformedNode, SemanticModel semanticModel )
        {
            // Don't run twice.
            if ( transformedNode.HasAnnotations( AnnotationKinds ) )
            {
                return transformedNode;
            }

            var annotatedNode = transformedNode;

            // Get info from the semantic mode.
            var symbolInfo = semanticModel.GetSymbolInfo( originalNode );
            var typeInfo = semanticModel.GetTypeInfo( originalNode );
            var declaredSymbol = semanticModel.GetDeclaredSymbol( originalNode );

            // Cache semanticModel.GetSymbolInfo
            if ( symbolInfo.Symbol != null )
            {
                if ( !this._symbolToAnnotationMap.TryGetValue( symbolInfo.Symbol, out var annotation ) )
                {
                    this._nextId++;
                    annotation = new SyntaxAnnotation( "symbol", this._nextId.ToString() );
                    this._symbolToAnnotationMap[symbolInfo.Symbol] = annotation;
                    this._annotationToSymbolMap[annotation] = symbolInfo.Symbol;
                }

                annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
            }

            // Cache semanticModel.GetDeclaredSymbol
            if ( declaredSymbol != null )
            {
                if ( !this._declaredSymbolToAnnotationMap.TryGetValue( declaredSymbol, out var annotation ) )
                {
                    this._nextId++;
                    annotation = new SyntaxAnnotation( "declared", this._nextId.ToString() );
                    this._declaredSymbolToAnnotationMap[declaredSymbol] = annotation;
                    this._annotationToDeclaredSymbolMap[annotation] = declaredSymbol;
                }

                annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );

                if ( declaredSymbol is ILocalSymbol local )
                {
                    this._nextId++;
                    annotation = new SyntaxAnnotation( "local", this._nextId.ToString() );
                    this._localToAssignmentMap[local] = annotation;
                }
            }

            // Cache semanticModel.GetTypeInfo.
            if ( typeInfo.Type != null )
            {
                if ( !this._typeToAnnotationMap.TryGetValue( typeInfo.Type, out var annotation ) )
                {
                    this._nextId++;
                    annotation = new SyntaxAnnotation( "type", this._nextId.ToString() );
                    this._typeToAnnotationMap[typeInfo.Type] = annotation;
                    this._annotationToTypeMap[annotation] = typeInfo.Type;
                }

                annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
            }

            return annotatedNode;
        }

        /// <summary>
        /// Returns the result of <c>Microsoft.CodeAnalysis.SemanticModel.GetSymbolInfo</c>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ISymbol? GetSymbol( SyntaxNode node )
        {

            var annotation = node.GetAnnotations( "symbol" ).SingleOrDefault();
            if ( annotation is not null )
            {
                return this._annotationToSymbolMap[annotation];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the result of <see cref="ModelExtensions.GetDeclaredSymbol"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ISymbol? GetDeclaredSymbol( SyntaxNode node )
        {
            var annotation = node.GetAnnotations( "declared" ).SingleOrDefault();
            if ( annotation is not null )
            {
                return this._annotationToDeclaredSymbolMap[annotation];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the corresponding parameter symbol of the invoked member for an argument.
        /// This takes named arguments and <see langword="params" /> parameters into account.
        /// </summary>
        /// <example>
        /// For the method <c>Add(int a, int b)</c> and the invocation <c>Add(b: 0, a: 1)</c>,
        /// calling this method on the first argument (<c>b: 0</c>) returns the second parameter (<c>b</c>).
        /// </example>
        public IParameterSymbol? GetParameterSymbol( ArgumentSyntax argument )
        {
            if ( argument.Parent?.Parent == null )
                return null;

            var invocationSymbol = this.GetSymbol( argument.Parent.Parent );

            var parameters = invocationSymbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.Parameters,
                IPropertySymbol propertySymbol => propertySymbol.Parameters,
                _ => ImmutableArray<IParameterSymbol>.Empty
            };

            if ( parameters.Length == 0 )
                return null;

            if ( argument.NameColon != null )
            {
                return parameters.FirstOrDefault( p => p.Name == argument.NameColon.Name.Identifier.ValueText );
            }

            int index = argument.Parent.ChildNodes().ToList().IndexOf( argument );
            if ( index == -1 )
                return null;

            if ( index < parameters.Length )
                return parameters[index];

            var lastParameter = parameters.Last();
            if ( lastParameter.IsParams )
                return lastParameter;

            return null;
        }

        /// <summary>
        /// Returns the result of <c>semanticModel.GetTypeInfo(node).Type</c>.
        /// </summary>
        public ITypeSymbol? GetType( SyntaxNode node )
        {
            var annotation = node.GetAnnotations( "type" ).SingleOrDefault();
            if ( annotation is not null )
            {
                return this._annotationToTypeMap[annotation];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all syntax nodes (under a given root) that assign a local variable.
        /// </summary>
        /// <param name="local">A local variable.</param>
        /// <param name="root">The root of the syntax tree under which assignments have to be retrieved.</param>
        /// <returns></returns>
        public IEnumerable<SyntaxNode> GetAssignments( ILocalSymbol local, SyntaxNode root )
        {
            return root.GetAnnotatedNodes( this.GetLocalAssignmentAllocation( local ) );
        }

        /// <summary>
        /// A <see cref="CSharpSyntaxRewriter"/> that adds annotations.
        /// </summary>
        private class AnnotatingRewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel _semanticModel;
            private readonly SemanticAnnotationMap _map;

            public AnnotatingRewriter( SemanticModel semanticModel, SemanticAnnotationMap map )
            {
                this._semanticModel = semanticModel;
                this._map = map;
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                return this._map.GetAnnotatedNode( node, base.Visit( node ), this._semanticModel );
            }

            public override SyntaxNode VisitVariableDeclarator( VariableDeclaratorSyntax node )
            {
                // We need annotations.
                var annotated =
                    (VariableDeclaratorSyntax) this._map.GetAnnotatedNode( node, base.VisitVariableDeclarator( node )!, this._semanticModel );

                if ( node.Initializer != null && this._map.GetDeclaredSymbol( annotated ) is ILocalSymbol local )
                {
                    return this.AddAssignmentAnnotation( local, annotated );
                }
                else
                {
                    return annotated;
                }
            }

            public override SyntaxNode VisitAssignmentExpression( AssignmentExpressionSyntax node )
            {
                var annotated = (AssignmentExpressionSyntax) base.VisitAssignmentExpression( node )!;
                if ( this._map.GetSymbol( annotated.Left ) is ILocalSymbol local )
                {
                    return this.AddAssignmentAnnotation( local, annotated );
                }
                else
                {
                    return annotated;
                }
            }

            private SyntaxNode AddAssignmentAnnotation( ILocalSymbol local, SyntaxNode node )
            {
                return node.WithAdditionalAnnotations( this._map.GetLocalAssignmentAllocation( local ) );
            }
        }
    }
}