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
        public ConstructorList( IEnumerable<MemberLink<IConstructor>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<Type?> parameterTypes )
        {
            return
                this.SourceItems
                .Select( c => c.GetForCompilation( this.Compilation.AssertNotNull() ) )
                .Where( c => c.Parameters.Count == parameterTypes.Count )
                .Where( c =>
                    c.Parameters
                    .Select( ( p, i ) => (p, i) )
                    .All( x => parameterTypes[x.i] == null || this.Compilation.AssertNotNull().InvariantComparer.Is( x.p.ParameterType, parameterTypes[x.i].AssertNotNull() ) ) );
        }

        public IEnumerable<IConstructor> OfCompatibleSignature( IReadOnlyList<IType> parameterTypes )
        {
            return
                this.SourceItems
                .Select( c => c.GetForCompilation( this.Compilation.AssertNotNull() ) )
                .Where( c => c.Parameters.Count == parameterTypes.Count )
                .Where( c =>
                    c.Parameters
                    .Select( ( p, i ) => (p, i) )
                    .All( x => this.Compilation.AssertNotNull().InvariantComparer.Is( x.p.ParameterType, parameterTypes[x.i] ) ) );
        }
    }
}