namespace Caravela.TestFramework.Templating
{
    public class TestInput
    {
        public TestInput( string? templateSource, string? targetSource )
        {
            this.TemplateSource = templateSource;
            this.TargetSource = targetSource;
        }

        public string? TemplateSource { get; set; }

        public string? TargetSource { get; set; }
    }
}
