// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilder : DeclarationBuilder, IAttributeBuilder, IObservableTransformation
    {
        public AttributeBuilder( DeclarationBuilder containingElement, IConstructor constructor, IReadOnlyList<TypedConstant> constructorArguments ) : base(
            containingElement.ParentAdvice )
        {
            this.ContainingElement = containingElement;
            this.ConstructorArguments = constructorArguments;
            this.Constructor = constructor;
        }

        public NamedArgumentsList NamedArguments { get; } = new();

        public void AddNamedArgument( string name, object? value )
        {
            if ( value != null )
            {
                var type = this.Compilation.Factory.GetTypeByReflectionType( value.GetType() );
                this.NamedArguments.Add( new KeyValuePair<string, TypedConstant>( name, new TypedConstant( type, value ) ) );
            }
            else
            {
                this.NamedArguments.Add( new KeyValuePair<string, TypedConstant>( name, TypedConstant.Null ) );
            }
        }

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

        public override IDeclaration ContainingElement { get; }

        CodeOrigin IDeclaration.Origin => CodeOrigin.Aspect;

        IDeclaration? IDeclaration.ContainingElement => throw new NotImplementedException();

        IAttributeList IDeclaration.Attributes => AttributeList.Empty;

        public override DeclarationKind ElementKind => DeclarationKind.Attribute;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        INamedType IAttribute.Type => this.Constructor.DeclaringType;

        public IConstructor Constructor { get; }

        public IReadOnlyList<TypedConstant> ConstructorArguments { get; }

        INamedArgumentList IAttribute.NamedArguments => this.NamedArguments;
    }
}