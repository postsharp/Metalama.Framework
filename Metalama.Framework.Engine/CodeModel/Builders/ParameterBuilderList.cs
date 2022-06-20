// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ParameterBuilderList : List<BaseParameterBuilder>, IParameterList
    {
        public ParameterBuilderList() { }

        public ParameterBuilderList( IEnumerable<BaseParameterBuilder> parameterBuilders ) : base( parameterBuilders ) { }

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<BaseParameterBuilder> AsBuilderList => this;

        IParameter IParameterList.this[ string name ] => this.Single<IParameter>( p => p.Name == name );
    }
}