// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestMetalamaProjectClassifier : IMetalamaProjectClassifier
{
    public bool IsMetalamaEnabled( Compilation compilation )
        => compilation.ExternalReferences.OfType<PortableExecutableReference>()
            .Any( x => Path.GetFileNameWithoutExtension( x.FilePath )!.Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) );
}