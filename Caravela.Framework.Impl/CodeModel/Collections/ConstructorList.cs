// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ConstructorList : MemberList<IConstructor, MemberLink<IConstructor>>, IConstructorList
    {
        public ConstructorList( CodeElement? containingElement, IEnumerable<MemberLink<IConstructor>> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?> parameterTypes )
        {
            var compilation = this.ContainingElement.AssertNotNull().Compilation;
            return this.OfSignature( ( i, t ) => parameterTypes[i] == null || compilation.InvariantComparer.Is( t, parameterTypes[i].AssertNotNull() ) );
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType> parameterTypes )
        {
            var compilation = this.ContainingElement.AssertNotNull().Compilation;
            return this.OfSignature( ( i, t ) => parameterTypes[i] == null || compilation.InvariantComparer.Is( t, parameterTypes[i] ) );
        }

        public IConstructor? OfExactSignature( IReadOnlyList<IType> parameterTypes )
        {
            var compilation = this.ContainingElement.AssertNotNull().Compilation;
            return 
                this.OfSignature( ( i, t ) => parameterTypes[i] == null || compilation.InvariantComparer.Equals( t, parameterTypes[i] ) )
                .SingleOrDefault();
        }

        private IEnumerable<IConstructor> OfSignature( Func<int, IType, bool> parameterTypePredicate )
        {
            var compilation = this.ContainingElement.AssertNotNull().Compilation;
            foreach (var sourceItem in this.SourceItems)
            {
                var projectedItem = sourceItem.GetForCompilation( compilation );
                var match = true;

                for (var i = 0; i < projectedItem.Parameters.Count; i++ )
                {
                    if (!parameterTypePredicate(i, projectedItem.Parameters[i].ParameterType))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    yield return projectedItem;
                }
            }
        }
    }
}