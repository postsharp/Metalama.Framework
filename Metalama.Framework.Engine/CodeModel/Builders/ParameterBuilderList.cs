// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class ParameterBuilderList : List<BaseParameterBuilder>, IParameterBuilderList, IParameterList
    {
        public static readonly ParameterBuilderList Empty = new( Array.Empty<BaseParameterBuilder>() );

        public ParameterBuilderList() { }

        public ParameterBuilderList( IEnumerable<BaseParameterBuilder> parameterBuilders ) : base( parameterBuilders ) { }

        IEnumerator<IParameterBuilder> IEnumerable<IParameterBuilder>.GetEnumerator() => this.GetEnumerator();

        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        IParameterBuilder IReadOnlyList<IParameterBuilder>.this[ int index ] => this[index];

        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<BaseParameterBuilder> AsBuilderList => this;

        public IParameterBuilder this[ string name ] => this.Single<IParameterBuilder>( p => p.Name == name );

        int IReadOnlyCollection<IParameter>.Count => this.Count;

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

        IParameter IParameterList.this[ string name ] => this[name];
    }
}