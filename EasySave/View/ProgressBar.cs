namespace EasySave.View
{
    public class ProgressBar
    {
        public string UpdateProgress(int jobId, string jobName, long totalFilesToCopy, long filesCopied)
        {
            const int progressBarWidth = 50;

            double progressPercentage = (double)filesCopied / totalFilesToCopy;
            int progressBarLength = (int)(progressPercentage * progressBarWidth);

            string progressBar = "[" + new string('#', progressBarLength) + new string('-', progressBarWidth - progressBarLength) + "]";

            string progressText = $"{jobId}: {jobName}\nProgress: {progressBar} {progressPercentage:P} ({filesCopied}/{totalFilesToCopy})";

            return progressText;
        }
    }
}