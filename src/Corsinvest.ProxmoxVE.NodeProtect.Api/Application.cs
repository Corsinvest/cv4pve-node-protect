/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.NodeProtect.Api;

/// <summary>
/// Node protect
/// </summary>
public class Application
{
    /// <summary>
    /// Date format directory
    /// </summary>
    public static readonly string FORMAT_DATE = "yyyy-MM-dd-HH-mm-ss";

    private const string FILE_NAME = "-config.tar.gz";

    private static DateTime DirectoryToDate(string directory)
        => DateTime.ParseExact(Path.GetFileName(directory), FORMAT_DATE, null);

    private static string FileNameLinuxTarGz(DateTime date) => $"/tmp/{date.ToString(FORMAT_DATE)}.tar.gz";

    private static (string Host, int Port) ParseHostAndPort(string hostAndPort)
    {
        var data = hostAndPort.Split(':');
        var host = data[0];
        var port = data.Length == 2
                        ? int.TryParse(data[1], out var result2) ? result2 : 22
                        : 22;

        return (host, port);
    }


    /// <summary>
    /// Backup
    /// </summary>
    /// <param name="hostsAndPort"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="pathsToBackup"></param>
    /// <param name="directoryWork"></param>
    /// <param name="keep"></param>
    /// <param name="out"></param>
    /// <param name="loggerFactory"></param>
    public static void Backup(string hostsAndPort,
                              string username,
                              string password,
                              string[] pathsToBackup,
                              string directoryWork,
                              int keep,
                              TextWriter @out,
                              ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Application>();

        @out.WriteLine($@"ACTION Backup
Keep: {keep}
Directory Work: {directoryWork}
Directory Node to archive:");

        foreach (var item in pathsToBackup) { @out.WriteLine(item); }

        var date = DateTime.Now;
        var fileNameTarGz = FileNameLinuxTarGz(date);

        //create folder date
        var pathSave = Path.Combine(directoryWork, date.ToString(FORMAT_DATE));
        Directory.CreateDirectory(pathSave);

        foreach (var hostAndPort in hostsAndPort.Split(','))
        {
            var (host, port) = ParseHostAndPort(hostAndPort);

            // Execute a (SHELL) Command for download
            using var sshClient = new SshClient(host, port, username, password);
            //create tar
            sshClient.Connect();

            var cmdCreateTarGz = $"tar -cvzPf {fileNameTarGz} {string.Join(" ", pathsToBackup)}";
            var retCmd = sshClient.CreateCommand(cmdCreateTarGz).Execute();

            logger.LogDebug("Create file tar.gz: {cmdCreateTarGz}", cmdCreateTarGz);
            logger.LogDebug("Result command: {retCmd}", retCmd);

            var fileToSave = Path.Combine(pathSave, $"{host}{FILE_NAME}");

            // download
            using var sftp = new SftpClient(host, port, username, password);
            using var stream = File.OpenWrite(fileToSave);
            sftp.Connect();

            logger.LogDebug("Download file tar.gz: {fileNameTarGz} to {fileToSave}", fileNameTarGz, fileToSave);

            sftp.DownloadFile(fileNameTarGz, stream);
            sftp.Disconnect();

            //delete tar
            var cmdRmTarGz = $"rm {fileNameTarGz}";
            retCmd = sshClient.CreateCommand(cmdRmTarGz).Execute();

            logger.LogDebug("Delete tar.gz: {cmdRmTarGz}", cmdRmTarGz);
            logger.LogDebug("Result command: {retCmd}", retCmd);

            sshClient.Disconnect();

            @out.WriteLine($"Create config: {fileToSave}");
        }

        //keep
        foreach (var directoryBackupDateTime in Directory.GetDirectories(directoryWork)
                                                         .OrderByDescending(a => a)
                                                         .Skip(keep))
        {
            Delete(directoryBackupDateTime, @out);
        }
    }

    /// <summary>
    /// Delete directory
    /// </summary>
    /// <param name="directoryBackupDateTime"></param>
    /// <param name="out"></param>
    public static void Delete(string directoryBackupDateTime, TextWriter @out)
    {
        @out.WriteLine($"Delete Backup: {directoryBackupDateTime}");
        Directory.Delete(directoryBackupDateTime, true);
    }

    /// <summary>
    /// Upload file config TarGz to node
    /// </summary>
    /// <param name="hostsAndPort"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="fileTarGz"></param>
    /// <param name="loggerFactory"></param>
    public static void UploadToNode(string hostsAndPort,
                                    string username,
                                    string password,
                                    string fileTarGz,
                                    ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Application>();

        foreach (var hostAndPort in hostsAndPort.Split(','))
        {
            var (host, port) = ParseHostAndPort(hostAndPort);
            if (host == fileTarGz[..fileTarGz.IndexOf(FILE_NAME)])
            {
                var fileName = FileNameLinuxTarGz(DirectoryToDate(fileTarGz));

                logger.LogDebug("Host: {host}:{port}", host, port);

                //upload
                using var sftp = new SftpClient(host, port, username, password);
                using var stream = File.OpenRead(fileTarGz);
                sftp.Connect();

                logger.LogDebug("File upload: {fileName}", fileName);

                sftp.UploadFile(stream, fileName);
                sftp.Disconnect();

                break;
            }
        }
    }
}