﻿using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
  #30594
  Enums marked as [RunTimeOrCompileTime] are not seen from compile-time assembly when defined in a referenced project with no aspect
*/

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30594
{
    internal class MyAspect : OverrideMethodAspect
    {
        public MyEnum Property { get; set; }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(this.Property.ToString());
            return meta.Proceed();
        }
    }

    // <target>
    internal class C
    {
        [MyAspect( Property = MyEnum.MyValue )]
        public void M() { }
    }



}
