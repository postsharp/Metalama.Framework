﻿#pragma warning disable CS0649, CS8618

using Caravela.Framework.Project;
using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.ForEachStatements.ForEachStatements
{
    class RunTimeClass
    {
        public IEnumerable<int> runTimeEnumerable;
    }

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public IEnumerable<int> compileTimeEnumerable;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            foreach (var x in compileTimeObject.compileTimeEnumerable)
            {
                // TODO: x should not be highlighted as template keyword here. #28396
                x.ToString();
            }

            foreach (var x in runTimeObject.runTimeEnumerable)
            {
                x.ToString();
            }

            return meta.Proceed();
        }
    }
}
