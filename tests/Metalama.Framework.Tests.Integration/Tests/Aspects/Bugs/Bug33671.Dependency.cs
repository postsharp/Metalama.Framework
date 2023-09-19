using System;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33671;

[RunTimeOrCompileTime]
public interface IFace
{
    string? ProfileName { get; init; }
}