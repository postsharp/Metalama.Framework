namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the parameters of the integration test input.
    /// </summary>
    public class TestInput
    {
        public TestInput( string testName, string templateSource, string? targetSource )
        {
            this.TestName = testName;
            this.TemplateSource = templateSource;
            this.TargetSource = targetSource;
        }

        /// <summary>
        /// Gets the name of the test. Usually equals the relative path of the test source.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the content of the test source file.
        /// </summary>
        public string TemplateSource { get; }

        /// <summary>
        /// Gets the content of the second test source file. Currently unused.
        /// </summary>
        public string? TargetSource { get; }
    }
}
