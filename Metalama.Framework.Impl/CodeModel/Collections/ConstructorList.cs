// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CodeModel.Collections
{
    internal class ConstructorList : MethodBaseList<IConstructor>, IConstructorList
    {
        public ConstructorList( Declaration? containingDeclaration, IEnumerable<MemberRef<IConstructor>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?>? argumentTypes )
        {
            return this.OfCompatibleSignature(
                (argumentTypes, this.ContainingDeclaration.AssertNotNull().Compilation),
                null,
                argumentTypes?.Count,
                GetParameter,
                false,
                true );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<Type?>? ArgumentTypes, ICompilation Compilation) context, int index )
                => context.ArgumentTypes != null && context.ArgumentTypes[index] != null
                    ? (context.Compilation.TypeFactory.GetTypeByReflectionType( context.ArgumentTypes[index].AssertNotNull() ), null)
                    : (null, null);
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null )
        {
            return this.OfCompatibleSignature( (argumentTypes, refKinds), null, argumentTypes?.Count, GetParameter, false, true );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<IType?>? ArgumentTypes, IReadOnlyList<RefKind?>? RefKinds) context, int index )
                => (context.ArgumentTypes?[index], context.RefKinds?[index]);
        }

        public IConstructor? OfExactSignature( IConstructor signatureTemplate )
        {
            return this.OfExactSignature( signatureTemplate, null, signatureTemplate.Parameters.Count, GetParameter, false, true );

            static (IType Type, RefKind RefKind) GetParameter( IConstructor context, int index )
                => (context.Parameters[index].Type, context.Parameters[index].RefKind);
        }

        public IConstructor? OfExactSignature( IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null )
        {
            return this.OfExactSignature( (parameterTypes, refKinds), null, parameterTypes.Count, GetParameter, false, true );

            static (IType Type, RefKind RefKind) GetParameter( (IReadOnlyList<IType> ParameterTypes, IReadOnlyList<RefKind>? RefKinds) context, int index )
                => (context.ParameterTypes[index], context.RefKinds?[index] ?? RefKind.None);
        }

        protected override int GetGenericParameterCount( IConstructor x )
        {
            // Constructors don't have generic parameters.
            return 0;
        }

        protected override MethodBaseList<IConstructor> GetMemberListForType( INamedType declaringType )
        {
            throw new NotSupportedException();
        }
    }
}