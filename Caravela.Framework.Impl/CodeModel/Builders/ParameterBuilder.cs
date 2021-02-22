using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class ParameterBuilder : CodeElementBuilder, IParameterBuilder, ICodeElementLink<IParameter>
    {
        private readonly string? _name;

        public RefKind RefKind { get; }

        public IType ParameterType { get; set; }

        public string Name => this._name ?? throw new NotSupportedException( "Cannot get the name of a return parameter." );

        public int Index { get; }

        public OptionalValue DefaultValue { get; set; }

        public bool IsParams { get; set; }

        public override ICodeElement? ContainingElement => this.DeclaringMember;

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        public IMember DeclaringMember { get; }

        public ParameterBuilder( IMethod containingMethod, int index, string? name, IType type, RefKind refKind ) : base()
        {
            this.DeclaringMember = containingMethod;
            this.Index = index;
            this._name = name;
            this.ParameterType = type;
            this.RefKind = refKind;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        // TODO: Implement compilation-consistent model.
        protected override ICodeElement GetForCompilation( CompilationModel compilation ) => this;

        internal ParameterSyntax ToDeclarationSyntax()
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;
            return (ParameterSyntax) syntaxGenerator.ParameterDeclaration(
                this.Name,
                syntaxGenerator.TypeExpression( this.ParameterType.GetSymbol() ),
                this.DefaultValue.ToExpressionSyntax( this.Compilation ),
                this.RefKind.ToRoslynRefKind() );
        }

        IParameter ICodeElementLink<IParameter>.GetForCompilation( CompilationModel compilation ) => (IParameter) this.GetForCompilation( compilation );
    }
}