using System.Diagnostics;

namespace Executor
{
    /* UPGRADE IDEAS:
     * - Make it possible to execute ALL apps in the target.txt file.
     * - Make it possible to add launch arguments to the target.txt file.
     */
    internal class Program
    {
        const string TARGET_FILE_NAME = "executor_target.txt";
        const string LOG_FILE_NAME = "executor_log.txt";

        static void Main(string[] args)
        {
            string target = "";
            if (File.Exists(TARGET_FILE_NAME))
            {
                using StreamReader sr = File.OpenText(TARGET_FILE_NAME);

                string? content = sr.ReadToEnd();
                if (content == null)
                {
                    Log($"Invalid executor_target, no path found.");
                    System.Environment.Exit(1);
                }

                target = VerifyAndAssignTarget(content);
            }
            else if (args.Length > 0)
            {
                string content = args[0];

                target = VerifyAndAssignTarget(content);
            }
            else
            {
                Log($"Cannot find {TARGET_FILE_NAME}, please create one.");
                System.Environment.Exit(1);
            }
            
            if(string.IsNullOrEmpty(target))
            {
                Log($"Invalid executor_target. Unexpected malformed target: {target}.");
                System.Environment.Exit(1);
            }

            ExecTarget(target);
        }

        static void ExecTarget(string pPath)
        {
            try
            {
                ProcessStartInfo processInfo;
                Process process;

                string fileExtension = Path.GetExtension(pPath).Trim();
                if (fileExtension == ".bat")
                {
                    // It's a batch script, execute it with CMD.
                    processInfo = new ProcessStartInfo("cmd.exe", "/c " + pPath);
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = true; // Do not wait - make the process stand alone
                    process = Process.Start(processInfo);
                }
                else if (fileExtension == ".ps1")
                {
                    // It's a powershell script, execute it with powershell.
                    var scriptArguments = "-File \"" + pPath + "\"";
                    processInfo = new ProcessStartInfo("powershell.exe", scriptArguments);

                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = true; // Do not wait - make the process stand alone
                    process = Process.Start(processInfo);;
                }
                else
                {
                    // Normal .exe assumed
                    ProcessStartInfo sinfo = new ProcessStartInfo();
                    sinfo.UseShellExecute = true; // Do not wait - make the process stand alone
                    sinfo.FileName = pPath;
                    Process.Start(sinfo);
                }
            }
            catch (Exception e) 
            {
                Log($"Unexpected error when running target: {e}");
            }
        }

        static string VerifyAndAssignTarget(string pPath)
        {
            try
            {
                if (!File.Exists(pPath))
                {
                    Log($"Invalid executor_target, the specified target \"{pPath}\" cannot be found.");
                    System.Environment.Exit(1);
                }
                return pPath;
            }
            catch
            {
                Log($"Invalid executor_target. Malformed path: {pPath}.");
                System.Environment.Exit(1);
            }

            return "";
        }

        static void Log(string pMessage)
        {
            bool alreadyExists = File.Exists(LOG_FILE_NAME);
            using StreamWriter sw = new StreamWriter(LOG_FILE_NAME, true);

            sw.BaseStream.Seek(0, SeekOrigin.End);
            if (alreadyExists)
            {
                sw.WriteLine();
            }

            sw.WriteLine($"{DateTime.Now.ToString()}: {pMessage}");

            sw.Flush();
            sw.Close();
        }
    }
}
