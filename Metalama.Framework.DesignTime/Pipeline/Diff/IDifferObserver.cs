// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

public interface IDifferObserver : IService
{
    void OnNewCompilation();

    void OnUpdateCompilationVersion();

    void OnMergeCompilationChanges();
}