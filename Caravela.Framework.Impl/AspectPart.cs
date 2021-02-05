using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    internal class AspectPart
    {
        public AspectType AspectType { get; }

        public string? Name { get; }

        public AspectPart( AspectType aspectType, string? name )
        {
            this.AspectType = aspectType;
            this.Name = name;
        }

        internal AspectCompilation Transform( AspectCompilation compilation )
        {
            var aspectDriver = (AspectDriver) this.AspectType.AspectDriver;

            var aspectInstances = compilation.AspectsByAspectType[this.AspectType.Name];

            var instanceResults = aspectInstances.Select( ai => aspectDriver.EvaluateAspect( ai ) );

            return compilation.Update( instanceResults );
        }
    }
}
