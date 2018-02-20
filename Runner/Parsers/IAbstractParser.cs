namespace PixelVisionRunner.Parsers
{
    public interface IAbstractParser
    {
        int currentStep { get; }
        int totalSteps { get; }
        bool completed { get; }
        void CalculateSteps();
        void NextStep();
    }
}