// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

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

        public abstract IEnumerable<MemberDeclarationSyntax> GetIntroducedMembers();

        public abstract MemberDeclarationSyntax InsertPositionNode { get; }

        SyntaxTree ISyntaxTreeIntroduction.TargetSyntaxTree => throw new System.NotImplementedException();
    }
}