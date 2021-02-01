using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A fake type that replaces <c>void</c> in template-generated code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Don't use __Void in user code.")]
    public readonly struct __Void
    {
        public override string ToString() => "void";
    }
}
