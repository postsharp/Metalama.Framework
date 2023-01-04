// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;

internal sealed class TestMetalamaProjectClassifier : IMetalamaProjectClassifier
{
    public bool TryGetMetalamaVersion( Compilation compilation, out Version? version )
    {
        var reference = compilation.ExternalReferences.OfType<PortableExecutableReference>()
            .SingleOrDefault( x => Path.GetFileNameWithoutExtension( x.FilePath )!.Equals( "Metalama.Framework", StringComparison.OrdinalIgnoreCase ) );

        if ( reference != null )
        {
            // We assume, in all tests, that the code is compiled against the current version of Metalama.
            version = EngineAssemblyMetadataReader.Instance.AssemblyVersion;

            return true;
        }
        else
        {
            version = null;

            return false;
        }
    }
}