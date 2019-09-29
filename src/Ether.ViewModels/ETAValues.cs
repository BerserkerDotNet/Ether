namespace Ether.ViewModels
{
    public class ETAValues
    {
        public ETAValues(float originalEstimate, float remainingWork, float completedWork)
        {
            OriginalEstimate = originalEstimate;
            RemainingWork = remainingWork;
            CompletedWork = completedWork;
        }

        public float OriginalEstimate { get; private set; }

        public float RemainingWork { get; private set; }

        public float CompletedWork { get; private set; }

        public bool IsEmpty => OriginalEstimate == 0 && RemainingWork == 0 && CompletedWork == 0;
    }
}
