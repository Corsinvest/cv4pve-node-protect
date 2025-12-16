/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Console.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using Microsoft.Extensions.Logging;

var app = ConsoleHelper.CreateApp("cv4pve-node-protect", "Node protect for Proxmox VE");
app.Options.Remove(app.GetApiTokenOption());
app.Options.Remove(app.GetValidateCertificateOption());

var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

app.GetUsernameOption().Description = "User name";
app.GetPasswordOption().Description = "The Password";

var cmdUpload = app.AddCommand("upload", "Upload configuration tar.gz to node");
var optDirectoryWorkUpload = cmdUpload.AddOption<string>("--directory-work", "Directory work")
                                      .AddValidatorExistDirectory();

cmdUpload.SetAction((action) => Application.UploadToNode(action.GetValue(app.GetHostOption())!,
                                                         action.GetValue(app.GetUsernameOption())!,
                                                         action.GetValue(app.GetPasswordOption())!,
                                                         action.GetValue(optDirectoryWorkUpload)!,
                                                         loggerFactory));

var cmdBackup = app.AddCommand("backup", "Backup configuration form nodes using ssh");
var optKeep = cmdBackup.AddOption<int>("--keep", "Specify the number which should will keep")
                       .AddValidatorRange(1, 100);
optKeep.Required = true;

var optPathBackup = cmdBackup.AddOption<string>("--paths", "Paths to backup ';' separated");
var optDirectoryWorkBackup = cmdBackup.AddOption<string>("--directory-work", "Directory work")
                                      .AddValidatorExistDirectory();

cmdBackup.SetAction((action) => Application.Backup(action.GetValue(app.GetHostOption())!,
                                                   action.GetValue(app.GetUsernameOption())!,
                                                   action.GetValue(app.GetPasswordOption())!,
                                                   action.GetValue(optPathBackup)!.Split(';'),
                                                   action.GetValue(optDirectoryWorkBackup)!,
                                                   action.GetValue(optKeep)!,
                                                   Console.Out,
                                                   loggerFactory));

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger(typeof(Program)));