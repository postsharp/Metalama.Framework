// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MemberList<IMethod, MemberLink<IMethod>>, IMethodList
    {

        public static MethodList Empty { get; } = new MethodList();

        private MethodList()
        {
        }

        public MethodList( IEnumerable<MemberLink<IMethod>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }

        public IEnumerable<IMethod> OfCompatibleSignature( string name, int genericParameterCount, IReadOnlyList<IType> parameterTypes )
        {
            return
                this.SourceItems
                .Select( c => c.GetForCompilation( this.Compilation.AssertNotNull() ) )
                .Where( c => c.Name == name )
                .Where( c => c.GenericParameters.Count == genericParameterCount )
                .Where( c => c.Parameters.Count == parameterTypes.Count )
                .Where( c =>
                    c.Parameters
                    .Select( ( p, i ) => (p, i) )
                    .All( x => this.Compilation.AssertNotNull().InvariantComparer.Is( x.p.ParameterType, parameterTypes[x.i] ) ) );
        }

        public IMethod? OfExactSignature( IMethod method )
        {
            return
                this.SourceItems
                .Select( c => c.GetForCompilation( this.Compilation.AssertNotNull() ) )
                .Where( c => c.Name == method.Name )
                .Where( c => c.GenericParameters.Count == method.GenericParameters.Count )
                .Where( c => c.Parameters.Count == method.Parameters.Count )
                .Where( c =>
                    c.Parameters
                    .Select( ( p, i ) => (p, i) )
                    .All( x => this.Compilation.AssertNotNull().InvariantComparer.Equals( x.p.ParameterType, method.Parameters[x.i].ParameterType ) ) )
                .SingleOrDefault();
        }
    }
}