using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.CSharpSyntax.TemplateClassMembers
{
    class Aspect : BaseAspect
    {
        public Aspect() : this("Result = {0}")
        {
        }

        public Aspect(string formatString) : base("Result = {0}")
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

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
    }
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}