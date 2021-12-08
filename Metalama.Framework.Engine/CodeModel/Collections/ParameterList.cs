// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class ParameterList : DeclarationList<IParameter, Ref<IParameter>>, IParameterList
    {
        public ParameterList( IMethodBase containingDeclaration, IEnumerable<Ref<IParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

        public ParameterList( IProperty containingDeclaration, IEnumerable<Ref<IParameter>> sourceItems ) : base(
            containingDeclaration,
            sourceItems ) { }

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
    }
}