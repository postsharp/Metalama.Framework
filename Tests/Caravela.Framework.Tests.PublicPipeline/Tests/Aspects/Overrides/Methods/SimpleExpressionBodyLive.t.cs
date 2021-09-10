internal class TargetClass
    {
        public void TargetMethod()
{
            Console.WriteLine("This is the overriding method.");
            Console.WriteLine("This is the original method.");
            goto __aspect_return_1;
        __aspect_return_1:
            return;
        };
    }