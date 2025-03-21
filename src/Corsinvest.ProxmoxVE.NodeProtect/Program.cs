/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.CommandLine;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using Microsoft.Extensions.Logging;

var app = ConsoleHelper.CreateApp("cv4pve-node-protect", "Node protect for Proxmox VE");
var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

app.GetApiTokenOption().IsHidden = true;
app.GetValidateCertificateOption().IsHidden = true;
app.GetUsernameOption().Description = "User name";
app.GetPasswordOption().Description = "The Password";

// upload
var cmdUpload = app.AddCommand("upload", "Upload configuration tar.gz to node");
var optDirectoryWorkUpload = cmdUpload.AddOption<string>("--directory-work", "Directory work")
                                      .AddValidatorExistDirectory();

cmdUpload.SetHandler((host, username, password, directoryWork)
                        => Application.UploadToNode(host, username, password, directoryWork, loggerFactory),
                      app.GetHostOption(),
                      app.GetUsernameOption(),
                      app.GetPasswordOption(),
                      optDirectoryWorkUpload);

// backup
var cmdBackup = app.AddCommand("backup", "Backup configuration form nodes using ssh");
var optKeep = cmdBackup.AddOption<int>("--keep", "Specify the number which should will keep")
                       .AddValidatorRange(1, 100);
optKeep.IsRequired = true;

var optPathBackup = cmdBackup.AddOption<string>("--paths", "Paths to backup ';' separated");
var optDirectoryWorkBackup = cmdBackup.AddOption<string>("--directory-work", "Directory work")
                                      .AddValidatorExistDirectory();

cmdBackup.SetHandler((host, username, password, pathBackup, directoryWork, keep)
                        => Application.Backup(host,
                                              username,
                                              password,
                                              pathBackup.Split(';'),
                                              directoryWork,
                                              keep,
                                              Console.Out,
                                              loggerFactory),
                      app.GetHostOption(),
                      app.GetUsernameOption(),
                      app.GetPasswordOption(),
                      optPathBackup,
                      optDirectoryWorkBackup,
                      optKeep);

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger(typeof(Program)));