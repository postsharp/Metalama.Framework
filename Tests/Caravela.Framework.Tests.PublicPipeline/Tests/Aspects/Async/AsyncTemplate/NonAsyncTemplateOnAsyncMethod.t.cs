    class TargetCode
{

[Aspect]
public ValueTask<int>
	AsyncMethod(int a)
	{
	global::System.Console.WriteLine("Getting task");
	var task = this.__AsyncMethod__OriginalImpl(a);
	global::System.Console.WriteLine("Got task");
	return (System.Threading.Tasks.ValueTask<int>
		)task;
		}

		private async ValueTask<int> __AsyncMethod__OriginalImpl(int a)
            {
                await Task.Yield();
                return a;
            }
        }