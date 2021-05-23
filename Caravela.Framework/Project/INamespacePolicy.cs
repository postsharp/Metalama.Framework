namespace Caravela.Framework.Project
{
    /// <summary>
    /// Namespace policies are types that can provide aspects and constraints to types the same namespace as the namespace policy type itself.
    /// They can be arbitrarily named as long as they implement this interface, but their namespace matters. 
    /// </summary>
    public interface INamespacePolicy
    {
        void BuildPolicy( INamespacePolicyBuilder builder );
    }
}