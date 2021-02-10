// unset

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal class AttributeBuilder : IAttributeBuilder
    {
        public AttributeBuilder(  IMethod constructor, IReadOnlyList<object?> constructorArguments )
        {
            this.ConstructorArguments = constructorArguments;
            this.Constructor = constructor;
        }

        public List<KeyValuePair<string,object?>> NamedArguments { get; } = new ();
        
        public void AddNamedArgument( string name, object? value ) => throw new NotImplementedException();

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        bool IEquatable<ICodeElement>.Equals( ICodeElement other ) => throw new NotImplementedException();

        ICodeElement? ICodeElement.ContainingElement => throw new NotImplementedException();

        IReadOnlyList<IAttribute> ICodeElement.Attributes => Array.Empty<IAttribute>();

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Attribute;

        INamedType IAttribute.Type => this.Constructor.DeclaringType;

        public IMethod Constructor { get; }

        public IReadOnlyList<object?> ConstructorArguments { get; }

        IReadOnlyList<KeyValuePair<string,object?>> IAttribute.NamedArguments => this.NamedArguments;
    }
    
    
}