// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal abstract class AspectReferenceRenamingSubstitution : SyntaxNodeSubstitution
    {
        public ResolvedAspectReference AspectReference { get; }

        public override SyntaxNode TargetNode => this.AspectReference.RootNode;

        public AspectReferenceRenamingSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base( compilationContext )
        {
            this.AspectReference = aspectReference;
        }
        protected SimpleNameSyntax RewriteName( SimpleNameSyntax name, string targetMemberName )
            => name switch
            {
                GenericNameSyntax genericName => genericName.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                IdentifierNameSyntax _ => name.WithIdentifier( Identifier( targetMemberName.AssertNotNull() ) ),
                _ => throw new AssertionFailedException( $"{name.Kind()} is not a supported name." )
            };
    }
}