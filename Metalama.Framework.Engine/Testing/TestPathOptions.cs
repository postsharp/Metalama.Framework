// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using System;

namespace Metalama.Framework.Engine.Testing;

public class TestPathOptions : DefaultPathOptions
{
    private readonly Lazy<string> _settingsDirectory;
    private readonly Lazy<string> _compileTimeProjectCacheDirectory;

    public TestPathOptions( Lazy<string> settingsDirectory, Lazy<string> compileTimeProjectCacheDirectory )
    {
        this._settingsDirectory = settingsDirectory;
        this._compileTimeProjectCacheDirectory = compileTimeProjectCacheDirectory;
    }

    // Don't create crash reports for user exceptions so we have deterministic error messages.
    public override string? GetNewCrashReportPath() => null;

    public override string CompileTimeProjectCacheDirectory => this._compileTimeProjectCacheDirectory.Value;

    public override string SettingsDirectory => this._settingsDirectory.Value;
}