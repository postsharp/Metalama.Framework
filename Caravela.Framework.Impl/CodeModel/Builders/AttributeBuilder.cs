// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilder : DeclarationBuilder, IAttributeBuilder, IObservableTransformation
    {
        public AttributeBuilder( DeclarationBuilder containingDeclaration, IConstructor constructor, IReadOnlyList<TypedConstant> constructorArguments ) : base(
            containingDeclaration.ParentAdvice )
        {
            this.ContainingDeclaration = containingDeclaration;
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

        public override IDeclaration ContainingDeclaration { get; }

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Aspect;

        IDeclaration? IDeclaration.ContainingDeclaration => throw new NotImplementedException();

        IAttributeList IDeclaration.Attributes => AttributeList.Empty;

        public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        INamedType IAttribute.Type => this.Constructor.DeclaringType;

        public IConstructor Constructor { get; }

        public IReadOnlyList<TypedConstant> ConstructorArguments { get; }

        INamedArgumentList IAttribute.NamedArguments => this.NamedArguments;
    }
}