using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio.Remoting.Api;

[JsonObject]
public class SerializableAnnotatedSyntaxTree
{
    private static string[] _annotationKinds = new[]
    {
        Simplifier.Annotation.Kind!, FormattingAnnotations.GeneratedCodeAnnotationKind, FormattingAnnotations.SourceCodeAnnotation.Kind!
    };
    
    public string Text { get; }

    public ImmutableArray<SerializableAnnotation> NodeAnnotations { get; }
    public ImmutableArray<SerializableAnnotation> TokenAnnotations { get; }

    public SerializableAnnotatedSyntaxTree( string text, ImmutableArray<SerializableAnnotation> nodeAnnotations, ImmutableArray<SerializableAnnotation> tokenAnnotations )
    {
        this.Text = text;
        this.NodeAnnotations = nodeAnnotations;
        this.TokenAnnotations = tokenAnnotations;
    }

    public SerializableAnnotatedSyntaxTree( SyntaxTree syntaxTree )
    {
        this.Text = syntaxTree.GetText().ToString();
        var nodeAnnotations = ImmutableArray.CreateBuilder<SerializableAnnotation>();
        var tokenAnnotations = ImmutableArray.CreateBuilder<SerializableAnnotation>();

        foreach ( var annotated in syntaxTree.GetRoot().GetAnnotatedNodesAndTokens() )
        {
            foreach ( var kind in _annotationKinds )
            {
                foreach ( var annotation in annotated.GetAnnotations( kind ) )
                {
                    var serializableAnnotation = new SerializableAnnotation( annotated.SpanStart, annotated.Span.Length, annotation.Kind!, annotation.Data );
                    
                    if ( annotated.IsNode )
                    {
                        nodeAnnotations.Add( serializableAnnotation );    
                    }
                    else
                    {
                        tokenAnnotations.Add( serializableAnnotation );
                    }
                }
            }
        }
    }
    
    
    public SyntaxTree ToSyntaxTree( CSharpParseOptions options , string path = "")
    {
        var unannotatedTree = CSharpSyntaxTree.ParseText( this.Text, options, path: path );
        var rewriter = new Rewriter( this );

        return unannotatedTree.WithRootAndOptions( rewriter.Visit( unannotatedTree.GetRoot() )!, options );

    }

    class Rewriter : SafeSyntaxRewriter
    {
        private readonly ImmutableDictionaryOfArray<int,SerializableAnnotation> _noteAnnotations;
        private readonly ImmutableDictionaryOfArray<int,SerializableAnnotation> _tokenAnnotations;

        public Rewriter( SerializableAnnotatedSyntaxTree parent )
        {
            this._noteAnnotations = parent.NodeAnnotations.ToMultiValueDictionary( a => a.SpanStart );
            this._tokenAnnotations = parent.TokenAnnotations.ToMultiValueDictionary( a => a.SpanStart );

        }

        public override SyntaxToken VisitToken( SyntaxToken token )
        {
            var annotations = this._tokenAnnotations[token.SpanStart];

            if ( annotations.IsDefaultOrEmpty )
            {
                return token;
            }
            else
            {
                return token.WithAdditionalAnnotations( annotations.Where( a => a.SpanLength == token.Span.Length ).Select( a => a.ToSyntaxAnnotation() ) );
            }
        }

        public override SyntaxNode? DefaultVisit( SyntaxNode node )
        {
            var rewrittenNode = base.DefaultVisit( node )!;
            
            var annotations = this._tokenAnnotations[node.SpanStart];

            if ( annotations.IsDefaultOrEmpty )
            {
                return rewrittenNode;
            }
            else
            {
                return rewrittenNode.WithAdditionalAnnotations(
                    annotations.Where( a => a.SpanLength == node.Span.Length ).Select( a => a.ToSyntaxAnnotation() ) );
            }
        }
    }

}