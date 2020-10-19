namespace Caravela.Framework
{
    internal class AspectPart
    {
        public string? Name { get; }
        public int ExecutionOrder { get; }

        public AspectPart(string? name, int executionOrder)
        {
            this.Name = name;
            this.ExecutionOrder = executionOrder;
        }
    }
}
