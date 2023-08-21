using OfficeOpenXml;
using System.Diagnostics;
using Testing.Services;

namespace Testing;

class Program
{
    private static bool stopPrinting = false;

    static async Task Main(string[] args)
    {
        Thread printingThread = new(PrintTextInLoop);
        printingThread.Start();

        Stopwatch stopwatch = Stopwatch.StartNew();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        DatabaseWithExcelTablesService service = new();
        await service.DiariesInfoAsync();

        stopwatch.Stop();
        stopPrinting = true;
        printingThread.Join();
        Console.WriteLine($"Программа успешно завершена.\n" +
            $"Время выполнения: {stopwatch.Elapsed}\n");
    }

    static void PrintTextInLoop()
    {
        GlobalVariable globalVar = GlobalVariable.Instance;
        Console.Write("Программа выполняется... ");
        using (var progress = new ProgressBar())
        {
            while (!stopPrinting) {
                progress.Report(globalVar.Value / 63.0);
                Thread.Sleep(20);
            }
        }
        Console.SetCursorPosition(0, Console.CursorTop);
    }
}