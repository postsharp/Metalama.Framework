// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class ReferencedAssembly : Declaration, IAssembly
    {
        public ReferencedAssembly( IAssemblySymbol assemblySymbol, CompilationModel compilation ) : base( compilation )
        {
            this.AssemblySymbol = assemblySymbol;
        }

        public override IDeclaration? ContainingDeclaration => this.Compilation;

        public IAssemblySymbol AssemblySymbol { get; }

        public override DeclarationKind DeclarationKind => DeclarationKind.AssemblyReference;

        public override ISymbol Symbol => this.AssemblySymbol;

        public override bool CanBeInherited => false;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        bool IAssembly.IsExternal => true;

        [Memo]
        public IAssemblyIdentity Identity => new AssemblyIdentityModel( this.AssemblySymbol.Identity );
    }
}