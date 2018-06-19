using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Yali.Extensions;

namespace Yali.Examples._01._Proxy
{
    public class Example
    {
        public static async Task Run()
        {
            var engine = Engine.Create();

            engine.Set("foo", new SharedClass());

            await engine.ExecuteAsync(@"
	            foo.print('Hello C#!')
	            foo.wait(1000)
	            foo.notice('Engine is automatically injected')
	            foo:nonstatic()
            ");
        }
    }
}
