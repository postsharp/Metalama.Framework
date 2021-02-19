﻿using System;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// Base class implementing <see cref="ICodeElementBuilder"/>. These classes are returned by introduction advices so the user can continue
    /// specifying the introduced code element. They are bound to the <see cref="CompilationModel"/> that created them, but implement
    /// <see cref="ICodeElementLink{T}"/> so they can resolve, using <see cref="CodeElementFactory"/>, to the consuming <see cref="CompilationModel"/>.
    /// 
    /// </summary>
    internal abstract class CodeElementBuilder : ICodeElementBuilder, ICodeElementLink<ICodeElement>
    {

        public abstract ICodeElement? ContainingElement { get; }

        IAttributeList ICodeElement.Attributes => this.Attributes;

        public AttributeBuilderList Attributes { get; } = new AttributeBuilderList();

        public abstract CodeElementKind ElementKind { get; }

        ICompilation ICodeElement.Compilation => this.Compilation;

        public CompilationModel Compilation => (CompilationModel?) this.ContainingElement?.Compilation ?? throw new AssertionFailedException();

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        
        public bool IsReadOnly { get; private set; }

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments ) => throw new System.NotImplementedException();

        public void RemoveAttributes( INamedType type ) => throw new NotImplementedException();

        public virtual void Freeze()
        {
            this.IsReadOnly = true;
        }

        public IDiagnosticLocation? DiagnosticLocation => this.ContainingElement?.DiagnosticLocation;

        ICodeElement ICodeElementLink<ICodeElement>.GetForCompilation( CompilationModel compilation ) => this.GetForCompilation( compilation );
        protected abstract ICodeElement GetForCompilation( CompilationModel compilation );

        // We are a link to ourselves.
        object? ICodeElementLink.Target => this;
    }
}
