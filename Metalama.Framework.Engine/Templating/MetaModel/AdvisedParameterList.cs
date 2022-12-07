// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal partial class AdvisedParameterList : IAdvisedParameterList, IAdviseParameterValueList
    {
        private readonly IHasParameters _method;
        private readonly ImmutableArray<AdvisedParameter> _parameters;

        public AdvisedParameterList( IHasParameters method )
        {
            this._method = method;
            this._parameters = method.Parameters.SelectAsImmutableArray( p => new AdvisedParameter( (IParameterImpl) p ) );
        }

        public CompilationModel Compilation => (CompilationModel) this._method.Compilation;

        public IAdvisedParameter this[ int index ] => this._parameters[index];

        public int Count => this._parameters.Length;

        public IEnumerator<IAdvisedParameter> GetEnumerator() => ((IEnumerable<IAdvisedParameter>) this._parameters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IAdvisedParameter this[ string name ]
            => this.SingleOrDefault( p => p.Name == name )
               ?? throw new KeyNotFoundException( $"There is no parameter named '{name}' in method '{this._method.ToDisplayString()}" );

        public IEnumerable<IAdvisedParameter> OfType( IType type ) => this.Where( p => p.Type.Is( type ) );

        public IEnumerable<IAdvisedParameter> OfType( Type type ) => this.OfType( this.Compilation.Factory.GetTypeByReflectionType( type ).AssertNotNull() );

        public IAdviseParameterValueList Values => this;

        object IAdviseParameterValueList.ToArray() => new ToArrayImpl( this );
    }
}