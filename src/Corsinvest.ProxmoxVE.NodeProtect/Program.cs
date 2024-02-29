/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: 2019 Copyright Corsinvest Srl
 */

using Corsinvest.ProxmoxVE.Api.Shell.Helpers;
using Corsinvest.ProxmoxVE.NodeProtect;

var app = ConsoleHelper.CreateApp("cv4pve-node-protect", "Node protect for Proxmox VE");
_ = new Commands(app);
return await app.ExecuteApp(args);
