// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime
{
    internal sealed class SymbolClassificationService : ISymbolClassificationService
    {
        private readonly CompilationContext _compilationContext;

        public SymbolClassificationService( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;
        }

        public ExecutionScope GetExecutionScope( ISymbol symbol ) => this._compilationContext.SymbolClassifier.GetTemplatingScope( symbol ).ToExecutionScope();

        public bool IsTemplate( ISymbol symbol ) => !this._compilationContext.SymbolClassifier.GetTemplateInfo( symbol ).IsNone;
    }
}