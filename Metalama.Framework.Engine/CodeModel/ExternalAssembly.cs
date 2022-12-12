// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class ExternalAssembly : Declaration, IAssembly
    {
        private readonly IAssemblySymbol _assemblySymbol;

        public ExternalAssembly( IAssemblySymbol assemblySymbol, CompilationModel compilation ) : base( compilation, assemblySymbol )
        {
            this._assemblySymbol = assemblySymbol;
        }

        public override IDeclaration ContainingDeclaration => this.Compilation;

        public override DeclarationKind DeclarationKind => DeclarationKind.AssemblyReference;

        public override ISymbol Symbol => this._assemblySymbol;

        public override bool CanBeInherited => false;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        public INamespace GlobalNamespace => this.Compilation.Factory.GetNamespace( this._assemblySymbol.GlobalNamespace );

        bool IAssembly.IsExternal => true;

        [Memo]
        public IAssemblyIdentity Identity => new AssemblyIdentityModel( this._assemblySymbol.Identity );

        [Memo]
        public INamedTypeCollection Types => new ExternalTypeCollection( this._assemblySymbol, this.Compilation, false );

        [Memo]
        public INamedTypeCollection AllTypes => new ExternalTypeCollection( this._assemblySymbol, this.Compilation, true );

        public override SyntaxTree? PrimarySyntaxTree => null;

        public override IDeclarationOrigin Origin => DeclarationOrigin.External;
    }
}