    public class Target
    {


        private int _myField1;


        private int _myField
        {
            get
            {
                Console.WriteLine("Aspect code");
                return this._myField1;
            }

            set
            {
                Console.WriteLine("Aspect code");
                this._myField1 = value;
            }
        }
    }