// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Testing.Api;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests;

internal static class TestHelper
{
    public static CompilationModel CreateCompilationModel(
        this TestContext testContext,
        string code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => (CompilationModel) testContext.CreateCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addMetalamaReferences );

    public static CompilationModel CreateCompilationModel(
        this TestContext testContext,
        IReadOnlyDictionary<string, string> code,
        string? dependentCode = null,
        bool ignoreErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null,
        string? name = null,
        bool addMetalamaReferences = true )
        => (CompilationModel) testContext.CreateCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addMetalamaReferences );

    public static CompilationModel CreateCompilationModel( this TestContext testContext, Compilation compilation )
        => (CompilationModel) testContext.CreateCompilation( compilation );
}