using System.Threading.Tasks;
using Meadow;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Desktop;

public class MeadowApp : App<global::Meadow.Desktop>
{
    private MeadowLeaRunner _meadowLeaRunner = null!;
    
    public override Task Initialize()
    {
        _meadowLeaRunner = new MeadowLeaRunner(CoreOperations.All());

        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        while (true)
        {
            await Task.Delay(1000);
        }
    }
}