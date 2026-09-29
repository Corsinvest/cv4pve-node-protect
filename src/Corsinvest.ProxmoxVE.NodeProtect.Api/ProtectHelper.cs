/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Text.RegularExpressions;

namespace Corsinvest.ProxmoxVE.NodeProtect.Api;

/// <summary>
/// Host parsing, archive naming and retention used by the command line tool, for callers that
/// want the same layout: one dated folder per run, one archive per node.
/// </summary>
public static partial class ProtectHelper
{
    /// <summary>
    /// SSH port used when a host has none.
    /// </summary>
    public const int DefaultPort = 22;

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}$")]
    private static partial Regex DateFolderRegex();

    /// <summary>
    /// Parses a comma-separated list of hosts, each <c>host[:port]</c>.
    /// </summary>
    /// <param name="hosts">E.g. <c>pve01,192.168.0.1:2222,[fe80::1]:22</c>.</param>
    /// <returns>Host and port of each entry, in order.</returns>
    public static IReadOnlyList<(string Host, int Port)> ParseHosts(string hosts)
        => [.. hosts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(ParseHostAndPort)];

    /// <summary>
    /// Parses one <c>host[:port]</c>: host name, IPv4, IPv6 in brackets (<c>[fe80::1]:2222</c>)
    /// or bare IPv6 without port (<c>fe80::1</c>). The port defaults to <see cref="DefaultPort"/>.
    /// </summary>
    /// <param name="hostAndPort">The entry to parse.</param>
    /// <returns>Host and port.</returns>
    public static (string Host, int Port) ParseHostAndPort(string hostAndPort)
    {
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

    /// <summary>
    /// Archive file name for a host: the host followed by <see cref="ProtectEngine.FileNameSuffix"/>.
    /// Characters not allowed in file names on this system (the <c>:</c> of IPv6 addresses on Windows)
    /// are replaced by <c>_</c>.
    /// </summary>
    /// <param name="host">Host as given by the user, without port.</param>
    /// <returns>File name, without folder.</returns>
    public static string GetArchiveFileName(string host)
        => string.Concat(host.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c))
           + ProtectEngine.FileNameSuffix;

    /// <summary>
    /// Whether a folder name is a dated backup folder (<see cref="ProtectEngine.DateFormat"/>).
    /// </summary>
    /// <param name="name">Folder name, without path.</param>
    /// <returns><c>true</c> for names like <c>2026-09-29-03-00-01</c>.</returns>
    public static bool IsBackupDirectoryName(string name) => DateFolderRegex().IsMatch(name);

    /// <summary>
    /// The dated backup folders to delete to keep only the newest <paramref name="keep"/>.
    /// Folders whose name is not a date are never returned.
    /// </summary>
    /// <param name="directories">Folders found in the work directory (full paths or names).</param>
    /// <param name="keep">Number of dated folders to keep.</param>
    /// <returns>The folders to delete, newest first.</returns>
    public static IEnumerable<string> GetBackupsToDelete(IEnumerable<string> directories, int keep)
        => directories.Where(d => IsBackupDirectoryName(Path.GetFileName(d)))
                      .OrderByDescending(d => Path.GetFileName(d), StringComparer.Ordinal)
                      .Skip(keep);
}
