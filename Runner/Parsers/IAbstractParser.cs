namespace PixelVisionRunner.Parsers
{
    public interface IAbstractParser
    {
        int totalSteps { get; }
        bool completed { get; }
        void CalculateSteps();
        void NextStep();
    }
}