using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32223;

public class Base<T> { }

public class C : Base<dynamic>
{
    private dynamic _f1;
    private dynamic[] _f2;
    private List<dynamic> _f3;

    private dynamic M( dynamic x )
    {
        dynamic l1;
        dynamic[] l2;
        List<dynamic> l3;

        return x;
    }
}