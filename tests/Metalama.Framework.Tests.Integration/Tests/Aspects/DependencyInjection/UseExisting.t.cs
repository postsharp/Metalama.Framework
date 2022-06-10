[MyAspect]
public class BaseClass
{
    public BaseClass(ICustomFormatter formatter) {this.formatter = formatter; }

    private global::System.ICustomFormatter formatter;}

public class DerivedClass : BaseClass
{
    public DerivedClass(ICustomFormatter formatter) : base(formatter) { }
}