// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class Member : MemberOrNamedType, IMember
    {
        protected Member( CompilationModel compilation ) : base( compilation ) { }

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
                return this.GetDerivedDeclarationsCore();
            }
        }

        private IEnumerable<IDeclaration> GetDerivedDeclarationsCore()
        {
            foreach ( var derivedType in this.Compilation.GetDerivedTypes( this.DeclaringType, true ) )
            {
                foreach ( var member in ((INamedTypeInternal) derivedType).GetOverridingMembers( this ) )
                {
                    yield return member;
                }
            }
        }

        public abstract bool IsImplicit { get; }

        public override bool CanBeInherited
            => this.DeclaringType.TypeKind == TypeKind.Interface
               || (this.IsVirtual && !this.IsSealed && ((IDeclarationImpl) this.DeclaringType).CanBeInherited);
    }
}