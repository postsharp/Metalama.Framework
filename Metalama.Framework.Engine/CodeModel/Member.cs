// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;
using System.Linq;
using TypeKind = Metalama.Framework.Code.TypeKind;

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

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        {
            if ( !this.CanBeInherited )
            {
                return Enumerable.Empty<IDeclaration>();
            }
            else
            {
                return this.GetDerivedDeclarationsCore( options );
            }
        }

        private IEnumerable<IDeclaration> GetDerivedDeclarationsCore( DerivedTypesOptions options )
        {
            foreach ( var derivedType in this.Compilation.GetDerivedTypes( this.DeclaringType, options ) )
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