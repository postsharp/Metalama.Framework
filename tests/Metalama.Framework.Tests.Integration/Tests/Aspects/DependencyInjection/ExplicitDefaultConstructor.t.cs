[MyAspect]
public class BaseClass
{
    public BaseClass(global::System.ICustomFormatter formatter) { this.formatter = formatter; }

    private global::System.ICustomFormatter formatter;
}

public class DerivedClass : BaseClass
{
    public DerivedClass(global::System.ICustomFormatter formatter) : base(formatter) { }
}