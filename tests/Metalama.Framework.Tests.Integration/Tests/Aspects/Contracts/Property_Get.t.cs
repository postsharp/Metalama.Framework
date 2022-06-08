internal class Target
    {
        private string q;

        [NotNull]
        public string P 
{ get
{
        global::System.String returnValue ;returnValue = "p";
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;

}}

        [NotNull]
        public string Q
        {
            get
            {
        global::System.String returnValue ;                returnValue = q;
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;
            }
        }
    }