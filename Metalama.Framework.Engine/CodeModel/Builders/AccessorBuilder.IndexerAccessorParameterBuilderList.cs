// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using System.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private sealed class IndexerAccessorParameterBuilderList : IParameterBuilderList, IParameterList
        {
            private readonly List<IndexerParameterBuilder> _parameters;

            public AccessorBuilder Accessor { get; }

            public IndexerAccessorParameterBuilderList( AccessorBuilder accessor )
            {
                this.Accessor = accessor;
                this._parameters = new List<IndexerParameterBuilder>();

                if ( this.Accessor.MethodKind == MethodKind.PropertySet )
                {
                    this._parameters.Add( new IndexerParameterBuilder( this.Accessor, null ) );
                }
            }

            public IndexerBuilder Indexer => (IndexerBuilder) this.Accessor.ContainingMember;

            public IParameterBuilder this[ string name ]
                => (this.Accessor.MethodKind, name) switch
                {
                    (MethodKind.PropertySet, "value") => this[this.Count - 1],
                    _ => this[this.Indexer.Parameters[name].Index]
                };

            public IParameterBuilder this[ int index ]
            {
                get
                {
                    switch ( this.Accessor.MethodKind )
                    {
                        case MethodKind.PropertySet:
                            while ( this.Indexer.Parameters.Count + 1 > this._parameters.Count )
                            {
                                this._parameters.Insert( this._parameters.Count - 1, new IndexerParameterBuilder( this.Accessor, this._parameters.Count - 1 ) );
                            }

                            return this._parameters[index];

                        default:
                            while ( this.Indexer.Parameters.Count > this._parameters.Count )
                            {
                                this._parameters.Add( new IndexerParameterBuilder( this.Accessor, this._parameters.Count ) );
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

            public IEnumerator<IParameterBuilder> GetEnumerator()
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

            IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

            int IReadOnlyCollection<IParameter>.Count => this.Count;

            IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];

            IParameter IParameterList.this[ string name ] => this[name];
        }
    }
}