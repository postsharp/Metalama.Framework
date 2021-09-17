// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MethodBaseList<IMethod>, IMethodList
    {
        // TODO: Finish generics for OfCompatibleSignature (now matching is done only for number of parameters).

        public static MethodList Empty { get; } = new();

        private MethodList() { }

        public MethodList( Declaration? containingDeclaration, IEnumerable<MemberRef<IMethod>> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        public IEnumerable<IMethod> OfCompatibleSignature(
            string name,
            int? genericParameterCount,
            IReadOnlyList<Type?>? argumentTypes,
            bool? isStatic = false,
            bool declaredOnly = true )
        {
            return this.OfCompatibleSignature(
                (argumentTypes, this.ContainingDeclaration.AssertNotNull().Compilation),
                name,
                genericParameterCount,
                argumentTypes?.Count,
                GetParameter,
                isStatic,
                declaredOnly );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<Type?>? ArgumentTypes, ICompilation Compilation) context, int index )
                => context.ArgumentTypes != null && context.ArgumentTypes[index] != null
                    ? (context.Compilation.TypeFactory.GetTypeByReflectionType( context.ArgumentTypes[index].AssertNotNull() ), null)
                    : (null, null);
        }

        public IEnumerable<IMethod> OfCompatibleSignature(
            string name,
            int? genericParameterCount = null,
            IReadOnlyList<IType?>? argumentTypes = null,
            IReadOnlyList<RefKind?>? refKinds = null,
            bool? isStatic = false,
            bool declaredOnly = true )
        {
            return this.OfCompatibleSignature(
                (argumentTypes, refKinds),
                name,
                genericParameterCount,
                argumentTypes?.Count,
                GetParameter,
                isStatic,
                declaredOnly );

            static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<IType?>? ArgumentTypes, IReadOnlyList<RefKind?>? RefKinds) context, int index )
                => (context.ArgumentTypes?[index], context.RefKinds?[index]);
        }

        public IMethod? OfExactSignature(
            string name,
            int genericParameterCount,
            IReadOnlyList<IType> parameterTypes,
            IReadOnlyList<RefKind>? refKinds = null,
            bool? isStatic = false,
            bool declaredOnly = true )
        {
            return this.OfExactSignature(
                (parameterTypes, refKinds),
                name,
                genericParameterCount,
                parameterTypes.Count,
                GetParameter,
                isStatic,
                declaredOnly );

            static (IType Type, RefKind RefKind) GetParameter( (IReadOnlyList<IType> ParameterTypes, IReadOnlyList<RefKind>? RefKinds) context, int index )
                => (context.ParameterTypes[index], context.RefKinds?[index] ?? RefKind.None);
        }

        public IMethod? OfExactSignature( IMethod signatureTemplate, bool matchIsStatic = true, bool declaredOnly = true )
        {
            return this.OfExactSignature(
                signatureTemplate,
                signatureTemplate.Name,
                signatureTemplate.GenericParameters.Count,
                signatureTemplate.Parameters.Count,
                GetParameter,
                matchIsStatic ? signatureTemplate.IsStatic : null,
                declaredOnly );

            static (IType Type, RefKind RefKind) GetParameter( IMethod context, int index )
                => (context.Parameters[index].ParameterType, context.Parameters[index].RefKind);
        }

        public IEnumerable<IMethod> OfKind( MethodKind kind ) => this.Where( m => m.MethodKind == kind );

        protected override MethodBaseList<IMethod> GetMemberListForType( INamedType declaringType )
        {
            return (MethodBaseList<IMethod>) declaringType.Methods;
        }

        protected override int GetGenericParameterCount( IMethod x )
        {
            return x.GenericParameters.Count;
        }
    }
}