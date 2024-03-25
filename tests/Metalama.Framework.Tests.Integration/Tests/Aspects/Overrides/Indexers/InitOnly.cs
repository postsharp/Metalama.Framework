using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.InitOnly;

[assembly: AspectOrder( typeof(OverridePropertyAttribute), typeof(OverrideIndexerAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.InitOnly
{
    public class OverrideIndexerAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var indexer in builder.Target.Indexers)
            {
                builder.Advice.OverrideAccessors( indexer, nameof(GetIndexer), null );
            }

            foreach (var indexer in builder.Target.Properties)
            {
                builder.Advice.OverrideAccessors( indexer, nameof(GetIndexer), null );
            }
        }

        [Template]
        public dynamic? GetIndexer()
        {
            Console.WriteLine( "Override" );

            return meta.Proceed();
        }
    }

    public class OverridePropertyAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.OverrideAccessors( property, nameof(GetProperty), nameof(SetProperty) );
            }
        }

        [Template]
        public dynamic? GetProperty()
        {
            var indexer = meta.Target.Type.Indexers.First();

            return indexer.GetValue( 42 );
        }

        [Template]
        public void SetProperty()
        {
            var indexer = meta.Target.Type.Indexers.First();
            indexer.SetValue( meta.Target.Parameters.Last(), 42 );
        }
    }

    // <target>
    [OverrideIndexer]
    [OverrideProperty]
    internal class TargetClass
    {
        public TargetClass()
        {
            this[42] = 42;
            Foo = 42;
        }

        public int this[ int x ]
        {
            get
            {
                Console.WriteLine( "Original" );

                return 42;
            }
            init
            {
                Console.WriteLine( "Original" );
            }
        }

        public int Foo
        {
            get
            {
                Console.WriteLine( "Original" );

                return 42;
            }
            init
            {
                Console.WriteLine( "Original" );
            }
        }
    }
}