namespace Caravela.Framework.Code
{
    public interface IEvent : IMember
    {
        INamedType DelegateType { get; }
        IMethod Adder { get; }
        IMethod Remover { get; }
        // TODO: how does this work? is it a "fake" method that invokes the underlying delegate for field-like events? yes
        IMethod? Raiser { get; }
    }
}