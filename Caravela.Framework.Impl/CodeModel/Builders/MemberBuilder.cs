// unset

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class MemberTransformationBuilder : CodeElementBuilder, IMemberBuilder, IMemberIntroduction, IObservableTransformation
    {
        public bool IsSealed { get; set; }

        public INamedType DeclaringType { get; }

        public Visibility Visibility { get; set; }

        public string Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsVirtual { get; set; }

        public sealed override ICodeElement? ContainingElement => this.DeclaringType;

        public MemberTransformationBuilder( INamedType declaringType )
        {
            this.DeclaringType = declaringType;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers();

        public abstract MemberDeclarationSyntax InsertPositionNode { get; }

        // TODO: This is temporary.
        SyntaxTree ISyntaxTreeIntroduction.TargetSyntaxTree => 
            ( (NamedType)this.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;
    }
}