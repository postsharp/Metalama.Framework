// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ConstructorList : MethodBaseList<IConstructor>, IConstructorList
    {
        public ConstructorList( CodeElement? containingElement, IEnumerable<MemberLink<IConstructor>> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?>? argumentTypes )
        {
            return this.OfCompatibleSignature( (argumentTypes, this.ContainingElement.AssertNotNull().Compilation), null, 0, argumentTypes?.Count, GetParameter, false, true );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<Type?>? ArgumentTypes, ICompilation Compilation) context, int index )
                => context.ArgumentTypes != null && context.ArgumentTypes[index] != null
                   ? (context.Compilation.TypeFactory.GetTypeByReflectionType( context.ArgumentTypes[index].AssertNotNull() ), null)
                   : (null, null);
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType?>? argumentTypes = null, IReadOnlyList<RefKind?>? refKinds = null )
        {
            return this.OfCompatibleSignature( (argumentTypes, refKinds), null, 0, argumentTypes?.Count, GetParameter, false, true );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<IType?>? ArgumentTypes, IReadOnlyList<RefKind?>? RefKinds) context, int index )
                => (context.ArgumentTypes?[index], context.RefKinds?[index]);
        }

        public IConstructor? OfExactSignature( IConstructor signatureTemplate )
        {
            return this.OfExactSignature( signatureTemplate, null, 0, signatureTemplate.Parameters.Count, GetParameter, false, true );

            static (IType Type, RefKind RefKind) GetParameter( IConstructor context, int index )
                => (context.Parameters[index].ParameterType, context.Parameters[index].RefKind);
        }

        public IConstructor? OfExactSignature( IReadOnlyList<IType> parameterTypes, IReadOnlyList<RefKind>? refKinds = null )
        {
            return this.OfExactSignature( (parameterTypes, refKinds), null, 0, parameterTypes.Count, GetParameter, false, true );

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