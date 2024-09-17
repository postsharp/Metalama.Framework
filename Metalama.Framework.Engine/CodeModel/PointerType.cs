// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class PointerType : RoslynType<IPointerTypeSymbol>, IPointerType
    {
        internal PointerType( IPointerTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this.Compilation.Factory.GetIType( this.Symbol.PointedAtType );

        public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );

        internal ITypeImpl WithPointedAtType( IType pointedAtType )
        {
            if ( pointedAtType == this.PointedAtType )
            {
                return this;
            }
            else
            {
                var symbol =
                    this.GetCompilationModel()
                        .RoslynCompilation.CreatePointerTypeSymbol(
                            pointedAtType.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) );

                return (ITypeImpl) this.GetCompilationModel().Factory.GetIType( symbol );
            }
        }
    }
}