// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.DeclarationBuilders;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class ParameterBuilderList : List<IParameterBuilder>, IParameterList
    {
        public ParameterBuilderList() { }

        public ParameterBuilderList( IEnumerable<IParameterBuilder> parameterBuilders ) : base( parameterBuilders ) { }

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        IParameter IReadOnlyList<IParameter>.this[int index] => this[index];

        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<IParameterBuilder> AsBuilderList => this;

        public IParameter this[string name] => this.Single<IParameter>( p => p.Name == name );
    }
}