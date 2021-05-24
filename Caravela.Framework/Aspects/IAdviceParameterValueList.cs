namespace Caravela.Framework.Aspects
{
    [CompileTimeOnly]
    public interface IAdviceParameterValueList
    {
        [return: RunTimeOnly]
        dynamic ToArray();

        [return: RunTimeOnly]
        dynamic ToValueTuple();
    }
}