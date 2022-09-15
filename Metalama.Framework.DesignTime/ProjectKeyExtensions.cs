// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public static class ProjectKeyExtensions
{
    public static ProjectKey GetProjectKey( this Compilation compilation ) => ProjectKey.FromCompilation( compilation );
}