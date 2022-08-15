#if TEST_OPTIONS
// @KeepDisabledCode
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug30928
{
    public interface ISomeInterface
    {
    }

    // <target>
    public class SomeDisposable : ISomeInterface
    {   
#if TESTRUNNER
        void Foo()
        {
            Debug.Fail("");
        }
#endif

#if !TESTRUNNER
        void Bar()
        {
            Debug.Fail("");
        }
#endif
    }
}
