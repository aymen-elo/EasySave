namespace EasySaveGUI.Helper
{
    /* CLI Progress bar for debugging and better visualization */
    public class ProgressBar
    {
        public string UpdateProgress(int jobId, string jobName, long totalFilesToCopy, long filesCopied)
        {
            const int progressBarWidth = 0;

            double progressPercentage = (double)filesCopied / totalFilesToCopy;
            int progressBarLength = (int)(progressPercentage * progressBarWidth);

            string progressBar = "[" + new string('#', progressBarLength) + new string('-', progressBarWidth - progressBarLength) + "]";

            string progressText = $"{jobId}: {jobName}\nProgress: {progressBar} {progressPercentage:P} ({filesCopied}/{totalFilesToCopy})";

            return progressText;
        }
    }
}