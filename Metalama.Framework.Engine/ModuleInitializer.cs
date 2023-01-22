// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis.Simplification;

namespace Metalama.Framework.Engine;

internal static class ModuleInitializer
{
    static ModuleInitializer()
    {
        TypeOfResolver.Resolver = UserCodeExecutionContext.ResolveCompileTimeTypeOf;
        FormattingAnnotations.Initialize( Simplifier.Annotation );
        MetalamaStringFormatter.Initialize( new MetalamaStringFormatterImpl() );
    }

    public static void EnsureInitialized() { }
}