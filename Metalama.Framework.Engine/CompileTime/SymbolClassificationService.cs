// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CompileTime
{
    internal class SymbolClassificationService : ISymbolClassificationService
    {
        private readonly CompilationServices _compilationServices;

        public SymbolClassificationService( CompilationServices compilationServices )
        {
            this._compilationServices = compilationServices;
        }

        public ExecutionScope GetExecutionScope( ISymbol symbol )
            => this._compilationServices.SymbolClassifier.GetTemplatingScope( symbol ).ToExecutionScope();

        public bool IsTemplate(  ISymbol symbol ) => !this._compilationServices.SymbolClassifier.GetTemplateInfo( symbol ).IsNone;
    }
}