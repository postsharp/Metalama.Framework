// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class Member : MemberOrNamedType, IMember
    {
        protected Member( CompilationModel compilation, ISymbol symbol ) : base( compilation, symbol ) { }

        public abstract bool IsExplicitInterfaceImplementation { get; }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public abstract bool IsAsync { get; }

        public bool IsVirtual => this.Symbol.IsVirtual;

        public bool IsOverride => this.Symbol.IsOverride;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true )
        {
            if ( !this.CanBeInherited )
            {
                return Enumerable.Empty<IDeclaration>();
            }
            else
            {
                return this.GetDerivedDeclarationsCore( deep );
            }
        }

        private IEnumerable<IDeclaration> GetDerivedDeclarationsCore( bool deep )
        {
            foreach ( var derivedType in this.Compilation.GetDerivedTypes( this.DeclaringType, deep ) )
            {
                foreach ( var member in ((INamedTypeInternal) derivedType).GetOverridingMembers( this ) )
                {
                    yield return member;
                }
            }
        }

        public override bool CanBeInherited
            => this.DeclaringType.TypeKind == TypeKind.Interface
               || ((this.IsAbstract || (this.IsVirtual && !this.IsSealed)) && ((IDeclarationImpl) this.DeclaringType).CanBeInherited);
    }
}