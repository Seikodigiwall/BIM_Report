using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DocoptNet;

using pyRevitLabs.Common;
using pyRevitLabs.NLog;
using pyRevitLabs.NLog.Config;
using pyRevitLabs.NLog.Targets;

using pyRevitLabs.PyRevit;
using pyRevitLabs.TargetApps.Revit;
using pyRevitCLI.Properties;

using Console = Colorful.Console;


// NOTE:
// ## Add a new command:
// 1) Update docopt usage pattern file
// 2) Add new command to PyRevitCLICommandType
// 3) Update the logic in PyRevitCLI.ProcessArguments
// 4) Add command code and make sure PyRevitCLI.ProcessArguments correctly parses the arguments
// 5) Update AppHelps to accept and print help for new command type
// 6) Make sure PyRevitCLI.ProcessArguments checks and ask for help print


namespace pyRevitCLI {

    internal enum PyRevitCLILogLevel {
        Quiet,
        InfoMessages,
        Debug,
    }

    internal enum PyRevitCLICommandType {
        Main,
        Version,
        Wiki,
        Blog,
        Docs,
        Source,
        YouTube,
        Support,
        Env,
        Update,
        Clone,
        Clones,
        Attach,
        Attached,
        Switch,
        Detach,
        Extend,
        Extensions,
        ExtensionsPaths,
        ExtensionsSources,
        Releases,
        Revits,
        Run,
        Caches,
        Config,
        Configs,
    }

    internal static class PyRevitCLI {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // uage patterns
        internal static string UsagePatterns => Resources.UsagePatterns;

        // arguments dict
        private static IDictionary<string, ValueObject> arguments = null;

        internal static bool IsVersionMode = false;
        internal static bool IsHelpMode = false;
        internal static bool IsHelpUsagePatternMode = false;

        // cli version property
        public static Version CLIVersion => Assembly.GetExecutingAssembly().GetName().Version;

        // cli entry point:
        static void Main(string[] args) {

            // process arguments for logging level
            var argsList = new List<string>(args);

            // check for testing and set the global test flag
            if (argsList.Contains("--test")) {
                argsList.Remove("--test");
                GlobalConfigs.UnderTest = true;
            }

            // setup logger
            // process arguments for hidden debug mode switch
            PyRevitCLILogLevel logLevel = PyRevitCLILogLevel.InfoMessages;
            var config = new LoggingConfiguration();
            var logconsole = new ConsoleTarget("logconsole") { Layout = @"${level}: ${message} ${exception}" };
            config.AddTarget(logconsole);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, logconsole);

            if (argsList.Contains("--verbose")) {
                argsList.Remove("--verbose");
                logLevel = PyRevitCLILogLevel.InfoMessages;
                config.AddRule(LogLevel.Info, LogLevel.Info, logconsole);
            }

            if (argsList.Contains("--debug")) {
                argsList.Remove("--debug");
                logLevel = PyRevitCLILogLevel.Debug;
                config.AddRule(LogLevel.Debug, LogLevel.Debug, logconsole);
            }

            // config logger
            LogManager.Configuration = config;

