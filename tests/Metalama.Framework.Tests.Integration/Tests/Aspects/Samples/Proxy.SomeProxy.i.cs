using System;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Samples;

namespace Metalama.Samples.Proxy.Tests
{
    public class SomeProxy : ISomeInterface
    {
        private ISomeInterface _intercepted;
        private IInterceptor _interceptor;

        public SomeProxy( IInterceptor interceptor, ISomeInterface intercepted )
        {
            _interceptor = interceptor;
            _intercepted = intercepted;
        }

        public int NonVoidMethod( int a, string b )
        {
            var args = ( a, b );

            return _interceptor.Invoke( ref args, Invoke );

            int Invoke( ref (int, string) receivedArgs )
            {
                return _intercepted.NonVoidMethod( receivedArgs.Item1, receivedArgs.Item2 );
            }
        }

        public void VoidMethod( int a, string b )
        {
            var args = ( a, b );
            _interceptor.Invoke( ref args, Invoke );

            ValueTuple Invoke( ref (int, string) receivedArgs )
            {
                _intercepted.VoidMethod( receivedArgs.Item1, receivedArgs.Item2 );

                return default;
            }
        }

        public void VoidNoParamMethod()
        {
            var args = default(ValueTuple);
            _interceptor.Invoke( ref args, Invoke );

            ValueTuple Invoke( ref ValueTuple receivedArgs )
            {
                _intercepted.VoidNoParamMethod();

                return default;
            }
        }
    }
}