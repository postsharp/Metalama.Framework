namespace Caravela.AspectWorkbench.Model
{
    static class NewTestDefaults
    {
        public const string TemplateSource = @"  
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.TestFramework.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;

class Aspect
{
  [Template]
  dynamic Template()
  {
        dynamic result = AdviceContext.Proceed();
        return result;
  }
}

class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";
    }
}
