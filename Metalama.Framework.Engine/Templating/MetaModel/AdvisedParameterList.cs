// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal sealed partial class AdvisedParameterList : IAdvisedParameterList, IAdvisedParameterValueList, IParameterList
    {
        private readonly IHasParameters _method;
        private readonly ImmutableArray<AdvisedParameter> _parameters;

        public AdvisedParameterList( IHasParameters method )
        {
            this._method = method;
            this._parameters = method.Parameters.SelectAsImmutableArray( p => new AdvisedParameter( (IParameterImpl) p ) );
        }

        private CompilationModel Compilation => (CompilationModel) this._method.Compilation;

        public IAdvisedParameter this[ int index ] => this._parameters[index];

        public int Count => this._parameters.Length;

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<IAdvisedParameter> GetEnumerator() => ((IEnumerable<IAdvisedParameter>) this._parameters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IParameterList AsParameterList() => this;

        public IAdvisedParameter this[ string name ]
            => this.SingleOrDefault<IAdvisedParameter>( p => p.Name == name )
               ?? throw new KeyNotFoundException( $"There is no parameter named '{name}' in method '{this._method.ToDisplayString()}" );

        public IEnumerable<IAdvisedParameter> OfType( IType type ) => this.Where<IAdvisedParameter>( p => p.Type.Is( type ) );

        public IEnumerable<IAdvisedParameter> OfType( Type type ) => this.OfType( this.Compilation.Factory.GetTypeByReflectionType( type ).AssertNotNull() );

        public IAdvisedParameterValueList Values => this;

        object IAdvisedParameterValueList.ToArray() => new ToArrayImpl( this );

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

        IParameter IParameterList.this[ string name ] => this[name];
    }
}