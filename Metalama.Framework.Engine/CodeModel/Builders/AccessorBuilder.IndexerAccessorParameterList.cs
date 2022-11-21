// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private sealed class IndexerAccessorParameterList : IParameterList
        {
            private readonly List<IndexerParameter> _parameters;

            public AccessorBuilder Accessor { get; }

            public IndexerAccessorParameterList( AccessorBuilder accessor )
            {
                this.Accessor = accessor;
                this._parameters = new List<IndexerParameter>();

                if ( this.Accessor.MethodKind == MethodKind.PropertySet )
                {
                    this._parameters.Add( new IndexerParameter( this.Accessor, null ) );
                }
            }

            public IndexerBuilder Indexer => (IndexerBuilder) this.Accessor.ContainingMember;

            public IParameter this[ string name ]
                => (this.Accessor.MethodKind, name) switch
                {
                    (MethodKind.PropertySet, "value") => this[this.Count - 1],
                    _ => this[this.Indexer.Parameters[name].Index]
                };

            public IParameter this[ int index ]
            {
                get
                {
                    switch ( this.Accessor.MethodKind )
                    {
                        case MethodKind.PropertySet:
                            while ( this.Indexer.Parameters.Count + 1 > this._parameters.Count )
                            {
                                this._parameters.Insert( this._parameters.Count - 1, new IndexerParameter( this.Accessor, this._parameters.Count - 1 ) );
                            }

                            return this._parameters[index];

                        default:
                            while ( this.Indexer.Parameters.Count > this._parameters.Count )
                            {
                                this._parameters.Add( new IndexerParameter( this.Accessor, this._parameters.Count ) );
                            }

                            return this._parameters[index];
                    }
                }
            }

            public int Count
                => this.Accessor.MethodKind switch
                {
                    MethodKind.PropertySet => this.Indexer.Parameters.Count + 1,
                    _ => this.Indexer.Parameters.Count
                };

            public IEnumerator<IParameter> GetEnumerator()
            {
                for ( var i = 0; i < this.Indexer.Parameters.Count; i++ )
                {
                    yield return this[i];
                }

                if ( this.Accessor.MethodKind == MethodKind.PropertySet )
                {
                    yield return this[this.Indexer.Parameters.Count];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}