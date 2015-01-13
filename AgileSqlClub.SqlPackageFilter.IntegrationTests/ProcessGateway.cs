using System.Diagnostics;
using System.Threading;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    public class ProcessGateway
    {
        private readonly string _program;
        private readonly string _args;

        public ProcessGateway(string program, string args)
        {
            _program = program;
            _args = args;
        }

        private Process _process;

        public void Run()
        {
            var processInfo = new ProcessStartInfo(_program, _args);
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            _process = Process.Start(processInfo);
            if(_process != null)
                _process.WaitForExit(Timeout.Infinite);
        }

        public string GetMessages()
        {
            return string.Format("StdOut: {0} StdErr: {1}", _process.StandardOutput, _process.StandardError);
        }
    }
}