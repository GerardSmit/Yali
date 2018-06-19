using System.Threading;
using System.Threading.Tasks;
using Yali.Attributes;

namespace Yali.Native.Proxy
{
    public interface ICallableProxy
    {
        [LuaMethod(Visible = false)]
        Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default);
    }
}
