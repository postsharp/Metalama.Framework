using System;
using System.ComponentModel;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A fake type that replaces <c>void</c> in template-generated code.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "Don't use __Void in user code." )]
#pragma warning disable IDE1006 // Naming Styles
    public readonly struct __Void
#pragma warning restore IDE1006 // Naming Styles
    {
        public override string ToString() => "void";
    }
}
