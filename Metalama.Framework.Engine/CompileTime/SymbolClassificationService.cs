// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class SymbolClassificationService : ISymbolClassificationService
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