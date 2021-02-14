// unset

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class MemberBuilder : CodeElementBuilder, IMemberBuilder, IMemberIntroduction, IObservableTransformation
    {
        protected Advice ParentAdvice { get; }
        
        public bool IsSealed { get; set; }

        public bool IsOverride { get; set; }

        public bool IsNew { get; set; }

        public bool IsAsync { get; set; }

        public INamedType DeclaringType { get; }

        public Accessibility Accessibility { get; set; }

        public string Name { get; set; }

        public bool IsAbstract { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public sealed override ICodeElement? ContainingElement => this.DeclaringType;

        public MemberBuilder( Advice parentAdvice, INamedType declaringType )
        {
            this.ParentAdvice = parentAdvice;
            this.DeclaringType = declaringType;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers();

        public abstract MemberDeclarationSyntax InsertPositionNode { get; }

        SyntaxTree ISyntaxTreeIntroduction.TargetSyntaxTree => throw new System.NotImplementedException();
    }
}