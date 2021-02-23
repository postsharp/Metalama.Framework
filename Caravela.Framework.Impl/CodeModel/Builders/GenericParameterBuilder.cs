using System;
using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class GenericParameterBuilder : CodeElementBuilder, IGenericParameterBuilder
    {
        private readonly IGenericParameter _template;

        public string Name => this._template.Name;

        public int Index => this._template.Index;

        IReadOnlyList<IType> IGenericParameter.TypeConstraints => throw new NotImplementedException();

        public IList<IType> TypeConstraints { get; } = new List<IType>();

        public bool IsCovariant { get; set; }

        public bool IsContravariant { get; set; }

        public bool HasDefaultConstructorConstraint { get; set; }

        public bool HasReferenceTypeConstraint { get; set; }

        public bool HasNonNullableValueTypeConstraint { get; set; }

        TypeKind IType.TypeKind => TypeKind.GenericParameter;

        ICompilation IType.Compilation => this.Compilation;

        public override ICodeElement? ContainingElement { get; }

        public override CodeElementKind ElementKind => CodeElementKind.GenericParameter;

        public GenericParameterBuilder( IMethod containingMethod, IGenericParameter template ) : base()
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }
    }
}