// unset

using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Caravela.Framework.Impl.Transformations
{
    internal sealed class ParameterBuilder : CodeElementBuilder, IParameterBuilder
    {
        private bool _hasDefaultValue;
        private object? _defaultValue;

        public RefKind RefKind { get; }

        public IType Type { get; set; }

        public string? Name { get; }

        public int Index { get; }

        public OptionalValue DefaultValue { get; set; }

        public bool IsParams { get; set; }

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

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();

        internal ParameterSyntax ToDeclarationSyntax(  )
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;
            return (ParameterSyntax) syntaxGenerator.ParameterDeclaration(
                this.Name,
                syntaxGenerator.TypeExpression( this.Type.GetSymbol() ),
                this.DefaultValue.ToExpressionSyntax( this.Compilation ),
                this.RefKind.ToRoslynRefKind());
        }
    }
    
    
}