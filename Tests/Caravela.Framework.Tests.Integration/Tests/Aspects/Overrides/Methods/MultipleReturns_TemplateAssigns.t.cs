internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void_TwoReturns(int x)
{
    global::System.Console.WriteLine("Begin override.");
void result ;            while (x > 0)
            {
                if (x == 42)
                {
goto __aspect_return_1;                }
    
                x--;
            }
    
            if (x > 0)
goto __aspect_return_1;__aspect_return_1:    global::System.Console.WriteLine("End override.");
    _ = (object)result;
    return;
}
    
        [Override]
        public int TargetMethod_Int_TwoReturns(int x)
{
    global::System.Console.WriteLine("Begin override.");
global::System.Int32 result ;            while (x > 0)
            {
                if (x == 42)
                {
result=42;goto __aspect_return_1;                }
    
                x--;
            }
    
            if (x > 0)
{result=-1;goto __aspect_return_1;}result=0;goto __aspect_return_1;__aspect_return_1:    global::System.Console.WriteLine("End override.");
    return (int)result;
}
    }