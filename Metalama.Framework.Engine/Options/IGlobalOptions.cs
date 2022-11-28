using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Options;

public interface IGlobalOptions : IGlobalService
{
    TimeSpan QuietPeriodTimerDelay { get; }
}