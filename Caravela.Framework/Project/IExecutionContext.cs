using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Project
{
    public interface IExecutionContext
    {
        IServiceProvider ServiceProvider { get; }
        IFormatProvider FormatProvider { get; }
        ICompilation? Compilation { get; }
    }
}