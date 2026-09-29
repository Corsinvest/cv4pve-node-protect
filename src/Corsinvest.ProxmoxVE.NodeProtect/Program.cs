/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.CommandLine;
using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Api.Console.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

internal partial class Program
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}$")]
    private static partial Regex DateFolderRegex();

    private static async Task<int> Main(string[] args)
    {
        var app = new RootCommand("Node protect for Proxmox VE");
        app.AddFullNameLogo();
        app.AddDebugOption();
        app.AddLogLevelOption();

        var optUsername = app.AddOption<string>("--username", "User name");
        var optPassword = app.AddOption<string>("--password", "Password, or 'file:/path/to/file' to read from file");
        optPassword.Required = false;

        var optPrivateKeyFile = app.AddOption<string>("--private-key-file", "Private key file")
                                   .AddValidatorExistFile();

        var optPassphrase = app.AddOption<string>("--passphrase", "Passphrase for private key file");

        var optHost = new Option<string>("--host")
        {
            Description = "Comma-separated list of hosts. Each entry is host[:port]. "
                        + "Supported formats: hostname, IPv4 (192.168.0.1), IPv6 in brackets ([fe80::1]), "
                        + "bare IPv6 without port (fe80::1). Default port is 22. "
                        + "Examples: 'pve01,pve02', '192.168.0.1:2222', '[::1]:22,pve01'.",
            Required = true,
            CustomParser = (e) => e.Tokens.Single().Value
        };

        app.Add(optHost);

        var optTimeout = app.AddOption<int?>("--timeout", "Timeout in seconds for ssh connection");

        // ConsoleHelper.CreateLoggerFactory filters only Program and the PVE API client:
        // the engine logs under its own category, so --debug / --log-level must reach it too.
        var logLevel = app.GetLogLevelFromDebug();
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter(typeof(Program).FullName, logLevel)
                   .AddFilter(typeof(ProtectEngine).FullName, logLevel)
                   .AddConsole();
        });

        var cmdBackup = app.AddCommand("backup", "Backup configuration from nodes using ssh");
        var optKeep = cmdBackup.AddOption<int>("--keep", "Specify the number of backups to retain")
                               .AddValidatorRange(1, 100);
        optKeep.Required = true;

        var optPathBackup = cmdBackup.AddOption<string>("--paths", "Paths to backup ';' separated");
        optPathBackup.Required = true;

        var optDirectoryWorkBackup = cmdBackup.AddOption<string>("--directory-work", "Directory work")
                                              .AddValidatorExistDirectory();
        optDirectoryWorkBackup.Required = true;

        cmdBackup.SetAction(async (action) => await RunBackupAsync(action));

        return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger<Program>());

        async Task RunBackupAsync(ParseResult action)
        {
            var username = action.GetValue(optUsername);
            var password = ResolvePassword(action.GetValue(optPassword));
            var privateKeyFile = action.GetValue(optPrivateKeyFile);
            var passphrase = action.GetValue(optPassphrase);
            var timeout = action.GetValue(optTimeout);
            var hostsAndPort = action.GetValue(optHost)!;
            var paths = action.GetValue(optPathBackup)!.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var directoryWork = action.GetValue(optDirectoryWorkBackup)!;
            var keep = action.GetValue(optKeep);

            ValidateAuth(username, password, privateKeyFile);

            var engine = new ProtectEngine(loggerFactory.CreateLogger<ProtectEngine>());

            var @out = Console.Out;
            @out.WriteLine($@"ACTION Backup
Keep: {keep}
Directory Work: {directoryWork}
Directory Node to archive:");
            foreach (var p in paths) { @out.WriteLine(p); }

            // Parse every host before creating anything, so a typo does not leave an empty dated folder
            var hosts = hostsAndPort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(ParseHostAndPort)
                                    .ToList();

            // Create timestamped directory for this run (owner only on Unix: archives contain secrets)
            var pathSave = Path.Combine(directoryWork, DateTime.Now.ToString(ProtectEngine.DateFormat));
            if (OperatingSystem.IsWindows()) { Directory.CreateDirectory(pathSave); }
            else { Directory.CreateDirectory(pathSave, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }

            // Backup each host: a failing node does not stop the others
            var failed = new List<string>();
            foreach (var (host, port) in hosts)
            {
                var targetFile = Path.Combine(pathSave, $"{ToFileName(host)}{ProtectEngine.FileNameSuffix}");
                try
                {
                    var connectionInfo = BuildConnectionInfo(host, port, username!, password, passphrase, privateKeyFile, timeout);
                    await engine.BackupNodeAsync(host, connectionInfo, paths, targetFile);
                    @out.WriteLine($"Create config: {Path.GetRelativePath(directoryWork, targetFile)}");
                }
                catch (Exception ex)
                {
                    failed.Add(host);
                    @out.WriteLine($"ERROR [{host}]: {ex.Message}");
                    loggerFactory.CreateLogger<Program>().LogDebug(ex, "Backup failed for {Host}", host);
                }
            }

            if (failed.Count > 0)
            {
                // An empty folder would count as a backup for --keep on the next runs
                if (!Directory.EnumerateFileSystemEntries(pathSave).Any()) { Directory.Delete(pathSave); }

                // Retention skipped: it must not delete good backups to make room for an incomplete one
                throw new InvalidOperationException($"Backup failed for {failed.Count} of {hosts.Count} node(s): {string.Join(", ", failed)}. Retention not applied.");
            }

            // Retention: keep only the latest N timestamped directories
            foreach (var item in Directory.GetDirectories(directoryWork)
                                          .Where(d => DateFolderRegex().IsMatch(Path.GetFileName(d)))
                                          .OrderByDescending(a => a)
                                          .Skip(keep))
            {
                @out.WriteLine($"Delete Backup: {Path.GetFileName(item)}");
                Directory.Delete(item, true);
            }
        }

        // IPv6 addresses contain ':', not allowed in Windows file names
        static string ToFileName(string host)
            => string.Concat(host.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        static string? ResolvePassword(string? password)
        {
            if (string.IsNullOrEmpty(password)) { return password; }
            if (!password.StartsWith("file:", StringComparison.Ordinal)) { return password; }

            var path = password["file:".Length..];
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Password file not found: {path}", path);
            }
            return File.ReadAllText(path).Trim();
        }

        static void ValidateAuth(string? username, string? password, string? privateKeyFile)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("Option '--username' is required!");
            }

            if (string.IsNullOrEmpty(privateKeyFile) && string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Option '--password' is required when using '--username' without a private key!");
            }
        }

        static (string Host, int Port) ParseHostAndPort(string hostAndPort)
        {
            const int DefaultPort = 22;

            // IPv6 in brackets: [addr] or [addr]:port (canonical "host:port" notation for IPv6)
            if (hostAndPort.StartsWith('['))
            {
                var closeBracket = hostAndPort.IndexOf(']');
                if (closeBracket < 0) { throw new ArgumentException($"Invalid IPv6 format, missing ']': {hostAndPort}"); }

                var hostV6 = hostAndPort[1..closeBracket];
                var rest = hostAndPort[(closeBracket + 1)..];

                if (rest.Length == 0) { return (hostV6, DefaultPort); }
                if (!rest.StartsWith(':') || !int.TryParse(rest[1..], out var portV6))
                {
                    throw new ArgumentException($"Invalid port after ']' in: {hostAndPort}");
                }
                return (hostV6, portV6);
            }

            // IPv6 without brackets (e.g. "fe80::1"): more than one ':' → treat whole string as host, default port
            if (hostAndPort.Count(c => c == ':') > 1) { return (hostAndPort, DefaultPort); }

            // IPv4 or hostname, optionally with single ":port"
            var parts = hostAndPort.Split(':');
            var port = parts.Length == 2 && int.TryParse(parts[1], out var p) ? p : DefaultPort;
            return (parts[0], port);
        }

        static ConnectionInfo BuildConnectionInfo(string host,
                                                  int port,
                                                  string username,
                                                  string? password,
                                                  string? passphrase,
                                                  string? privateKeyFile,
                                                  int? timeoutSeconds)
        {
            var authMethod = BuildAuthMethod(username, password, passphrase, privateKeyFile);
            var connectionInfo = new ConnectionInfo(host, port, username, authMethod);
            if (timeoutSeconds.HasValue) { connectionInfo.Timeout = TimeSpan.FromSeconds(timeoutSeconds.Value); }
            return connectionInfo;
        }

        static AuthenticationMethod BuildAuthMethod(string username,
                                                    string? password,
                                                    string? passphrase,
                                                    string? privateKeyFile)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("Username is required!");
            }

            if (!string.IsNullOrEmpty(privateKeyFile))
            {
                var keyFile = string.IsNullOrEmpty(passphrase)
                                ? new PrivateKeyFile(privateKeyFile)
                                : new PrivateKeyFile(privateKeyFile, passphrase);
                return new PrivateKeyAuthenticationMethod(username, keyFile);
            }

            if (!string.IsNullOrEmpty(password))
            {
                return new PasswordAuthenticationMethod(username, password);
            }

            throw new InvalidOperationException("Invalid authentication!");
        }
    }
}
