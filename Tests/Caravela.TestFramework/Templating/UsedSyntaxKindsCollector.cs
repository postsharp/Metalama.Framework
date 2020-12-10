using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Caravela.TestFramework.Templating
{
    public class UsedSyntaxKindsCollector : CSharpSyntaxWalker
    {
        public HashSet<SyntaxKind> CollectedSyntaxKinds { get; } = new HashSet<SyntaxKind>();

        public override void Visit( SyntaxNode node )
        {
            base.Visit( node );
            this.CollectedSyntaxKinds.Add( node.Kind() );
        }
    }
}
