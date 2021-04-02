using System;
using System.Threading.Tasks;

namespace CVRP
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Application(args).RunAsync();
        }
    }
}
