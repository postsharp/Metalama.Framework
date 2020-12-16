using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string TemplateClassMembers_Template = @"  
using System;
using System.Collections.Generic;

class Aspect : BaseAspect
{
    public Aspect() : this(""Result = {0}"")
    {
    }

    public Aspect(string formatString) : base(""Result = {0}"")
    {
    }
    
    [Template]
    dynamic Template()
    {
        dynamic result = AdviceContext.Proceed();
        
        Console.WriteLine(this.Format(result));
        
        return result;
    }
    
    public override string Format(object o)
    {
        return string.Format(FormatString, o);
    }
}

abstract class BaseAspect
{
    protected BaseAspect(string formatString)
    {
        this.FormatString = formatString;
    }
   
    public string FormatString { get; set; }
    
    public abstract string Format(object o);
}";

        private const string TemplateClassMembers_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string TemplateClassMembers_ExpectedOutput = @"";

        [Fact]
        public async Task TemplateClassMembers()
        {
            var testResult = await this._testRunner.Run( new TestInput( TemplateClassMembers_Template, TemplateClassMembers_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
