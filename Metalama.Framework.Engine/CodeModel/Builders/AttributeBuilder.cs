// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Impl.CodeModel.Collections;
using Metalama.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilder : DeclarationBuilder, IAttribute, IObservableTransformation
    {
        private readonly AttributeConstruction _attributeConstruction;

        public AttributeBuilder( DeclarationBuilder containingDeclaration, AttributeConstruction attributeConstruction ) : base(
            containingDeclaration.ParentAdvice )
        {
            this._attributeConstruction = attributeConstruction;
            this.ContainingDeclaration = containingDeclaration;
        }

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

        public override bool CanBeInherited => false;

        public override IDeclaration ContainingDeclaration { get; }

        bool IObservableTransformation.IsDesignTime => false;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Aspect;

        IDeclaration? IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

        IAttributeList IDeclaration.Attributes => AttributeList.Empty;

        public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        public INamedType Type => this.Constructor.DeclaringType;

        public IConstructor Constructor => this._attributeConstruction.Constructor;

        public ImmutableArray<TypedConstant> ConstructorArguments => this._attributeConstruction.ConstructorArguments;

        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments => this._attributeConstruction.NamedArguments;

        IType IHasType.Type => this.Type;

        public FormattableString FormatPredecessor() => $"attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";
    }
}