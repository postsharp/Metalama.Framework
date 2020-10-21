namespace Caravela.Reactive
{
    public interface IReactiveCollector
    {
        void AddDependency( IReactiveObservable<IReactiveObserver> source, int version );

        void AddSideValue( IReactiveSideValue value );

        void AddSideValues( ReactiveSideValues values );
    }
}