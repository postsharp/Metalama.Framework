// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code
{
    internal interface ICompilationHelpers
    {
        IteratorInfo GetIteratorInfo( IMethod method );

        AsyncInfo GetAsyncInfo( IMethod method );

        AsyncInfo GetAsyncInfo( IType type );
    }
}