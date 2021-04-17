// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    [CompileTime]
    public interface IAspectWeaver : IAspectDriver
    {
        CSharpCompilation Transform( AspectWeaverContext context );
    }
}