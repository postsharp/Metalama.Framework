// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public partial class DesignTimeSourceGenerator
{
    protected abstract class SourceGeneratorImpl
    {
        public IProjectOptions ProjectOptions { get; }

        protected SourceGeneratorImpl( IProjectOptions projectOptions )
        {
            this.ProjectOptions = projectOptions;
        }

        public abstract void GenerateSources( Compilation compilation, GeneratorExecutionContext context );
    }
}