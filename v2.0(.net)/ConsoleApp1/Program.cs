using WindowsInput.Events;

namespace ConsoleApp1;

class Program
{
    static async Task Main(string[] args)
    {
        var sim=new EventBuilder();
        sim
            .MoveTo(100,299);
        await sim.Invoke();
    }
}
