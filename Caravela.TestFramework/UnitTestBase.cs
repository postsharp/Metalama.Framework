using System.IO;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// The base class for integration tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use <see cref="FromDirectoryAttribute"/> to execute one test method on many test inputs.
    /// This is useful to write only one test method per category of tests.
    /// </para>
    /// </remarks>
    public abstract class UnitTestBase
    {
        protected readonly ITestOutputHelper Logger;

        /// <summary>
        /// Gets the root directory path of the current test project.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property is read from <c>AssemblyMetadataAttribute</c> with <c>Key = "ProjectDirectory"</c>.
        /// </para>
        /// </remarks>
        protected string ProjectDirectory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        public UnitTestBase( ITestOutputHelper logger )
        {
            this.Logger = logger;
            this.ProjectDirectory = TestEnvironment.GetProjectDirectory( this.GetType().Assembly );
        }
    }
}
