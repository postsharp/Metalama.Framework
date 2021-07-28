using System;

namespace Caravela.Framework.Impl.Utilities
{
    [Flags]
    internal enum ModifierCategories
    {
        Accessibility = 1,
        Inheritance = 2,
        Async = 4,
        Static = 8,
        ReadOnly = 16,
        Unsafe = 32,
        Volatile = 64,
        All = Accessibility | Inheritance | Async | Static | ReadOnly | Unsafe | Volatile
        
        
    }
}