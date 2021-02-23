using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilder : CodeElementBuilder, IAttributeBuilder, IObservableTransformation
    {
        public AttributeBuilder( ICodeElement containingElement, IMethod constructor, IReadOnlyList<object?> constructorArguments )
        {
            this.ContainingElement = containingElement;
            this.ConstructorArguments = constructorArguments;
            this.Constructor = constructor;
        }

        public NamedArgumentsList NamedArguments { get; } = new();

        public void AddNamedArgument( string name, object? value ) => throw new NotImplementedException();

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

        public override ICodeElement ContainingElement { get; }

        CodeOrigin ICodeElement.Origin => CodeOrigin.Aspect;

        ICodeElement? ICodeElement.ContainingElement => throw new NotImplementedException();

        IAttributeList ICodeElement.Attributes => AttributeList.Empty;

        public override CodeElementKind ElementKind => CodeElementKind.Attribute;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        INamedType IAttribute.Type => this.Constructor.DeclaringType;

        public IMethod Constructor { get; }

        public IReadOnlyList<object?> ConstructorArguments { get; }

        INamedArgumentList IAttribute.NamedArguments => this.NamedArguments;
    }
}