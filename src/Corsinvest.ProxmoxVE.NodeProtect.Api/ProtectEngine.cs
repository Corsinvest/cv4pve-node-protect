/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.NodeProtect.Api;

/// <summary>
/// Backs up configuration from a single Proxmox VE node via SSH.
/// The tar.gz archive is streamed over the SSH channel directly into the caller's
/// target file — no temporary files are created on the node.
/// </summary>
public class ProtectEngine(ILogger<ProtectEngine> logger)
{
    /// <summary>
    /// Default suffix for the tar.gz file produced for a node.
    /// </summary>
    public const string FileNameSuffix = "-config.tar.gz";

    /// <summary>
    /// Date format used by conventional callers for timestamped backup directories.
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd-HH-mm-ss";

    private static string ShellQuote(string value)
    {
        if (value.Contains('\''))
        {
            throw new ArgumentException($"Path must not contain single quotes: {value}", nameof(value));
        }
        return $"'{value}'";
    }

    /// <summary>
    /// Streams a tar.gz of the given paths from the node into <paramref name="targetFilePath"/>.
    /// Nothing is written to the node's filesystem.
    /// </summary>
    /// <param name="node">Host name shown in logs (informational).</param>
    /// <param name="connectionInfo">SSH connection info with auth already configured by the caller.</param>
    /// <param name="paths">Absolute paths on the node to include in the archive.</param>
    /// <param name="targetFilePath">Local destination file. Parent directory must exist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Human-readable operation summary.</returns>
    public async Task<string> BackupNodeAsync(string node,
                                              ConnectionInfo connectionInfo,
                                              IEnumerable<string> paths,
                                              string targetFilePath,
                                              CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        try
        {
            await using var fileStream = File.Create(targetFilePath);
            return await BackupNodeToStreamAsync(node, connectionInfo, paths, fileStream, cancellationToken);
        }
        catch
        {
            DeleteIfExists(targetFilePath);
            throw;
        }
    }

    /// <summary>
    /// Streams a tar.gz of the given paths from the node into <paramref name="destination"/>.
    /// Nothing is written to the node's filesystem nor to the local disk — the caller owns the stream.
    /// </summary>
    /// <param name="node">Host name shown in logs (informational).</param>
    /// <param name="connectionInfo">SSH connection info with auth already configured by the caller.</param>
    /// <param name="paths">Absolute paths on the node to include in the archive.</param>
    /// <param name="destination">Destination stream. Not disposed by this method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Human-readable operation summary.</returns>
    public async Task<string> BackupNodeToStreamAsync(string node,
                                                      ConnectionInfo connectionInfo,
                                                      IEnumerable<string> paths,
                                                      Stream destination,
                                                      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionInfo);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(destination);

        var sw = Stopwatch.StartNew();

        using var sshClient = new SshClient(connectionInfo);
        await sshClient.ConnectAsync(cancellationToken);

        var quotedPaths = string.Join(" ", paths.Select(ShellQuote));
        // -f - streams the archive to stdout so we can pipe it over SSH without touching disk on the node.
        // --ignore-failed-read tolerates paths that don't exist on the node instead of failing the whole run.
        using var cmd = sshClient.CreateCommand($"tar --one-file-system --ignore-failed-read -czPf - {quotedPaths}");

        logger.LogDebug("[{Node}] Executing: {Cmd}", node, cmd.CommandText);

        var asyncResult = cmd.BeginExecute();
        await cmd.OutputStream.CopyToAsync(destination, cancellationToken);
        cmd.EndExecute(asyncResult);

        // tar exit: 0 ok, 1 = some files differed/changed during read (non-fatal, common on live FS),
        // >=2 = real error
        if (cmd.ExitStatus >= 2)
        {
            throw new InvalidOperationException($"[{node}] tar failed (exit {cmd.ExitStatus}): {cmd.Error}");
        }

        sw.Stop();
        logger.LogDebug("[{Node}] Backup streamed in {TotalSeconds:F2} seconds", node, sw.Elapsed.TotalSeconds);

        return $"[{node}] tar.gz streamed in {sw.Elapsed.TotalSeconds:F2} sec";
    }

    private void DeleteIfExists(string path)
    {
        if (!File.Exists(path)) { return; }
        try { File.Delete(path); }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to delete partial local file {Path}", path); }
    }
}
