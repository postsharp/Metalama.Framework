// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Impl.CodeModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdviceParameterList : IAdviceParameterList, IAdviceParameterValueList
    {
        private readonly IHasParameters _method;
        private readonly AdviceParameter[] _parameters;

        public AdviceParameterList( IHasParameters method )
        {
            this._method = method;
            this._parameters = method.Parameters.Select( p => new AdviceParameter( p ) ).ToArray();
        }

        public CompilationModel Compilation => (CompilationModel) this._method.Compilation;

        public IAdviceParameter this[ int index ] => this._parameters[index];

        public int Count => this._parameters.Length;

        public IEnumerator<IAdviceParameter> GetEnumerator() => ((IEnumerable<IAdviceParameter>) this._parameters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IAdviceParameter this[ string name ]
            => this.SingleOrDefault( p => p.Name == name )
               ?? throw new KeyNotFoundException( $"There is no parameter named '{name}' in method '{this._method.ToDisplayString()}" );

        public IEnumerable<IAdviceParameter> OfType( IType type ) => this.Where( p => p.ParameterType.Is( type ) );

        public IEnumerable<IAdviceParameter> OfType( Type type ) => this.OfType( this.Compilation.Factory.GetTypeByReflectionType( type ).AssertNotNull() );

        public IAdviceParameterValueList Values => this;

        dynamic IAdviceParameterValueList.ToArray() => new ToArrayImpl( this );

        dynamic IAdviceParameterValueList.ToValueTuple() => new ToValueTupleImpl( this );
    }
}