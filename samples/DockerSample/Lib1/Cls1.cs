using Lib2;

namespace Lib1
{
    public class Cls1
    {
        [Test]
        public async Task CallAsync(Cls2 cls2)
        {
            await Task.Yield();
            cls2.Call();
        }
    }
}
