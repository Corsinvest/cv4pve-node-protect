/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.CommandLine;
using Corsinvest.ProxmoxVE.Api.Console.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using Microsoft.Extensions.Logging;

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
    Description = "The host name host[:port],host1[:port],host2[:port]",
    Required = true,
    CustomParser = (e) => e.Tokens.Single().Value
};

app.Add(optHost);

var optTimeout = app.AddOption<int?>("--timeout", "Timeout in seconds for ssh connection");

var loggerFactory = ConsoleHelper.CreateLoggerFactory<Program>(app.GetLogLevelFromDebug());

var cmdBackup = app.AddCommand("backup", "Backup configuration from nodes using ssh");
var optKeep = cmdBackup.AddOption<int>("--keep", "Specify the number of backups to retain")
                       .AddValidatorRange(1, 100);
optKeep.Required = true;

var optPathBackup = cmdBackup.AddOption<string>("--paths", "Paths to backup ';' separated");
optPathBackup.Required = true;

var optDirectoryWorkBackup = cmdBackup.AddOption<string>("--directory-work", "Directory work")
                                      .AddValidatorExistDirectory();
optDirectoryWorkBackup.Required = true;

cmdBackup.SetAction(async (action) => await GetEngine(action)
                                             .BackupAsync(action.GetValue(optPathBackup)!.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                                                          action.GetValue(optDirectoryWorkBackup)!,
                                                          action.GetValue(optKeep)!,
                                                          Console.Out,
                                                          loggerFactory));

ProtectEngine GetEngine(ParseResult action)
{
    var username = action.GetValue(optUsername);
    var password = action.GetValue(optPassword);
    var privateKeyFile = action.GetValue(optPrivateKeyFile);

    if (string.IsNullOrEmpty(privateKeyFile) && string.IsNullOrEmpty(username))
    {
        throw new InvalidOperationException("Option '--username' or '--private-key-file' is required!");
    }

    if (!string.IsNullOrEmpty(username) && string.IsNullOrEmpty(privateKeyFile) && string.IsNullOrEmpty(password))
    {
        throw new InvalidOperationException("Option '--password' is required when using '--username' without a private key!");
    }

    // Support '--password=file:/path/to/file'
    if (!string.IsNullOrEmpty(password) && password.StartsWith("file:", StringComparison.Ordinal))
    {
        var path = password["file:".Length..];
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Password file not found: {path}", path);
        }
        password = File.ReadAllText(path).Trim();
    }

    return new ProtectEngine(action.GetValue(optHost)!,
                             username!,
                             password!,
                             action.GetValue(optPassphrase)!,
                             privateKeyFile!,
                             action.GetValue(optTimeout));
}

return await app.ExecuteAppAsync(args, loggerFactory.CreateLogger<Program>());
