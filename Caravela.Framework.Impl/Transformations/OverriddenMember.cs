// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class OverriddenMember : INonObservableTransformation, IMemberIntroduction, IOverriddenDeclaration
    {
        public Advice Advice { get; }

        public IMember OverriddenDeclaration { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        IDeclaration IOverriddenDeclaration.OverriddenDeclaration => this.OverriddenDeclaration;

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public OverriddenMember( Advice advice, IMember overriddenDeclaration, AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.LinkerOptions = linkerOptions;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        public MemberDeclarationSyntax InsertPositionNode
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var memberSymbol = (this.OverriddenDeclaration as Member)?.Symbol;

                if ( memberSymbol != null )
                {
                    return memberSymbol.DeclaringSyntaxReferences.Select( x => (MemberDeclarationSyntax) x.GetSyntax() ).First();
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
            }
        }
    }
}
