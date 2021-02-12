using System;
using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class CodeElementBuilder : ICodeElementBuilder
    {

        public abstract ICodeElement? ContainingElement { get; }

        IReadOnlyList<IAttribute> ICodeElement.Attributes => this.Attributes;

        public List<AttributeBuilder> Attributes { get; } = new List<AttributeBuilder>();

        public abstract CodeElementKind ElementKind { get; }

        public ICompilation Compilation => this.ContainingElement?.Compilation ?? throw new AssertionFailedException();

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public abstract bool Equals( ICodeElement other );

        public bool IsReadOnly { get; private set; }

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments ) => throw new System.NotImplementedException();

        public void RemoveAttributes( INamedType type ) => throw new NotImplementedException();

        public virtual void Freeze()
        {
            this.IsReadOnly = true;
        }
    }
}
