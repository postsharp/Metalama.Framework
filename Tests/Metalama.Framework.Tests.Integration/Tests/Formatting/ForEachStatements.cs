﻿#pragma warning disable CS0649, CS8618

using System.Collections.Generic;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.ForEachStatements.ForEachStatements
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
        dynamic? Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            foreach (var x in compileTimeObject.compileTimeEnumerable)
            {
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
