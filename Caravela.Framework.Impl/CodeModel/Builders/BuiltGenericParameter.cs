// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltGenericParameter : BuiltDeclaration, IGenericParameter
    {
        public BuiltGenericParameter( GenericParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.GenericParameterBuilder = builder;
        }

        public GenericParameterBuilder GenericParameterBuilder { get; }

        public override DeclarationBuilder Builder => this.GenericParameterBuilder;

        public TypeKind TypeKind => TypeKind.GenericParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => throw new NotImplementedException();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public string Name => this.GenericParameterBuilder.Name;

        public int Index => this.GenericParameterBuilder.Index;

        public IReadOnlyList<IType> TypeConstraints => this.GenericParameterBuilder.ReadOnlyTypeConstraints;

        public TypeKindConstraint TypeKindConstraint => this.GenericParameterBuilder.TypeKindConstraint;

        public VarianceKind Variance => this.GenericParameterBuilder.Variance;

        public bool HasDefaultConstructorConstraint => this.GenericParameterBuilder.HasDefaultConstructorConstraint;
    }
}