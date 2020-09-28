/*
 * This file is part of the cv4pve-node-protect https://github.com/Corsinvest/cv4pve-node-protect,
 *
 * This source file is available under two different licenses:
 * - GNU General Public License version 3 (GPLv3)
 * - Corsinvest Enterprise License (CEL)
 * Full copyright and license information is available in
 * LICENSE.md which is distributed with this source code.
 *
 * Copyright (C) 2016 Corsinvest Srl	GPLv3 and CEL
 */

using Corsinvest.ProxmoxVE.Api.Extension.Helpers;
using Renci.SshNet;
using System;
using System.IO;
using System.Linq;

namespace Corsinvest.ProxmoxVE.NodeProtect.Api
{
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

        /// <summary>
        /// Backup
        /// </summary>
        /// <param name="hostsAndPort"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="pathsToBackup"></param>
        /// <param name="directoryWork"></param>
        /// <param name="keep"></param>
        /// <param name="debug"></param>
        /// <param name="out"></param>
        public static void Backup(string hostsAndPort,
                                  string username,
                                  string password,
                                  string[] pathsToBackup,
                                  string directoryWork,
                                  int keep,
                                  bool debug,
                                  TextWriter @out)
        {
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

            foreach (var (host, port) in ClientHelper.GetHostsAndPorts(hostsAndPort, 22, true, null))
            {
                // Execute a (SHELL) Command for download
                using var sshClient = new SshClient(host, port, username, password);
                //create tar
                sshClient.Connect();

                var cmdCreateTarGz = $"tar -cvzPf {fileNameTarGz} {string.Join(" ", pathsToBackup)}";
                var retCmd = sshClient.CreateCommand(cmdCreateTarGz).Execute();
                if (debug)
                {
                    @out.WriteLine($"Create file tar.gz: {cmdCreateTarGz}");
                    @out.WriteLine($"Result command: {retCmd}");
                }

                var fileToSave = Path.Combine(pathSave, $"{host}{FILE_NAME}");

                // download
                using var sftp = new SftpClient(host, port, username, password);
                using var stream = File.OpenWrite(fileToSave);
                sftp.Connect();
                if (debug) { @out.WriteLine($"Download file tar.gz: {fileNameTarGz} to {fileToSave}"); }
                sftp.DownloadFile(fileNameTarGz, stream);
                sftp.Disconnect();

                //delete tar
                var cmdRmTarGz = $"rm {fileNameTarGz}";
                retCmd = sshClient.CreateCommand(cmdRmTarGz).Execute();
                if (debug)
                {
                    @out.WriteLine($"Delete tar.gz: {cmdRmTarGz}");
                    @out.WriteLine($"Result command: {retCmd}");
                }
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
        /// <param name="debug"></param>
        /// <param name="out"></param>
        public static void UploadToNode(string hostsAndPort,
                                        string username,
                                        string password,
                                        string fileTarGz,
                                        bool debug,
                                        TextWriter @out)
        {
            var (host, port) = ClientHelper.GetHostsAndPorts(hostsAndPort, 22, true, null)
                                            .Where(a => a.Host == fileTarGz.Substring(0, fileTarGz.IndexOf(FILE_NAME)))
                                            .FirstOrDefault();

            var fileName = FileNameLinuxTarGz(DirectoryToDate(fileTarGz));

            //upload
            using var sftp = new SftpClient(host, port, username, password);
            using var stream = File.OpenRead(fileTarGz);
            sftp.Connect();

            if (debug)
            {
                @out.WriteLine($"Host: {host}:{port}");
                @out.WriteLine($"File upload: {fileName}");
            }

            sftp.UploadFile(stream, fileName);
            sftp.Disconnect();
        }
    }
}