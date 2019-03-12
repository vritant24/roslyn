using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FormatRunner
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            const string Version = "3.0.4-prerelease.19161.3";

            await RunProcess("dotnet", "tool uninstall dotnet-format --tool-path .tools").ConfigureAwait(false);
            await RunProcess("dotnet", $"tool install dotnet-format --version {Version} --tool-path .tools --add-source https://dotnet.myget.org/F/format/api/v3/index.json").ConfigureAwait(false);

            foreach (var file in Directory.EnumerateFiles(@".\artifacts\bin\FormatRunner\Debug\netcoreapp2.1", "Microsoft.CodeAnalysis*.dll"))
            {
                var fileName = Path.GetFileName(file);
                var toolPath = $@".\.tools\.store\dotnet-format\{Version}\dotnet-format\{Version}\tools\netcoreapp2.1\any";
                var newFilePath = Path.Combine(toolPath, fileName);

                File.Copy(file, newFilePath, true);
            }

            return await RunProcess(@".\.tools\dotnet-format", $"{string.Join(' ', args)}").ConfigureAwait(false);
        }

        static Task<int> RunProcess(string executable, string arguments)
        {
            Console.WriteLine($"> {executable} {arguments}");
            Console.WriteLine();

            var processStartInfo = new ProcessStartInfo(executable, arguments);
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            var process = new Process();
            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    Console.Error.WriteLine(e.Data);
                }
            };

            process.Exited += (s, e) =>
            {
                // We must call WaitForExit to make sure we've received all OutputDataReceived/ErrorDataReceived calls
                // or else we'll be returning a list we're still modifying. For paranoia, we'll start a task here rather
                // than enter right back into the Process type and start a wait which isn't guaranteed to be safe.
                Task.Run(() =>
                {
                    process.WaitForExit();
                    Console.WriteLine();
                    tcs.TrySetResult(process.ExitCode);
                });
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}
