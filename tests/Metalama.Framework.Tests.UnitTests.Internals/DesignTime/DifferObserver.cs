// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class DifferObserver : IDifferObserver
{
    public int NewCompilationEventCount { get; private set; }

    public int UpdateCompilationVersionEventCount { get; private set; }

    public int MergeCompilationChangesEventCount { get; private set; }

    public void OnNewCompilation() => this.NewCompilationEventCount++;

    public void OnUpdateCompilationVersion() => this.UpdateCompilationVersionEventCount++;

    public void OnMergeCompilationChanges() => this.MergeCompilationChangesEventCount++;
}