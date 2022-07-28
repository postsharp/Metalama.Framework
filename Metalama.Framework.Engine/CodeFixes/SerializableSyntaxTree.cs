// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.CodeFixes;

public class SerializableSyntaxTree
{
    public string FilePath { get; }

    public string SourceText { get; }

    public ImmutableArray<SerializableAnnotation> Annotations { get; }

    [JsonConstructor]
    public SerializableSyntaxTree( string filePath, string sourceText, ImmutableArray<SerializableAnnotation> annotations )
    {
        this.FilePath = filePath;
        this.SourceText = sourceText;
        this.Annotations = annotations;
    }

    public SerializableSyntaxTree( string filePath, SyntaxNode syntaxRoot )
    {
        this.FilePath = filePath;
        this.SourceText = syntaxRoot.ToFullString();

        var annotationFinder = new AnnotationReader();
        annotationFinder.Visit( syntaxRoot );
        this.Annotations = annotationFinder.GetAnnotations();
    }

    public SerializableSyntaxTree( SyntaxTree tree ) : this( tree.FilePath, tree.GetRoot() ) { }

    public SyntaxNode GetAnnotatedSyntaxNode( CancellationToken cancellationToken = default )
    {
        var syntaxRoot = SyntaxFactory.ParseCompilationUnit( this.SourceText );

        if ( !this.Annotations.IsDefaultOrEmpty )
        {
            foreach ( var annotation in this.Annotations )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var realAnnotation = annotation.Kind switch
                {
                    SerializableAnnotationKind.Formatter => Formatter.Annotation,
                    SerializableAnnotationKind.Simplifier => Simplifier.Annotation,
                    SerializableAnnotationKind.GeneratedCode => FormattingAnnotations.SystemGeneratedCodeAnnotation,
                    SerializableAnnotationKind.SourceCode => FormattingAnnotations.SourceCodeAnnotation,
                    _ => throw new AssertionFailedException()
                };

                var node = syntaxRoot.FindNode( annotation.TextSpan );
                syntaxRoot = syntaxRoot.ReplaceNode( node, node.WithAdditionalAnnotations( realAnnotation ) );
            }
        }

        return syntaxRoot;
    }

    private class AnnotationReader : SafeSyntaxWalker
    {
        private readonly ImmutableArray<SerializableAnnotation>.Builder _annotations = ImmutableArray.CreateBuilder<SerializableAnnotation>();

        public ImmutableArray<SerializableAnnotation> GetAnnotations() => this._annotations.ToImmutable();

        private void AddAnnotation( SyntaxNode node, SerializableAnnotationKind kind )
        {
            this._annotations.Add( new SerializableAnnotation( node.Span, kind ) );
        }

        public override void DefaultVisit( SyntaxNode node )
        {
            if ( node.ContainsAnnotations )
            {
                if ( node.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                {
                    this.AddAnnotation( node, SerializableAnnotationKind.GeneratedCode );
                }
                else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
                {
                    this.AddAnnotation( node, SerializableAnnotationKind.SourceCode );
                }
                else if ( node.HasAnnotation( Simplifier.Annotation ) )
                {
                    this.AddAnnotation( node, SerializableAnnotationKind.Simplifier );
                }
                else if ( node.HasAnnotation( Formatter.Annotation ) )
                {
                    this.AddAnnotation( node, SerializableAnnotationKind.Formatter );
                }
            }

            base.DefaultVisit( node );
        }
    }
}

public readonly struct SerializableAnnotation
{
    public TextSpan TextSpan { get; }

    public SerializableAnnotationKind Kind { get; }

    public SerializableAnnotation( TextSpan textSpan, SerializableAnnotationKind kind )
    {
        this.TextSpan = textSpan;
        this.Kind = kind;
    }
}

public enum SerializableAnnotationKind
{
    GeneratedCode,
    SourceCode,
    Simplifier,
    Formatter
}