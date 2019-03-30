namespace PixelVision8.Runner.Services
{
    public interface IExport
    {
        ExportService exportService { get; }
        void StartExport(bool useSteps = true);
    }
}