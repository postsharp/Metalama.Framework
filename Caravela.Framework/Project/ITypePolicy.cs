namespace Caravela.Framework.Project
{
    /// <summary>
    /// A type policy is a nested type of arbitrary name that implements <see cref="ITypePolicy"/> and
    /// that can add aspects and advices to the declaring type. Type policies are executed before any other aspect.
    /// They cannot have layers.
    /// </summary>
    public interface ITypePolicy
    {
        void BuildPolicy( ITypePolicyBuilder typePolicyBuilder );
    }
    
    // TODO: the problem with this design, based on nested types, is that the nested type is compile-time-only and has access
    // to the base type which is run-time only. This makes the template compiler more complex (the type must be un-nested).
}