using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    class ReturnParameterLink : ICodeElementLink<IParameter>
    {
        
        public ReturnParameterLink( IMethodSymbol method )
        {
            this.Method = method;
        }

        public CodeElementLink<ICodeElement> ToSymbolicLink() => new CodeElementLink<ICodeElement>( this );

        public IMethodSymbol Method { get; }
        public IParameter GetForCompilation( CompilationModel compilation ) => throw new NotImplementedException();

        public ISymbol? Symbol => this.Method;

        public ICodeElementBuilder? Builder => null;

        public object? LinkedObject => this;
    }
}