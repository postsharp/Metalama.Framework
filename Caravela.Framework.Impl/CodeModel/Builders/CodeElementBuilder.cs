using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class CodeElementBuilder : ICodeElementBuilder, IToSyntax
    {
        
        public abstract ICodeElement? ContainingElement { get; }

        IReadOnlyList<IAttribute> ICodeElement.Attributes => this.Attributes;

        public List<AttributeBuilder> Attributes { get; } = new List<AttributeBuilder>();

        public abstract CodeElementKind ElementKind { get; }

        public abstract CSharpSyntaxNode GetSyntaxNode();

        public abstract IEnumerable<CSharpSyntaxNode> GetSyntaxNodes();

        public abstract MemberDeclarationSyntax GetDeclaration();

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
        
        public abstract bool Equals( ICodeElement other );

        public bool IsReadOnly { get; private set; }

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments ) => throw new System.NotImplementedException();

        public void Freeze()
        {
            this.IsReadOnly = true;
            this.ForEachChild( c => c.Freeze() );
        }

        protected abstract void ForEachChild( Action<CodeElementBuilder> action );
    }
}
