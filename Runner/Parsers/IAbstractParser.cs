namespace PixelVision8.Runner.Parsers
{
    public interface IAbstractParser
    {
        int totalSteps { get; }
        bool completed { get; }
        void CalculateSteps();
        void NextStep();
    }
}