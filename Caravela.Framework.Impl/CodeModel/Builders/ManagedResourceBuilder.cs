// unset

using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal class ManagedResourceBuilder : ICodeElementBuilder, IIntroducedElement
    {
        public ManagedResourceBuilder(IAdvice advice) 
        {
            // TODO
        }
        
        
        public ResourceDescription ToResourceDescription()
        {
            throw new System.NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new System.NotImplementedException();

        public bool Equals( ICodeElement other ) => throw new System.NotImplementedException();

        public ICodeElement? ContainingElement => null;

        public IReadOnlyList<IAttribute> Attributes => Array.Empty<IAttribute>();

        public CodeElementKind ElementKind => CodeElementKind.Resource;

        public bool IsReadOnly => true;

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments ) => throw new NotSupportedException();

        public SyntaxTree TargetSyntaxTree => null;
    }
}