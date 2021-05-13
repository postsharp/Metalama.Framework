// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating.Mapping
{
    internal partial class TextMap
    {
        private class Visitor : CSharpSyntaxWalker
        {
            private readonly ILocationAnnotationMap _annotationMap;

            public Visitor( ILocationAnnotationMap annotationMap )
            {
                this._annotationMap = annotationMap;
            }

            public string? SourcePath { get; private set; }

            public SkipListIndexedDictionary<int, TextPointMapping> TextPointMappings { get; } = new();

            private enum PointKind
            {
                Start,
                End
            }

            private static TextPoint GetTextPoint( Location location, PointKind kind )
                => kind switch
                {
                    PointKind.Start => new TextPoint( location.SourceSpan.Start, location.GetLineSpan().StartLinePosition ),
                    PointKind.End => new TextPoint( location.SourceSpan.End, location.GetLineSpan().EndLinePosition ),
                    _ => throw new AssertionFailedException()
                };

            private static TextPointMapping GetTextPointMapping( Location source, Location target, PointKind kind )
                => new( GetTextPoint( source, kind ), GetTextPoint( target, kind ) );

            private void ProcessNodeOrToken( SyntaxNodeOrToken node )
            {
                var targetLocation = node.GetLocation();

                if ( targetLocation == null )
                {
                    return;
                }

                var sourceLocation = this._annotationMap.GetLocation( node );

                if ( sourceLocation != null )
                {
                    if ( !string.IsNullOrEmpty( sourceLocation.SourceTree?.FilePath ) )
                    {
                        if ( this.SourcePath == null )
                        {
                            this.SourcePath = sourceLocation.SourceTree!.FilePath;
                        }

                        if ( !this.TextPointMappings.ContainsKey( targetLocation.SourceSpan.Start ) )
                        {
                            _ = this.TextPointMappings.Add(
                                targetLocation.SourceSpan.Start,
                                GetTextPointMapping( sourceLocation, targetLocation, PointKind.Start ) );
                        }

                        if ( !this.TextPointMappings.ContainsKey( targetLocation.SourceSpan.End ) )
                        {
                            _ = this.TextPointMappings.Add(
                                targetLocation.SourceSpan.End,
                                GetTextPointMapping( sourceLocation, targetLocation, PointKind.End ) );
                        }
                    }
                }
            }

            public override void Visit( SyntaxNode? node )
            {
                this.ProcessNodeOrToken( node );
                base.Visit( node );
            }

            public override void VisitToken( SyntaxToken token )
            {
                this.ProcessNodeOrToken( token );
                base.VisitToken( token );
            }
        }
    }
}