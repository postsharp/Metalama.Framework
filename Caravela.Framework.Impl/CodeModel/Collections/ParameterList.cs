// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ParameterList : CodeElementList<IParameter, CodeElementLink<IParameter>>, IParameterList
    {
        public ParameterList( IMethodBase containingElement, IEnumerable<CodeElementLink<IParameter>> sourceItems ) : base( containingElement, sourceItems )
        {
        }
        public ParameterList( IProperty containingElement, IEnumerable<CodeElementLink<IParameter>> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        private ParameterList()
        {
        }

        public static ParameterList Empty { get; } = new ParameterList();
    }
}