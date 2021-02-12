// unset

using System;
using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Transformations
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

        Code.TypeKind IType.TypeKind => Code.TypeKind.GenericParameter;

        // TODO: Here we have a design problem.
        public ITypeFactory TypeFactory => throw new NotSupportedException();

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

        public override bool Equals( ICodeElement other ) => throw new NotImplementedException();
    }
}