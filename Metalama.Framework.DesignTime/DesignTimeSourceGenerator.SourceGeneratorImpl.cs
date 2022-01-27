// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public partial class DesignTimeSourceGenerator
{
    protected abstract class SourceGeneratorImpl
    {
        public abstract void GenerateSources( IProjectOptions projectOptions, Compilation compilation, GeneratorExecutionContext context );
    }
}