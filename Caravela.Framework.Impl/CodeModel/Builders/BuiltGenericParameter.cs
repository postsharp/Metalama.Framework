// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

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

        ICompilation ICompilationElement.Compilation => this.Compilation;

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