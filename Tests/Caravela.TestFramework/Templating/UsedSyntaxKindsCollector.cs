using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework.Templating
{
    public class UsedSyntaxKindsCollector : CSharpSyntaxWalker
    {
        public HashSet<SyntaxKind> CollectedSyntaxKinds { get; } = new HashSet<SyntaxKind>();

        public override void Visit( SyntaxNode? node )
        {
            if ( node == null )
            {
                throw new ArgumentNullException( nameof( node ) );
            }

            base.Visit( node );
            this.CollectedSyntaxKinds.Add( node.Kind() );
        }
    }
}
