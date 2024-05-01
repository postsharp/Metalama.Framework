// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Utilities.Caching;

internal interface IRecyclable
{
    void CleanUp();

    void Recycle();
}