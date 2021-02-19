using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class ManagedResourceBuilder : INonObservableTransformation
    {

        public ResourceDescription ToResourceDescription()
        {
            throw new System.NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new System.NotImplementedException();

        public bool Equals( ICodeElement other ) => throw new System.NotImplementedException();

        public ICodeElement? ContainingElement => null;

        public IReadOnlyList<IAttribute> Attributes => Array.Empty<IAttribute>();

        public CodeElementKind ElementKind => CodeElementKind.ManagedResource;

        public bool IsReadOnly => true;

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments ) => throw new NotSupportedException();
    }
}