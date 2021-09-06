// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Namespace : Declaration, INamespace
    {
        private readonly INamespaceSymbol _symbol;

        internal Namespace( INamespaceSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;

        public override ISymbol Symbol => this._symbol;

        public string Name => this._symbol.IsGlobalNamespace ? "" : this._symbol.Name;

        public string FullName => this._symbol.IsGlobalNamespace ? "" : this._symbol.ToDisplayString();

        public bool IsGlobalNamespace => this._symbol.IsGlobalNamespace;

        public INamespace? ParentNamespace => throw new NotImplementedException();

        public IReadOnlyList<INamedType> Types => throw new NotImplementedException();

        public IReadOnlyList<INamespace> ChildrenNamespaces => throw new NotImplementedException();
    }
}