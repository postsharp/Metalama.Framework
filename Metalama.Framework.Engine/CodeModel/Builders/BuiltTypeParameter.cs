// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltTypeParameter : BuiltDeclaration, ITypeParameter
    {
        public BuiltTypeParameter( TypeParameterBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.TypeParameterBuilder = builder;
        }

        public TypeParameterBuilder TypeParameterBuilder { get; }

        public override DeclarationBuilder Builder => this.TypeParameterBuilder;

        public TypeKind TypeKind => TypeKind.TypeParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => throw new NotImplementedException();

        public bool? IsReferenceType => this.TypeParameterBuilder.IsReferenceType;

        public bool? IsNullable => this.TypeParameterBuilder.IsNullable;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public string Name => this.TypeParameterBuilder.Name;

        public int Index => this.TypeParameterBuilder.Index;

        public IReadOnlyList<IType> TypeConstraints => this.TypeParameterBuilder.ReadOnlyTypeConstraints;

        public TypeKindConstraint TypeKindConstraint => this.TypeParameterBuilder.TypeKindConstraint;

        public VarianceKind Variance => this.TypeParameterBuilder.Variance;

        public bool? IsConstraintNullable => this.TypeParameterBuilder.IsConstraintNullable;

        public bool HasDefaultConstructorConstraint => this.TypeParameterBuilder.HasDefaultConstructorConstraint;
    }
}