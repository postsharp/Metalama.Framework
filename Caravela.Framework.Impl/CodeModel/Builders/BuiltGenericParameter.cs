using System;
using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltGenericParameter : BuiltCodeElement, IGenericParameter
    {
        public BuiltGenericParameter( GenericParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.GenericParameterBuilder = builder;
        }

        public GenericParameterBuilder GenericParameterBuilder { get; }

        public override CodeElementBuilder Builder => this.GenericParameterBuilder;

        public TypeKind TypeKind => TypeKind.GenericParameter;

        ICompilation IType.Compilation => this.Compilation;

        public string Name => this.GenericParameterBuilder.Name;

        public int Index => this.GenericParameterBuilder.Index;

        public IReadOnlyList<IType> TypeConstraints => throw new NotImplementedException();

        public bool IsCovariant => this.GenericParameterBuilder.IsCovariant;

        public bool IsContravariant => this.GenericParameterBuilder.IsContravariant;

        public bool HasDefaultConstructorConstraint => this.GenericParameterBuilder.HasDefaultConstructorConstraint;

        public bool HasReferenceTypeConstraint => this.GenericParameterBuilder.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this.GenericParameterBuilder.HasNonNullableValueTypeConstraint;
    }
}