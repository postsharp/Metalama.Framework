using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.PropertyInput
{
#pragma warning disable CS0067
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class Target
    {
        private string q;


private string _p;
        
        [NotNull]
        public string P 
{ get
{ 
        return this._p;
}
set
{ 
        if (value == null)
        {
            throw new global::System.ArgumentNullException();
        }

        this._p=value;
}
}

        [NotNull]
        public string Q
        {
            get 
{ 
        return q;
}
            set 
{ 
        if (value == null)
        {
            throw new global::System.ArgumentNullException();
        }

        q = value + "-";
}
        }
        
        
    }
}
