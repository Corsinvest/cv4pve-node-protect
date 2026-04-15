/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Api.Shared;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.NodeProtect.Api;

/// <summary>
/// Node protect
/// </summary>
public class ProtectEngine(string hostsAndPort,
                           string username,
                           string password,
                           string passphrase,
                           string privateKeyFile,
                           int? timeout)
{
    /// <summary>
    /// Date format directory
    /// </summary>
    public static readonly string FORMAT_DATE = "yyyy-MM-dd-HH-mm-ss";

    private const string FILE_NAME = "-config.tar.gz";

    private static readonly Regex DateFolderRegex = new(@"^\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}$", RegexOptions.Compiled);

    private static (string Host, int Port) ParseHostAndPort(string hostAndPort)
    {
        var data = hostAndPort.Split(':');
        var host = data[0];
        var port = data.Length == 2
                        ? int.TryParse(data[1], out var result2) ? result2 : 22
                        : 22;

        return (host, port);
    }

    private static string ShellQuote(string value)
    {
        if (value.Contains('\''))
        {
            throw new ArgumentException($"Path must not contain single quotes: {value}", nameof(value));
        }
        return $"'{value}'";
    }

    /// <summary>
    /// Backup
    /// </summary>
    public async Task BackupAsync(string[] pathsToBackup,
                                  string directoryWork,
                                  int keep,
                                  TextWriter @out,
                                  ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<ProtectEngine>();

        @out.WriteLine($@"ACTION Backup
Keep: {keep}
Directory Work: {directoryWork}
Directory Node to archive:");

        foreach (var item in pathsToBackup) { @out.WriteLine(item); }

        var date = DateTime.Now;

        //create folder date
        var pathSave = Path.Combine(directoryWork, date.ToString(FORMAT_DATE));
        Directory.CreateDirectory(pathSave);

        foreach (var hostAndPort in hostsAndPort.Split(','))
        {
            var (host, port) = ParseHostAndPort(hostAndPort);

            var ret = await BackupAsync(host, port, pathsToBackup, logger);
            var fileToSave = Path.Combine(pathSave, $"{host}{FILE_NAME}");

            await using (var fileStream = File.Create(fileToSave))
                await ret.Stream.CopyToAsync(fileStream);

            @out.WriteLine($"Create config: {fileToSave}");
        }

        //keep: only prune directories whose name matches the timestamp format
        foreach (var item in Directory.GetDirectories(directoryWork)
                                      .Where(d => DateFolderRegex.IsMatch(Path.GetFileName(d)))
                                      .OrderByDescending(a => a)
                                      .Skip(keep))
        {
            @out.WriteLine($"Delete Backup: {item}");
            Directory.Delete(item, true);
        }
    }

    private ConnectionInfo GetConnectionInfo(string node, int port)
    {
        var connectionInfo = new ConnectionInfo(node, port, username, GetAuthMethod());
        if (timeout.HasValue) { connectionInfo.Timeout = TimeSpan.FromSeconds(timeout.Value); }
        return connectionInfo;
    }

    private AuthenticationMethod GetAuthMethod()
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new PveException("Username is required!");
        }

        if (!string.IsNullOrEmpty(privateKeyFile))
        {
            var keyFile = string.IsNullOrEmpty(passphrase)
                            ? new PrivateKeyFile(privateKeyFile)
                            : new PrivateKeyFile(privateKeyFile, passphrase);
            return new PrivateKeyAuthenticationMethod(username, keyFile);
        }
        else if (!string.IsNullOrEmpty(password))
        {
            return new PasswordAuthenticationMethod(username, password);
        }
        else
        {
            throw new PveException("Invalid authentication!");
        }
    }

    /// <summary>
    /// Backup to stream
    /// </summary>
    public async Task<(string Logs, Stream Stream)> BackupAsync(string node,
                                                                int port,
                                                                IEnumerable<string> paths,
                                                                ILogger logger)
    {
        var totalSw = Stopwatch.StartNew();
        var stream = new MemoryStream();
        var logs = new StringBuilder();

        using var sshClient = new SshClient(GetConnectionInfo(node, port));
        await sshClient.ConnectAsync(CancellationToken.None);

        async Task<(string StdOut, int ExitCode, string StdErr)> execCmdAsync(string command)
        {
            using var c = sshClient.CreateCommand(command);
            await c.ExecuteAsync();
            return (c.Result, c.ExitStatus ?? -1, c.Error);
        }

        // Create a random, non-predictable temp file on the remote node
        var mkTempRet = await execCmdAsync("mktemp /tmp/cv4pve-node-protect-XXXXXXXX.tar.gz");
        if (mkTempRet.ExitCode != 0 || string.IsNullOrWhiteSpace(mkTempRet.StdOut))
        {
            sshClient.Disconnect();
            throw new PveException($"[{node}] Cannot create temp file: {mkTempRet.StdErr}");
        }
        var remoteTarGz = mkTempRet.StdOut.Trim();

        try
        {
            // Restrict permissions before writing sensitive config
            await execCmdAsync($"chmod 600 {ShellQuote(remoteTarGz)}");

            // Create tar.gz
            var sw = Stopwatch.StartNew();
            logs.AppendLine($"[{node}] Create file tar.gz: {remoteTarGz}");

            var quotedPaths = string.Join(" ", paths.Select(ShellQuote));
            var cmd = $"tar --one-file-system -cvzPf {ShellQuote(remoteTarGz)} {quotedPaths}";
            var ret = await execCmdAsync(cmd);
            sw.Stop();

            logger.LogDebug("Create tar.gz: {Cmd}", cmd);
            logger.LogDebug("Result: {ExitCode}, Time: {Time}ms", ret.ExitCode, sw.ElapsedMilliseconds);

            logs.Append(ret.StdOut);
            logs.AppendLine($"[{node}] Created tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");

            // Download file
            sw.Restart();
            using (var sftpClient = new SftpClient(GetConnectionInfo(node, port)))
            {
                await sftpClient.ConnectAsync(CancellationToken.None);
                try
                {
                    await sftpClient.DownloadFileAsync(remoteTarGz, stream);
                    stream.Position = 0;
                }
                finally
                {
                    sftpClient.Disconnect();
                }
            }
            sw.Stop();
            logs.AppendLine($"[{node}] Downloaded tar.gz in {sw.Elapsed.TotalSeconds:F2} sec");
        }
        finally
        {
            // Always try to remove remote temp file, even on failure
            try
            {
                var rmRet = await execCmdAsync($"rm -f {ShellQuote(remoteTarGz)}");
                logger.LogDebug("Delete tar.gz: Result {ExitCode}", rmRet.ExitCode);
                logs.AppendLine($"[{node}] Deleted remote tar.gz");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete remote file {Remote}", remoteTarGz);
            }

            sshClient.Disconnect();
        }

        totalSw.Stop();
        logger.LogDebug("Backup completed in {TotalSeconds:F2} seconds", totalSw.Elapsed.TotalSeconds);

        return (logs.ToString(), stream);
    }
}
