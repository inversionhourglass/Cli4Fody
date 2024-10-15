using Rougamo;
using Rougamo.Context;
using System.Reflection;

namespace Lib2
{
    public class TestAttribute : MoAttribute
    {
        public override Feature Features => Feature.OnEntry;

        public override void OnEntry(MethodContext context)
        {
            MethodBase method = context.Method;
            string fullName = $"{method.DeclaringType?.FullName}.{method.Name}";
            Console.WriteLine($"{fullName} OnEntry");
        }
    }
}
