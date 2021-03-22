// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Tests.Integration.Templating
{
    internal class UsedSyntaxKindsCollector : CSharpSyntaxWalker
    {
        private bool _visitingTemplateClass;

        public HashSet<SyntaxKind> CollectedSyntaxKinds { get; } = new HashSet<SyntaxKind>();

        public override void Visit( SyntaxNode? node )
        {
            if ( node == null )
            {
                throw new ArgumentNullException( nameof( node ) );
            }

            base.Visit( node );

            if ( this._visitingTemplateClass )
            {
                this.CollectedSyntaxKinds.Add( node.Kind() );
            }
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            if ( node.Identifier.ValueText == "Aspect" )
            {
                this._visitingTemplateClass = true;
                this.CollectedSyntaxKinds.Add( node.Kind() );
            }

            base.VisitClassDeclaration( node );

            this._visitingTemplateClass = false;
        }
    }
}
