// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class ParameterList : DeclarationCollection<IParameter>, IParameterList
    {
        public ParameterList( IMethodBase declaringMethod, IReadOnlyList<IRef<IParameter>> sourceItems )
            : base( declaringMethod, sourceItems ) { }

        public ParameterList( IIndexer declaringIndexer, IReadOnlyList<IRef<IParameter>> sourceItems )
            : base( declaringIndexer, sourceItems ) { }

        private ParameterList() { }

        public static ParameterList Empty { get; } = new();

        public IParameter this[ string name ]
        {
            get
            {
                var parameter = this.SingleOrDefault( p => p.Name == name );

                if ( parameter == null )
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(name),
                        $"The method '{this.ContainingDeclaration}' does not contain a parameter named '{name}'" );
                }

                return parameter;
            }
        }

        public object ToValueArray() => new ValueArrayExpression( this );

        public IParameter this[ int index ] => this.GetItem( this.Source[index] );
    }
}