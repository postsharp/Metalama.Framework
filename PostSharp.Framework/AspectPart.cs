namespace PostSharp.Framework.Sdk
{
    internal class AspectPart
    {
        public string? Name { get; }
        public int ExecutionOrder { get; }

        public AspectPart(string? name, int executionOrder)
        {
            Name = name;
            ExecutionOrder = executionOrder;
        }
    }
}
