namespace GameCreator.Services
{
    public interface IExport
    {
        ExportService exportService { get; }
        void StartExport(bool useSteps = true);
    }
}