using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    public class ProcessGateway
    {
        private readonly string _program;
        private readonly string _args;
        public string Messages { get; private set; }

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
            
            Messages = GetMessages();
        }

        private string GetMessages()
        {
            var ret = string.Format("StdOut: {0} StdErr: {1}", _process.StandardOutput.ReadToEnd(), _process.StandardError.ReadToEnd());
            return ret;
        }

        public void WasDeploySuccess()
        {
            if (Messages.IndexOf("Successfully published database", StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            Assert.Fail("SqlPackage.exe did not complete successfully, messages: {0}", Messages);
        }
    }
}