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

using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect.Api;
using McMaster.Extensions.CommandLineUtils;

namespace Corsinvest.ProxmoxVE.NodeProtect
{
    public class ShellCommands
    {
        /// <summary>
        /// Shell command for cli.
        /// </summary>
        /// <param name="parent"></param>
        public ShellCommands(CommandLineApplication parent)
        {
            parent.GetUsername().Description = "User name";
            parent.GetPassword().Description = "The Password";

            Backup(parent);
            Upload(parent);
        }

        private void Upload(CommandLineApplication parent)
        {
            parent.Command("upload", cmd =>
            {
                cmd.Description = "Upload configuration tar.gz to node";
                cmd.AddFullNameLogo();

                var optDirectoryWork = cmd.Option("--directory-work", "Directory work", CommandOptionType.SingleValue)
                                          .Accepts(a => a.ExistingDirectory());

                cmd.OnExecute(() =>
                {
                    Application.UploadToNode(parent.GetHost().Value(),
                                             parent.GetUsername().Value(),
                                             parent.GetPassword().Value(),
                                             optDirectoryWork.Value(),
                                             parent.DebugIsActive(),
                                             parent.Out);             
                });
            });
        }

        private void Backup(CommandLineApplication parent)
        {
            parent.Command("backup", cmd =>
            {
                cmd.Description = "Backup configuration form nodes using ssh";
                cmd.AddFullNameLogo();

                var optKeep = cmd.KeepOption().IsRequired();
                var optPathBackup = cmd.Option("--paths", "Paths to backup ';' separated", CommandOptionType.MultipleValue);
                var optDirectoryWork = cmd.Option("--directory-work", "Directory work", CommandOptionType.SingleValue)
                                          .Accepts(a => a.ExistingDirectory());

                cmd.OnExecute(() =>
                {
                    Application.Backup(parent.GetHost().Value(),
                                       parent.GetUsername().Value(),
                                       parent.GetPassword().Value(),
                                       optPathBackup.Values.ToArray(),
                                       optDirectoryWork.Value(),
                                       optKeep.ParsedValue,
                                       parent.DebugIsActive(),
                                       parent.Out);
                });
            });
        }
    }
}