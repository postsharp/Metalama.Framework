using System;

namespace PostSharp.Patterns.AutoCancellationToken
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]

    public class AutoCancellationTokenAttribute : Attribute { }
}
