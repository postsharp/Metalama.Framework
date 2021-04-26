// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MemberList<IMethod, MemberLink<IMethod>>, IMethodList
    {
        public static MethodList Empty { get; } = new();

        private MethodList() { }

        public MethodList( IEnumerable<MemberLink<IMethod>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation ) { }
    }
}