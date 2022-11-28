using System;

namespace Metalama.Framework.Engine.Options;

public class DefaultGlobalOptions : IGlobalOptions
{
    public virtual TimeSpan QuietPeriodTimerDelay => TimeSpan.FromSeconds( 3 );
}