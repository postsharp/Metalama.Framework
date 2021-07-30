// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationHelpers : ICompilationHelpers
    {
        public IteratorInfo GetIteratorInfo( IMethod method ) => method.GetIteratorInfoImpl();

        public AsyncInfo GetAsyncInfo( IMethod method ) => method.GetAsyncInfoImpl();
    }
}