/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System;
using System.CommandLine;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using Microsoft.Extensions.Logging;

namespace Corsinvest.ProxmoxVE.NodeProtect;

public class Commands
{
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Shell command for cli.
    /// </summary>
    /// <param name="command"></param>
    public Commands(RootCommand command)
    {
        command.GetApiToken().IsHidden = true;

        _loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(command.GetLogLevelFromDebug());

        command.GetUsername().Description = "User name";
        command.GetPassword().Description = "The Password";

        Backup(command);
        Upload(command);
    }

    private void Upload(RootCommand command)
    {
        var cmd = command.AddCommand("upload", "Upload configuration tar.gz to node");
        var optDirectoryWork = cmd.AddOption("--directory-work", "Directory work").AddValidatorExistDirectory();

        cmd.SetHandler(() =>
        {
            Application.UploadToNode(command.GetHost().GetValue(),
                                     command.GetUsername().GetValue(),
                                     command.GetPassword().GetValue(),
                                     optDirectoryWork.GetValue(),
                                     _loggerFactory);
        });
    }

    private void Backup(RootCommand command)
    {
        var cmd = command.AddCommand("backup", "Backup configuration form nodes using ssh");

        var optKeep = cmd.AddOption<int>("--keep", "Specify the number which should will keep")
                         .AddValidatorRange(1, 100);
        optKeep.IsRequired = true;

        var optPathBackup = cmd.AddOption("--paths", "Paths to backup ';' separated");
        var optDirectoryWork = cmd.AddOption("--directory-work", "Directory work").AddValidatorExistDirectory();

        cmd.SetHandler(() =>
        {
            Application.Backup(command.GetHost().GetValue(),
                               command.GetUsername().GetValue(),
                               command.GetPassword().GetValue(),
                               optPathBackup.GetValue().Split(';'),
                               optDirectoryWork.GetValue(),
                               optKeep.GetValue(),
                               Console.Out,
                               _loggerFactory);
        });
    }
}