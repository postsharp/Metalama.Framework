// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class MemberBuilder : CodeElementBuilder, IMemberBuilder, IMemberIntroduction, IObservableTransformation
    {
        public bool IsSealed { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsOverride { get; set; }

        public bool IsNew { get; set; }

        public bool IsAsync { get; set; }

        public INamedType DeclaringType { get; }

        public Accessibility Accessibility { get; set; }

        public string Name { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public sealed override ICodeElement ContainingElement => this.DeclaringType;

        public MemberBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice )
        {
            this.DeclaringType = declaringType;
            this.Name = name;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        public abstract MemberDeclarationSyntax InsertPositionNode { get; }

        // TODO: This is temporary.
        SyntaxTree ISyntaxTreeTransformation.TargetSyntaxTree => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;
    }
}