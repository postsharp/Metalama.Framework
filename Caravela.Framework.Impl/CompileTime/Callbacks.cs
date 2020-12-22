using System;
using System.IO;

namespace Caravela.Framework.Impl.CompileTime
{
    public static class Callbacks
    {
        public static Func<MemoryStream, MemoryStream>? AssemblyRewriter { get; set; }
    }
}
