// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    public class AnnotationTriviaAspectTests : AspectTestSuite
    {
        public AnnotationTriviaAspectTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [FromDirectory( @"Formatting" )]
        public Task All( string testName ) => this.GetTestResultAsync( testName );

        protected override TestRunnerBase CreateTestRunner( TestRunnerKind kind )
            => new AnnotationUnitTestRunner( this.ServiceProvider, this.ProjectDirectory );
    }
}