using System;
using System.Collections.Generic;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions.TemplateInRunTimeCode;

class TargetCode
{
    // <target>
    void Method()
    {
        Generate();

        Introduce();

        [Template]
        void Generate()
        {
        }

        [Introduce]
        void Introduce()
        {
        }
    }
}