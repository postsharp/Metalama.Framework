// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.AspectConfiguration;

internal class Configurator
{
    public Configurator( IDeclaration declaration, Framework.Options.AspectOptions options )
    {
        this.Declaration = declaration;
        this.Options = options;
    }

    public IDeclaration Declaration { get; }

    public Framework.Options.AspectOptions Options { get; }
}