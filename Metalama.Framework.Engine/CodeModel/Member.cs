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
        protected Member( CompilationModel compilation ) : base( compilation ) { }

        public abstract bool IsExplicitInterfaceImplementation { get; }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public abstract bool IsAsync { get; }

        public bool IsVirtual => this.Symbol.IsVirtual;

        public bool IsOverride => this.Symbol.IsOverride;

        public bool HasImplementation
            => this.Symbol switch
            {
                IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: null } => false,
                IFieldSymbol { IsConst: true } => false,
                { IsAbstract: true } => false,
                { IsExtern: true } => false,
                _ => true,
            };

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        {
            if ( !this.CanBeInherited )
            {
                return Enumerable.Empty<IDeclaration>();
            }
            else
            {
                return GetDerivedDeclarationsCore( this, options );
            }
        }

        internal static IEnumerable<IDeclaration> GetDerivedDeclarationsCore( IMember self, DerivedTypesOptions options )
        {
            foreach ( var derivedType in self.Compilation.GetDerivedTypes( self.DeclaringType, options ) )
            {
                foreach ( var member in ((INamedTypeImpl) derivedType).GetOverridingMembers( self ) )
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