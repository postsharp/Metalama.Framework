using Caravela.Framework.Code;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IOverriddenElement
    {
        ICodeElement OverriddenElement { get; }
    }
}
