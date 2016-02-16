using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server.Configuration
{
    public class DiagnosticReporter
    {
        readonly StringBuilder _report = new StringBuilder();
        readonly UserConfiguration _userConfig = UserConfiguration.Current;

        public string GetVerificationReport()
        {
            RunReport();
            return _report.ToString();
        }

        void RunReport()
        {
            DumpAppSettings();
            CheckUserConfigurationFile();
            CheckRepositoryDirectory();
            CheckGitSettings();
            CheckFederatedAuth();
            CheckADMembership();
            CheckInternalMembership();
            ExceptionLog();
        }

        void ExceptionLog()
        {
            _report.AppendLine("Exception Log");
            SafelyRun(() =>
            {
                var loggingListener = Trace.Listeners.OfType<TextWriterTraceListener>().FirstOrDefault();
                if (loggingListener != null)
                {
                    var writer = loggingListener.Writer as StreamWriter;
                    if (writer != null)
                    {
                        var fileStream = writer.BaseStream as FileStream;
                        if (fileStream != null)
                        {
                            SafelyReport("LogFileName: ", () => fileStream.Name);
                            var chunkSize = 10000;
                            var length = new FileInfo(fileStream.Name).Length;
                            Report("Log File total length", length);

                            var startingPoint = Math.Max(0, length - chunkSize);
                            Report("Starting log dump from ", startingPoint);

                            using (var logText = File.Open(fileStream.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                logText.Seek(startingPoint, SeekOrigin.Begin);
                                var reader = new StreamReader(logText);
                                _report.AppendLine(reader.ReadToEnd());
                            }
                        }
                    }
                }
            });
        }

        void DumpAppSettings()
        {
            _report.AppendLine("Web.Config AppSettings");
            foreach (string key in ConfigurationManager.AppSettings)
            {
                QuotedReport("AppSettings."+key, ConfigurationManager.AppSettings[key]);
            }
        }

        void CheckUserConfigurationFile()
        {
            _report.AppendLine("User Configuration:");
            var configFile = MapPath(AppSetting("UserConfiguration"));
            QuotedReport("User Config File", configFile);
            SafelyReport("User config readable", () => !String.IsNullOrEmpty(File.ReadAllText(configFile)));
        }

        void CheckRepositoryDirectory()
        {
            _report.AppendLine("Repo Directory");
            QuotedReport("Configured repo path", _userConfig.RepositoryPath);
            QuotedReport("Effective repo path", _userConfig.Repositories);
            SafelyReport("Repo folder exists", () => Directory.Exists(_userConfig.Repositories));
        }

        void CheckGitSettings()
        {
            _report.AppendLine("Git Exe");
            var gitPath = MapPath(AppSetting("GitPath"));
            QuotedReport("Git path", gitPath);
            SafelyReport("Git.exe exists", () => File.Exists(gitPath));
        }

        void CheckFederatedAuth()
        {
            _report.AppendLine("Federated Authentication");
            if (AppSetting("AuthenticationProvider") == "Federation")
            {
                SafelyReport("Metadata available", () =>
                {
                    WebClient client = new WebClient();
                    var metadata = client.DownloadString(AppSetting("FederationMetadataAddress"));
                    return !String.IsNullOrWhiteSpace(metadata);
                });

            }
            else
            {
                Report("Not Enabled", "");
            }
        }

        void CheckADMembership()
        {
            _report.AppendLine("Active Directory");

            if (AppSetting("MembershipService") == "ActiveDirectory")
            {
                SafelyReport("Backand folder exists", () => Directory.Exists(MapPath(AppSetting("ActiveDirectoryBackendPath"))));

                var ad = ADBackend.Instance;
                SafelyReport("Users", () => String.Join(",", ad.Users.Select(user => user.Name)));

                _report.AppendLine("AD Teams");
                SafelyRun(() =>
                {
                    foreach (var item in ad.Teams)
                    {
                        var thisTeam = item;
                        SafelyReport(item.Name, () => String.Join(",", thisTeam.Members));
                    }
                });
                _report.AppendLine("AD Roles");
                SafelyRun(() =>
                {
                    foreach (var item in ad.Roles)
                    {
                        var thisRole = item;
                        SafelyReport(item.Name, () => String.Join(",", thisRole.Members));
                    }
                });
            }
            else
            {
                Report("Not Enabled");
            }
        }

        void CheckInternalMembership()
        {
            _report.AppendLine("Internal Membership");

            if (AppSetting("MembershipService") == "Internal")
            {
                SafelyReport("Users", () => String.Join(",", new EFMembershipService().GetAllUsers().Select(member => member.Name)));
            }
            else
            {
                Report("Not Enabled");
            }
        }

        void SafelyRun(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Report("Diag error", FormatException(ex));
            }
        }

        void SafelyReport(string tag, Func<object> func)
        {
            try
            {
                object result = func();
                if (result is bool)
                {
                    if ((bool)result)
                    {
                        Report(tag, "OK");
                    }
                    else
                    {
                        Report(tag, "FAIL");
                    }
                }
                else
                {
                    Report(tag, result.ToString());
                }
            }
            catch (Exception ex)
            {
                Report(tag, FormatException(ex));
            }
        }

        string MapPath(string path)
        {
            return Path.IsPathRooted(path) ? path : HostingEnvironment.MapPath(path);
        }

        string AppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        static string FormatException(Exception ex)
        {
            return "EXCEPTION: " + ex.ToString().Replace("\r\n", "***");
        }

        void Report(string tag, object value = null)
        {
            if (value != null)
            {
                _report.AppendFormat("--{0}: {1}" + Environment.NewLine, tag, value);
            }
            else
            {
                _report.AppendLine("--" + tag);
            }
        }
        void QuotedReport(string tag, object value)
        {
            Report(tag, "'"+value+"'");
        }
    }
}