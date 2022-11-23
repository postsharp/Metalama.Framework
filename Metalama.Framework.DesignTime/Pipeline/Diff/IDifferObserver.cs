// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// An observer interface used for testing this namespace.
/// </summary>
public interface IDifferObserver : IGlobalService // Must be global
{
    void OnNewCompilation();

    void OnComputeIncrementalChanges();

    void OnComputeNonIncrementalChanges();

    void OnMergeCompilationChanges();
}