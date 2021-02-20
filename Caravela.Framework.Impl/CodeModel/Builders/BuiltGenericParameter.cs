using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltGenericParameter : BuiltCodeElement, IGenericParameter, ICodeElementLink<IGenericParameter>
    {
        public BuiltGenericParameter(GenericParameterBuilder builder, CompilationModel compilation) : base(compilation)
        {
            this.GenericParameterBuilder = builder;
        }
        
        public GenericParameterBuilder GenericParameterBuilder { get; }

        public override CodeElementBuilder Builder => this.GenericParameterBuilder;

        public TypeKind TypeKind => TypeKind.GenericParameter;

        public ITypeFactory TypeFactory => this.Compilation.Factory;

        public string Name => this.GenericParameterBuilder.Name;

        public int Index => this.GenericParameterBuilder.Index;

        public IReadOnlyList<IType> TypeConstraints => throw new NotImplementedException();

        public bool IsCovariant => this.GenericParameterBuilder.IsCovariant;

        public bool IsContravariant => this.GenericParameterBuilder.IsContravariant;

        public bool HasDefaultConstructorConstraint => this.GenericParameterBuilder.HasDefaultConstructorConstraint;

        public bool HasReferenceTypeConstraint => this.GenericParameterBuilder.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this.GenericParameterBuilder.HasNonNullableValueTypeConstraint;

        public IGenericParameter GetForCompilation( CompilationModel compilation ) =>
            compilation == this.Compilation ? this : throw new AssertionFailedException();

        public bool Equals( IType other ) => throw new NotImplementedException();
    }
}