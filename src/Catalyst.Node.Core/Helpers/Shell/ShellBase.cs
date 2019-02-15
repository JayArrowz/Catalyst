using System;
using System.Reflection;
using System.Security;
using System.Text;
using Catalyst.Node.Common.Shell;
using Serilog;

namespace Catalyst.Node.Core.Helpers.Shell
{
    public abstract class ShellBase : IShell
    {
        private static readonly ILogger Logger = Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public virtual string Prompt => "ADS";
        protected bool ShowPrompt { private get; set; } = true;
        private static string ServiceName => "Catalyst Distributed Shell";

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnStart(string[] args);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnStartNode(string[] args);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnStartWork(string[] args);

        /// <summary>
        /// </summary>
        public abstract bool OnStop(string[] args);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnStopNode(string[] args);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnStopWork(string[] args);

        /// <summary>
        ///     Prints a list of available cli commands.
        /// </summary>
        /// <returns></returns>
        public bool OnHelpCommand(string advancedCmds = "")
        {
            var normalCmds =
                "Normal Commands:\n" +
                "\tstart node\n" +
                "\tstart work\n" +
                "\tstop node\n" +
                "\tstop work\n" +
                "\tget info\n" +
                "\tget config\n" +
                "\tget version\n" +
                "\thelp\n" +
                "\tclear\n" +
                "\texit\n";

            Logger.Information(normalCmds);

            if (advancedCmds != "") Logger.Information(advancedCmds);
            return true;
        }

        /// <summary>
        ///     Catalyst.Cli command service router.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual bool OnCommand(string[] args)
        {
            switch (args[0].ToLower())
            {
                case "get":
                    return OnGetCommand(args);
                case "help":
                    return OnHelpCommand();
                case "clear":
                    Console.Clear();
                    return true;
                case "exit":
                    Logger.Information("exit trace");
                    return false;
                default:
                    return CommandNotFound(args);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool OnGetCommand(string[] args)
        {
            switch (args[1].ToLower())
            {
                case "info":
                    return OnGetInfo();
                case "config":
                    return OnGetConfig();
                case "version":
                    return OnGetVersion();
                case "mempool":
                    return OnGetMempool();
                default:
                    return CommandNotFound(args);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool OnGetInfo();

        /// <summary>
        ///     Prints the current loaded settings.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnGetConfig();

        /// <summary>
        ///     Prints the current node version.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnGetVersion();

        /// <summary>
        ///     Prints stats about the mempool implementation.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnGetMempool();

        /// <summary>
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public string ReadPassword(string prompt)
        {
            const string t =
                " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            Logger.Information(prompt);
            Logger.Information(": ");

            Console.ForegroundColor = ConsoleColor.Yellow;

            do
            {
                key = Console.ReadKey(true);
                if (t.IndexOf(key.KeyChar) != -1)
                {
                    sb.Append(key.KeyChar);
                    Logger.Information('*'.ToString());
                }
                else if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Logger.Information(key.KeyChar.ToString());
                    Logger.Information(' '.ToString());
                    Logger.Information(key.KeyChar.ToString());
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.ForegroundColor = ConsoleColor.White;
            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public SecureString ReadSecureString(string prompt)
        {
            const string t =
                " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            var securePwd = new SecureString();
            ConsoleKeyInfo key;
            Logger.Information(prompt);
            Logger.Information(": ");

            Console.ForegroundColor = ConsoleColor.Yellow;

            do
            {
                key = Console.ReadKey(true);
                if (t.IndexOf(key.KeyChar) != -1)
                {
                    securePwd.AppendChar(key.KeyChar);
                    Logger.Information('*'.ToString());
                }
                else if (key.Key == ConsoleKey.Backspace && securePwd.Length > 0)
                {
                    securePwd.RemoveAt(securePwd.Length - 1);
                    Logger.Information(key.KeyChar.ToString());
                    Logger.Information(' '.ToString());
                    Logger.Information(key.KeyChar.ToString());
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.ForegroundColor = ConsoleColor.White;
            securePwd.MakeReadOnly();
            return securePwd;
        }

        /// <summary>
        ///     Runs the main cli ui.
        /// </summary>
        /// <returns></returns>
        public bool RunConsole()
        {
            var running = true;

            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            Logger.Information($"{ServiceName} Version: {ver}");

            while (running)
            {
                if (ShowPrompt)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Logger.Information($"{Prompt}> ");
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                var line = Console.ReadLine()?.Trim();
                if (line == null) break;
                Console.ForegroundColor = ConsoleColor.White;
                var args = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0)
                    continue;

                try
                {
                    running = OnCommand(args);
                }
                catch (SystemException ex)
                {
                    Logger.Error(ex, "Exception raised in Shell");
                }
            }

            Console.ResetColor();
            return running;
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool CommandNotFound(string[] args)
        {
            Logger.Error("error: command not found " + args);
            return true;
        }
    }
}