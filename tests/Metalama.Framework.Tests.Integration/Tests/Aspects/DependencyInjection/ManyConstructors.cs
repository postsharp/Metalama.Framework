namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.ManyConstructors;

// <target>
[MyAspect]
public class BaseClass
{
    public BaseClass() { }

    public BaseClass( int x ) : this() { }

    public BaseClass( long x ) { }
}

// <target>
public class DerivedClass : BaseClass
{
    public DerivedClass() { }

    public DerivedClass( int x ) : this() { }

    public DerivedClass( long x ) : base( x ) { }

    public DerivedClass( float x ) : this( (int) x ) { }
}