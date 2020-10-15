namespace Caravela.Reactive
{
    public readonly struct ReactiveCollectorToken
    {
        internal IReactiveTokenCollector Collector { get; }

        internal ReactiveCollectorToken(IReactiveTokenCollector collector)
        {
            Collector = collector;
        }
    }
}