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

namespace Corsinvest.ProxmoxVE.NodeProtect
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = ShellHelper.CreateConsoleApp("cv4pve-node-protect", "Node protect for Proxmox VE");
            new ShellCommands(app);
            return app.ExecuteConsoleApp(args);
        }
    }
}