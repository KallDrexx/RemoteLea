using System;
using System.Threading.Tasks;
using Meadow;

namespace RemoteLea.Meadow.Desktop;

public class Program
{
    public static async Task Main(string[] args)
    {
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        ApplicationConfiguration.Initialize();
        await MeadowOS.Start(args);
    }
}