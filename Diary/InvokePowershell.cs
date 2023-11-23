namespace Diary
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Class for invoking powershell scripts.
    /// </summary>
    public class InvokePowershell
    {
        private StringBuilder output;
        private StringBuilder error;

        /// <summary>
        /// Invokes the given script and returns the output and error as strings.
        /// </summary>
        /// <param name="script">The script to be executed.</param>
        /// <returns>A tuple containing the output and error.</returns>
        public (string Output, string Error) Start(string script)
        {
            this.output = new StringBuilder();
            this.error = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = script,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                this.output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                this.error.AppendLine(e.Data);
            };

            process.Start();
            if (process.StartInfo.RedirectStandardError)
            {
                process.BeginErrorReadLine();
            }

            if (process.StartInfo.RedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            return (this.output.ToString().Trim(), this.error.ToString().Trim());
        }
    }
}
