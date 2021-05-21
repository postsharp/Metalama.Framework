// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal sealed class GenericParameterBuilder : DeclarationBuilder, IGenericParameterBuilder
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

        public Type ToType() => throw new NotImplementedException();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public override IDeclaration? ContainingElement { get; }

        public override DeclarationKind ElementKind => DeclarationKind.GenericParameter;

        public GenericParameterBuilder( MethodBuilder containingMethod, IGenericParameter template ) : base( containingMethod.ParentAdvice )
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

        // TODO: How to implement this?
        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            return this.Name;
        }
    }
}