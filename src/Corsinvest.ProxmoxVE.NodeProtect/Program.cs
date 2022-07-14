/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using System.Threading.Tasks;
using Corsinvest.ProxmoxVE.Api.Shell.Helpers;

namespace Corsinvest.ProxmoxVE.NodeProtect;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = ConsoleHelper.CreateApp("cv4pve-node-protect", "Node protect for Proxmox VE");
        _ = new Commands(app);
        return await app.ExecuteApp(args);
    }
}