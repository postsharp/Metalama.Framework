// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MemberList<IMethod, MemberLink<IMethod>>, IMethodList
    {

        public static MethodList Empty { get; } = new MethodList();

        private MethodList()
        {
        }

        public MethodList( IEnumerable<MemberLink<IMethod>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}