            try {
                // process docopt
                // docopt raises exception if pattern matching fails
                arguments = new Docopt().Apply(UsagePatterns, argsList, exit: false, help: false);

                // print active arguments in debug mode
                if (logLevel == PyRevitCLILogLevel.Debug)
                    PrintArguments(arguments);

                // setup output log
                if (arguments["--log"] != null) {
                    var logfile = new FileTarget("logfile") { FileName = arguments["--log"].Value as string };
                    config.AddTarget(logfile);
                    config.AddRuleForAllLevels(logfile);

                    arguments.Remove("--log");

                    // update logger config
                    LogManager.Configuration = config;
                }

                // check if requesting version
                IsVersionMode = arguments["--version"].IsTrue || arguments["-V"].IsTrue;

                // check if requesting help
                IsHelpMode = arguments["--help"].IsTrue || arguments["-h"].IsTrue;

                // check if requesting help with full usage patterns
                IsHelpUsagePatternMode = arguments["--usage"].IsTrue;

                try {
                    // now call methods based on inputs
                    ProcessArguments();

                    // process global error codes
                    ProcessErrorCodes();
                }
                catch (Exception ex) {
                    LogException(ex, logLevel);
                }

                // Flush and close down internal threads and timers
                LogManager.Shutdown();
            }
            catch (Exception ex) {
                // when docopt fails, print help
                logger.Debug("Arg processing failed. | {0}", ex.Message);
                PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Main);
            }
        }

        // configure env
        private static void PrepareEnv() {
        }

        // cli argument processor
        private static void ProcessArguments() {
            if (IsHelpUsagePatternMode) Console.WriteLine(UsagePatterns.Replace("\t", "    "));

            else if (IsVersionMode) PyRevitCLIAppCmds.PrintVersion();

            else if (all("wiki")) CommonUtils.OpenUrl(PyRevitLabsConsts.WikiUrl);

            else if (all("blog")) CommonUtils.OpenUrl(PyRevitLabsConsts.BlogsUrl);

            else if (all("docs")) CommonUtils.OpenUrl(PyRevitLabsConsts.DocsUrl);

            else if (all("source")) CommonUtils.OpenUrl(PyRevitLabsConsts.SourceRepoUrl);

            else if (all("youtube")) CommonUtils.OpenUrl(PyRevitLabsConsts.YoutubeUrl);

            else if (all("support")) CommonUtils.OpenUrl(PyRevitLabsConsts.SupportUrl);

            else if (all("env")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Env);
                else
                    PyRevitCLIAppCmds.MakeEnvReport(json: arguments["--json"].IsTrue);
            }

            else if (all("update") && !any("clones", "extensions")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Update);
                else
                    PyRevitCLIAppCmds.UpdateRemoteDateSources();
            }

            else if (all("clone")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Clone);
                else
                    PyRevitCLICloneCmds.CreateClone(
                        cloneName: TryGetValue("<clone_name>"),
                        deployName: TryGetValue("<deployment_name>"),
                        branchName: TryGetValue("--branch"),
                        repoUrl: TryGetValue("--source"),
                        imagePath: TryGetValue("--image"),
                        destPath: TryGetValue("--dest")
                    );
            }

            else if (all("clones")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Clones);

                else if (all("info"))
                    PyRevitCLICloneCmds.PrintCloneInfo(TryGetValue("<clone_name>"));

                else if (all("open"))
                    PyRevitCLICloneCmds.OpenClone(TryGetValue("<clone_name>"));

                else if (all("add"))
                    PyRevitCLICloneCmds.RegisterClone(
                        TryGetValue("<clone_name>"),
                        TryGetValue("<clone_path>"),
                        force: arguments["--force"].IsTrue
                        );

                else if (all("forget"))
                    PyRevitCLICloneCmds.ForgetClone(
                        allClones: arguments["--all"].IsTrue,
                        cloneName: TryGetValue("<clone_name>")
                        );

                else if (all("rename"))
                    PyRevitCLICloneCmds.RenameClone(
                        cloneName: TryGetValue("<clone_name>"),
                        cloneNewName: TryGetValue("<clone_new_name>")
                        );

                else if (all("delete"))
                    PyRevitCLICloneCmds.DeleteClone(
                        allClones: arguments["--all"].IsTrue,
                        cloneName: TryGetValue("<clone_name>"),
                        clearConfigs: arguments["--clearconfigs"].IsTrue
                        );

                else if (all("branch"))
                    PyRevitCLICloneCmds.GetSetCloneBranch(
                       cloneName: TryGetValue("<clone_name>"),
                       branchName: TryGetValue("<branch_name>")
                       );

                else if (all("version"))
                    PyRevitCLICloneCmds.GetSetCloneTag(
                       cloneName: TryGetValue("<clone_name>"),
                       tagName: TryGetValue("<tag_name>")
                       );

                else if (all("commit"))
                    PyRevitCLICloneCmds.GetSetCloneCommit(
                       cloneName: TryGetValue("<clone_name>"),
                       commitHash: TryGetValue("<commit_hash>")
                       );

                else if (all("origin"))
                    PyRevitCLICloneCmds.GetSetCloneOrigin(
                       cloneName: TryGetValue("<clone_name>"),
                       originUrl: TryGetValue("<origin_url>"),
                       reset: arguments["--reset"].IsTrue
                       );

                else if (all("deployments"))
                    PyRevitCLICloneCmds.PrintCloneDeployments(TryGetValue("<clone_name>"));

                else if (all("engines"))
                    PyRevitCLICloneCmds.PrintCloneEngines(TryGetValue("<clone_name>"));

                else if (all("update"))
                    PyRevitCLICloneCmds.UpdateClone(
                        allClones: arguments["--all"].IsTrue,
                        cloneName: TryGetValue("<clone_name>")
                        );

                else
                    PyRevitCLICloneCmds.PrintClones();
            }

            else if (all("attach")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Attach);
                else
                    PyRevitCLICloneCmds.AttachClone(
                        cloneName: TryGetValue("<clone_name>"),
                        latest: arguments["latest"].IsTrue,
                        dynamoSafe: arguments["dynamosafe"].IsTrue,
                        engineVersion: TryGetValue("<engine_version>"),
                        revitYear: TryGetValue("<revit_year>"),
                        installed: arguments["--installed"].IsTrue,
                        attached: arguments["--attached"].IsTrue,
                        allUsers: arguments["--allusers"].IsTrue
                        );
            }

            else if (all("detach")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Detach);
                else
                    PyRevitCLICloneCmds.DetachClone(
                        revitYear: TryGetValue("<revit_year>"),
                        all: arguments["--all"].IsTrue
                        );
            }

            else if (all("attached")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Attached);
                else
                    PyRevitCLICloneCmds.ListAttachments(revitYear: TryGetValue("<revit_year>"));
            }

            else if (all("switch")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Switch);
                else
                    PyRevitCLICloneCmds.SwitchAttachment(
                        cloneName: TryGetValue("<clone_name>"),
                        revitYear: TryGetValue("<revit_year>")
                        );
            }

            else if (all("extend")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Extend);

                else if (any("ui", "lib", "run"))
                    PyRevitCLIExtensionCmds.Extend(
                        ui: arguments["ui"].IsTrue,
                        lib: arguments["lib"].IsTrue,
                        run: arguments["run"].IsTrue,
                        extName: TryGetValue("<extension_name>"),
                        destPath: TryGetValue("--dest"),
                        repoUrl: TryGetValue("<repo_url>"),
                        branchName: TryGetValue("--branch")
                        );

                else
                    PyRevitCLIExtensionCmds.Extend(
                        extName: TryGetValue("<extension_name>"),
                        destPath: TryGetValue("--dest"),
                        branchName: TryGetValue("--branch")
                        );
            }

            else if (all("extensions")) {
                if (all("search"))
                    PyRevitCLIExtensionCmds.PrintExtensionDefinitions(
                        searchPattern: TryGetValue("<search_pattern>"),
                        headerPrefix: "Matched"
                    );

                else if (any("info", "help", "open"))
                    PyRevitCLIExtensionCmds.ProcessExtensionInfoCommands(
                        extName: TryGetValue("<extension_name>"),
                        info: arguments["info"].IsTrue,
                        help: arguments["help"].IsTrue,
                        open: arguments["open"].IsTrue
                    );

                else if (all("delete"))
                    PyRevitCLIExtensionCmds.DeleteExtension(TryGetValue("<extension_name>"));

                else if (all("origin"))
                    PyRevitCLIExtensionCmds.GetSetExtensionOrigin(
                        extName: TryGetValue("<extension_name>"),
                        originUrl: TryGetValue("<origin_url>"),
                        reset: arguments["--reset"].IsTrue
                        );

                else if (all("paths")) {
                    if (IsHelpMode)
                        PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.ExtensionsPaths);

                    else if (all("add"))
                        PyRevitCLIExtensionCmds.AddExtensionPath(
                            searchPath: TryGetValue("<extensions_path>")
                        );

                    else if (all("forget"))
                        PyRevitCLIExtensionCmds.ForgetAllExtensionPaths(
                            all: arguments["--all"].IsTrue,
                            searchPath: TryGetValue("<extensions_path>")
                        );

                    else
                        PyRevitCLIExtensionCmds.PrintExtensionSearchPaths();
                }

                else if (any("enable", "disable"))
                    PyRevitCLIExtensionCmds.ToggleExtension(
                        enable: arguments["enable"].IsTrue,
                        extName: TryGetValue("<extension_name>")
                    );

                else if (all("sources")) {
                    Console.WriteLine("dfsdfsd");
                    if (IsHelpMode)
                        PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.ExtensionsSources);

                    else if (all("add"))
                        PyRevitCLIExtensionCmds.AddExtensionLookupSource(
                            lookupPath: TryGetValue("<source_json_or_url>")
                        );

                    else if (all("forget"))
                        PyRevitCLIExtensionCmds.ForgetExtensionLookupSources(
                            all: arguments["--all"].IsTrue,
                            lookupPath: TryGetValue("<source_json_or_url>")
                        );

                    else
                        PyRevitCLIExtensionCmds.PrintExtensionLookupSources();
                }

                else if (all("update"))
                    PyRevitCLIExtensionCmds.UpdateExtension(
                        all: arguments["--all"].IsTrue,
                        extName: TryGetValue("<extension_name>")
                    );

                else if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Extensions);

                else
                    PyRevitCLIExtensionCmds.PrintExtensions();
            }

            else if (all("releases")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Releases);

                else if (all("open"))
                    PyRevitCLIReleaseCmds.OpenReleasePage(
                        searchPattern: TryGetValue("<search_pattern>"),
                        latest: arguments["latest"].IsTrue,
                        listPreReleases: arguments["--pre"].IsTrue
                        );

                else if (all("download"))
                    PyRevitCLIReleaseCmds.DownloadReleaseAsset(
                        arguments["archive"].IsTrue ? GithubReleaseAssetType.Archive : GithubReleaseAssetType.Installer,
                        destPath: TryGetValue("--dest"),
                        searchPattern: TryGetValue("<search_pattern>"),
                        latest: arguments["latest"].IsTrue,
                        listPreReleases: arguments["--pre"].IsTrue
                        );

                else
                    PyRevitCLIReleaseCmds.PrintReleases(
                        searchPattern: TryGetValue("<search_pattern>"),
                        latest: arguments["latest"].IsTrue,
                        printReleaseNotes: arguments["--notes"].IsTrue,
                        listPreReleases: arguments["--pre"].IsTrue
                        );
            }

            else if (all("revits")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Revits);

                else if (all("killall"))
                    PyRevitCLIRevitCmds.KillAllRevits(
                        revitYear: TryGetValue("<revit_year>")
                    );

                else if (all("fileinfo"))
                    PyRevitCLIRevitCmds.ProcessFileInfo(
                        targetPath: TryGetValue("<file_or_dir_path>"),
                        outputCSV: TryGetValue("--csv"),
                        IncludeRVT: arguments["--rvt"].IsTrue,
                        includeRTE: arguments["--rte"].IsTrue,
                        includeRFA: arguments["--rfa"].IsTrue,
                        includeRFT: arguments["--rft"].IsTrue
                        );

                else if (arguments["--supported"].IsTrue)
                    PyRevitCLIRevitCmds.ProcessBuildInfo(
                        outputCSV: TryGetValue("--csv")
                        );

                else
                    PyRevitCLIRevitCmds.PrintLocalRevits(running: arguments["--installed"].IsFalse);
            }

            else if (all("run")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Run);
                else
                    PyRevitCLIRevitCmds.RunPythonCommand(
                        inputCommand: TryGetValue("<script_or_command_name>"),
                        targetFile: TryGetValue("<model_file>"),
                        revitYear: TryGetValue("--revit"),
                        runOptions: new PyRevitRunnerOptions() {
                            PurgeTempFiles = arguments["--purge"].IsTrue,
                            ImportPath = TryGetValue("--import", null)
                        }
                    );
            }

            else if (all("caches")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Caches);

                else if (all("bim360", "clear"))
                    PyRevitCLIAppCmds.ClearCaches(
                        allCaches: arguments["--all"].IsTrue,
                        revitYear: TryGetValue("<revit_year>"),
                        cachetype: TargetCacheType.BIM360Cache
                        );

                else if (all("clear"))
                    PyRevitCLIAppCmds.ClearCaches(
                        allCaches: arguments["--all"].IsTrue,
                        revitYear: TryGetValue("<revit_year>"),
                        cachetype: TargetCacheType.PyRevitCache
                        );
            }

            else if (all("config")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Config);
                else
                    PyRevitCLIConfigCmds.SeedConfigs(
                        templateConfigFilePath: TryGetValue("--from")
                    );
            }

            else if (all("configs")) {
                if (IsHelpMode)
                    PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Configs);

                else if (all("bincache")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetBinaryCaches(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Binary cache is {0}",
                                                        PyRevitConfigs.GetBinaryCaches() ? "Enabled" : "Disabled"));
                }

                else if (all("checkupdates")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetCheckUpdates(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Check Updates is {0}",
                                                        PyRevitConfigs.GetCheckUpdates() ? "Enabled" : "Disabled"));
                }

                else if (all("autoupdate")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetAutoUpdate(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Auto Update is {0}",
                                                        PyRevitConfigs.GetAutoUpdate() ? "Enabled" : "Disabled"));
                }

                else if (all("rocketmode")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetRocketMode(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Rocket Mode is {0}",
                                                        PyRevitConfigs.GetRocketMode() ? "Enabled" : "Disabled"));
                }

                else if (all("logs")) {
                    if (all("none"))
                        PyRevitConfigs.SetLoggingLevel(PyRevitLogLevels.Quiet);

                    else if (all("verbose"))
                        PyRevitConfigs.SetLoggingLevel(PyRevitLogLevels.Verbose);

                    else if (all("debug"))
                        PyRevitConfigs.SetLoggingLevel(PyRevitLogLevels.Debug);

                    else
                        Console.WriteLine(string.Format("Logging Level is {0}", PyRevitConfigs.GetLoggingLevel().ToString()));
                }

                else if (all("filelogging")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetFileLogging(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("File Logging is {0}",
                                                        PyRevitConfigs.GetFileLogging() ? "Enabled" : "Disabled"));
                }

                else if (all("startuptimeout")) {
                    if (arguments["<timeout>"] is null)
                        Console.WriteLine(string.Format("Startup log timeout is set to: {0}",
                                                        PyRevitConfigs.GetStartupLogTimeout()));
                    else
                        PyRevitConfigs.SetStartupLogTimeout(int.Parse(TryGetValue("<timeout>")));
                }

                else if (all("loadbeta")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetLoadBetaTools(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Load Beta is {0}",
                                                        PyRevitConfigs.GetLoadBetaTools() ? "Enabled" : "Disabled"));
                }

                else if (all("cpyversion")) {
                    if (arguments["<cpy_version>"] is null)
                        Console.WriteLine(string.Format("CPython version is set to: {0}",
                                                        PyRevitConfigs.GetCpythonEngineVersion()));
                    else
                        PyRevitConfigs.SetCpythonEngineVersion(int.Parse(TryGetValue("<cpy_version>")));
                }

                else if (all("usercanupdate")) {
                    if (any("yes", "no"))
                        PyRevitConfigs.SetUserCanUpdate(arguments["yes"].IsTrue);
                    else
                        Console.WriteLine(string.Format("User {0} update",
                                                        PyRevitConfigs.GetUserCanUpdate() ? "CAN" : "CAN NOT"));
                }

                else if (all("usercanextend")) {
                    if (any("yes", "no"))
                        PyRevitConfigs.SetUserCanExtend(arguments["yes"].IsTrue);
                    else
                        Console.WriteLine(string.Format("User {0} extend",
                                                        PyRevitConfigs.GetUserCanExtend() ? "CAN" : "CAN NOT"));
                }

                else if (all("usercanconfig")) {
                    if (any("yes", "no"))
                        PyRevitConfigs.SetUserCanConfig(arguments["yes"].IsTrue);
                    else
                        Console.WriteLine(string.Format("User {0} config",
                                                        PyRevitConfigs.GetUserCanConfig() ? "CAN" : "CAN NOT"));

                } 
                
                else if (all("colordocs")) {
                    if (any("enable", "disable"))
                        PyRevitConfigs.SetColorizeDocs(arguments["enable"].IsTrue);
                    else
                        Console.WriteLine(string.Format("Doc Colorizer is {0}",
                                                        PyRevitConfigs.GetColorizeDocs() ? "Enabled" : "Disabled"));
                } 
                
                else if (all("telemetry")) {
                    if (all("utc")) {
                        if (any("yes", "no"))
                            PyRevitConfigs.SetUTCStamps(arguments["yes"].IsTrue);
                        else
                            Console.WriteLine(PyRevitConfigs.GetUTCStamps() ? "Using UTC timestamps" : "Using Local timestamps");
                    }

                    else if (all("file")) {
                        var destPath = TryGetValue("<dest_path>");
                        if (destPath is null)
                            Console.WriteLine(string.Format("Telemetry File Path: {0}", PyRevitConfigs.GetAppTelemetryFlags()));
                        else
                            PyRevitConfigs.EnableTelemetry(telemetryFileDir: destPath);
                    }

                    else if (all("server")) {
                        var serverUrl = TryGetValue("<dest_path>");
                        if (serverUrl is null)
                            Console.WriteLine(string.Format("Telemetry Server Url: {0}", PyRevitConfigs.GetAppTelemetryFlags()));
                        else
                            PyRevitConfigs.EnableTelemetry(telemetryServerUrl: serverUrl);

                    }

                    else if (all("enable"))
                        PyRevitConfigs.EnableTelemetry();

                    else if (all("disable"))
                        PyRevitConfigs.DisableTelemetry();

                    else {
                        Console.WriteLine(string.Format("Telemetry is {0}",
                                                        PyRevitConfigs.GetTelemetryStatus() ? "Enabled" : "Disabled"));
                        Console.WriteLine(string.Format("File Path: {0}", PyRevitConfigs.GetTelemetryFilePath()));
                        Console.WriteLine(string.Format("Server Url: {0}", PyRevitConfigs.GetTelemetryServerUrl()));
                    }
                }

                else if (all("apptelemetry")) {
                    if (all("flags")) {
                        var flagsValue = TryGetValue("<flags>");
                        if (flagsValue is null)
                            Console.WriteLine(string.Format("App Telemetry Flags: {0}", PyRevitConfigs.GetAppTelemetryFlags()));
                        else
                            PyRevitConfigs.SetAppTelemetryFlags(flags: flagsValue);
                    }

                    else if (all("server")) {
                        var serverPath = TryGetValue("<server_path>");
                        if (serverPath is null)
                            Console.WriteLine(string.Format("App Telemetry Server: {0}", PyRevitConfigs.GetAppTelemetryServerUrl()));
                        else
                            PyRevitConfigs.EnableAppTelemetry(apptelemetryServerUrl: serverPath);

                    }

                    else if (all("enable"))
                        PyRevitConfigs.EnableAppTelemetry();

                    else if (all("disable"))
                        PyRevitConfigs.DisableAppTelemetry();

                    else {
                        Console.WriteLine(string.Format("App Telemetry is {0}",
                                                        PyRevitConfigs.GetAppTelemetryStatus() ? "Enabled" : "Disabled"));
                        Console.WriteLine(string.Format("Server Url: {0}", PyRevitConfigs.GetAppTelemetryServerUrl()));
                        Console.WriteLine(string.Format("App Telemetry flag is {0}",
                                                        PyRevitConfigs.GetAppTelemetryFlags()));
                    }
                }

                else if (all("outputcss")) {
                    if (arguments["<css_path>"] is null)
                        Console.WriteLine(string.Format("Output Style Sheet is set to: {0}",
                                                        PyRevitConfigs.GetOutputStyleSheet()));
                    else
                        PyRevitConfigs.SetOutputStyleSheet(TryGetValue("<css_path>"));
                }

                else if (all("seed"))
                    PyRevitConfigs.SeedConfig(makeCurrentUserAsOwner: arguments["--lock"].IsTrue);

                else if (any("enable", "disable")) {
                    if (arguments["<option_path>"] != null) {
                        // extract section and option names
                        string orignalOptionValue = TryGetValue("<option_path>");
                        if (orignalOptionValue.Split(':').Count() == 2) {
                            string configSection = orignalOptionValue.Split(':')[0];
                            string configOption = orignalOptionValue.Split(':')[1];

                            var cfg = PyRevitConfigs.GetConfigFile();
                            cfg.SetValue(configSection, configOption, arguments["enable"].IsTrue);
                        }
                        else
                            PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Main);
                    }
                }

                else {
                    if (arguments["<option_path>"] != null) {
                        // extract section and option names
                        string orignalOptionValue = TryGetValue("<option_path>");
                        if (orignalOptionValue.Split(':').Count() == 2) {
                            string configSection = orignalOptionValue.Split(':')[0];
                            string configOption = orignalOptionValue.Split(':')[1];

                            var cfg = PyRevitConfigs.GetConfigFile();

                            // if no value provided, read the value
                            var optValue = TryGetValue("<option_value>");
                            if (optValue != null)
                                cfg.SetValue(configSection, configOption, optValue);
                            else if (optValue is null) {
                                var existingVal = cfg.GetValue(configSection, configOption);
                                if (existingVal != null)
                                    Console.WriteLine( string.Format("{0} = {1}", configOption, existingVal));
                                else
                                    Console.WriteLine(string.Format("Configuration key \"{0}\" is not set", configOption));
                            }
                        }
                        else
                            PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Main);
                    }
                }
            }

            else if (IsHelpMode) PyRevitCLIAppHelps.PrintHelp(PyRevitCLICommandType.Main);
        }

        // internal helper functions:
        private static bool all(params string[] keywords) {
            logger.Debug("Checking for all: {0}", string.Join(",", keywords));
            foreach (var keyword in keywords)
                if (!arguments.ContainsKey(keyword) || !arguments[keyword].IsTrue) {
                    logger.Debug("Missing: {0}", keyword);
                    return false;
                }
            return true;
        }

        private static bool any(params string[] keywords) {
            logger.Debug("Checking for any: {0}", string.Join(",", keywords));
            foreach (var keyword in keywords)
                if (arguments[keyword].IsTrue) {
                    logger.Debug("Matching: {0}", keyword);
                    return true;
                }
            return false;
        }

        internal static string TryGetValue(string key, string defaultValue = null) {
            return arguments[key] != null ? arguments[key].Value as string : defaultValue;
        }

        // private:
        private static void PrintArguments(IDictionary<string, ValueObject> arguments) {
            var activeArgs = arguments.Where(x => x.Value != null && (x.Value.IsTrue || x.Value.IsString));
            foreach (var arg in activeArgs)
                Console.WriteLine("{0} = {1}", arg.Key, arg.Value.ToString());
        }

        private static void ProcessErrorCodes() {
        }

        private static void LogException(Exception ex, PyRevitCLILogLevel logLevel) {
            if (logLevel == PyRevitCLILogLevel.Debug)
                logger.Error(string.Format("{0} ({1})\n{2}", ex.Message, ex.GetType().ToString(), ex.StackTrace));
            else
                logger.Error(string.Format("{0}\nRun with \"--debug\" option to see debug messages", ex.Message));
        }
    }
}
