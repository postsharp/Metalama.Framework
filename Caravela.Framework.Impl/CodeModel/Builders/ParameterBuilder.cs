// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class ParameterBuilder : CodeElementBuilder, IParameterBuilder
    {
        private bool _hasDefaultValue;
       private object? _defaultValue;

        public Code.RefKind RefKind { get; }

        public IType Type { get; set; }
       

        public string? Name { get; }
    
        public int Index { get; }



        public OptionalValue DefaultValue { get; set; }
      

        public override ICodeElement? ContainingElement { get; }

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        public ParameterBuilder( IMethod containingMethod, int index, string name, IType type, RefKind refKind ) : base()
        {
            this.ContainingElement = containingMethod;
            this.Index = index;
            this.Name = name;
            this.Type = type;
            this.RefKind = refKind;
        }

        public override MemberDeclarationSyntax GetDeclaration()
        {
            throw new NotSupportedException();
        }

        public override CSharpSyntaxNode GetSyntaxNode() => throw new NotSupportedException();

        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => throw new NotSupportedException();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();
        protected override void ForEachChild( Action<CodeElementBuilder> action ) => throw new NotImplementedException();
    }
}