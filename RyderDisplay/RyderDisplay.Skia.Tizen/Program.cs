using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace RyderDisplay.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new RyderDisplay.App(), args);
            host.Run();
        }
    }
}
