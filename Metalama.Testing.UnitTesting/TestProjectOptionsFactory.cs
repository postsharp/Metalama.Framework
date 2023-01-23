// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;

namespace Metalama.Testing.UnitTesting
{
    internal sealed class TestProjectOptionsFactory : IProjectOptionsFactory
    {
        private readonly IProjectOptions _projectOptions;

        public TestProjectOptionsFactory( IProjectOptions projectOptions )
        {
            this._projectOptions = projectOptions;
        }

        public IProjectOptions GetProjectOptions( Project project ) => this._projectOptions;
    }
}