namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows invocation of the method.
    /// </summary>
    public interface IMethodInvocation
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic Invoke( dynamic? instance, params dynamic[] args );
    }
}