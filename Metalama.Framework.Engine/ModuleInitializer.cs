// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis.Simplification;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Metalama.Framework.Engine;

internal class ModuleInitializer
{
    static ModuleInitializer()
    {
        TypeOfResolver.Resolver = UserCodeExecutionContext.ResolveCompileTimeTypeOf;
        FormattingAnnotations.SimplifyAnnotation = Simplifier.Annotation;
    }
#if NET5_0_OR_GREATER
    [ModuleInitializer]
#endif
    public static void EnsureInitialized() { }
}