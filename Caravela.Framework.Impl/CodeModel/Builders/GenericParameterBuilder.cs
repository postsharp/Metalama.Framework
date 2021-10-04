// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class GenericParameterBuilder : DeclarationBuilder, IGenericParameterBuilder
    {
        private readonly List<IType> _typeConstraints = new();

        public string Name { get; set; }

        public int Index { get; }

        IReadOnlyList<IType> IGenericParameter.TypeConstraints => this._typeConstraints;

        public IReadOnlyList<IType> ReadOnlyTypeConstraints => this._typeConstraints;

        public TypeKindConstraint TypeKindConstraint { get; set; }

        public VarianceKind Variance { get; set; }

        public bool? IsConstraintNullable { get; set; }

        public bool HasDefaultConstructorConstraint { get; set; }

        public void AddTypeConstraint( IType type ) => this._typeConstraints.Add( type );

        public void AddTypeConstraint( Type type ) => this._typeConstraints.Add( this.Compilation.Factory.GetTypeByReflectionType( type ) );

        TypeKind IType.TypeKind => TypeKind.GenericParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => throw new NotImplementedException();

        public bool? IsReferenceType => this.IsReferenceTypeImpl();

        public bool? IsNullable => this.IsNullableImpl();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public override IDeclaration? ContainingDeclaration { get; }

        public override DeclarationKind DeclarationKind => DeclarationKind.GenericParameter;

        public GenericParameterBuilder( MethodBuilder containingMethod, int index, string name ) : base( containingMethod.ParentAdvice )
        {
            this.ContainingDeclaration = containingMethod;
            this.Index = index;
            this.Name = name;
        }

        // TODO: How to implement this?
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            return this.Name;
        }
    }
}