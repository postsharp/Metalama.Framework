namespace Caravela.TestFramework
{
    public class TestInput
    {
        public TestInput( string testName, string templateSource, string? targetSource )
        {
            this.TestName = testName;
            this.TemplateSource = templateSource;
            this.TargetSource = targetSource;
        }

        public string TestName { get; }

        public string TemplateSource { get; }

        public string? TargetSource { get; set; }
    }
}
