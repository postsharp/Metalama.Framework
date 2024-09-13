// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.GeneratedCodeAnalysis;

/// <summary>
/// Used to communicate information about the original (pre-transformer) compilation to <see cref="GeneratedCodeAnalyzerBase"/>.
/// This is necessary, because the post-transformer compilation does not contain compile-time code.
/// </summary>
internal static class OriginalCompilationInfo
{
    // Note: this depends on implementation details of the compiler, namely that AsyncLocal is preserved between transformer and transformed code analyzer
    private static readonly AsyncLocal<Compilation?> _originalCompilation = new();
    private static readonly AsyncLocal<AspectPipelineConfiguration?> _originalConfiguration = new();

    public static Compilation? OriginalCompilation
    {
        get => _originalCompilation.Value;
        set => _originalCompilation.Value = value;
    }
    public static AspectPipelineConfiguration? OriginalConfiguration
    {
        get => _originalConfiguration.Value;
        set => _originalConfiguration.Value = value;
    }
}