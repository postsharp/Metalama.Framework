// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CompileTime;

internal partial class CompileTimeCompilationBuilder
{
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    private sealed partial class ProduceCompileTimeCodeRewriter
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private class Context : IDisposable
        {
            private readonly ProduceCompileTimeCodeRewriter _parent;
            private readonly Context _oldContext;

            public Context(
                TemplatingScope scope,
                INamedTypeSymbol? nestedType,
                string? nestedTypeNewName,
                int nestingLevel,
                ProduceCompileTimeCodeRewriter parent )
            {
                this.Scope = scope;
                this.NestedTypeNewName = nestedTypeNewName;
                this.NestingLevel = nestingLevel;
                this.NestedType = nestedType;
                this._parent = parent;

                // This will be null for the root context.
                this._oldContext = parent._currentContext;
            }

            public TemplatingScope Scope { get; }

            public string? NestedTypeNewName { get; }

            public int NestingLevel { get; }

            public INamedTypeSymbol? NestedType { get; }

            public void Dispose() => this._parent._currentContext = this._oldContext;
        }
    }
}