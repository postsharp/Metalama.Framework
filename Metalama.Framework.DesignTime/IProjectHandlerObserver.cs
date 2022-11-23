// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Services;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

public interface IProjectHandlerObserver : IGlobalService
{
    void OnGeneratedCodePublished( ImmutableDictionary<string, string> sources );

    void OnTouchFileWritten( string content );
}