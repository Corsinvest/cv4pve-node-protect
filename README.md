# <img src="icon.png" alt="" height="36" align="top"> cv4pve-node-protect

```
     ______                _                      __
    / ____/___  __________(_)___ _   _____  _____/ /_
   / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
  / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
  \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Node Protect for Proxmox VE (Made in Italy)
```

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-node-protect.svg?style=flat-square)](LICENSE.md)
[![Release](https://img.shields.io/github/release/Corsinvest/cv4pve-node-protect.svg?style=flat-square)](https://github.com/Corsinvest/cv4pve-node-protect/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Corsinvest/cv4pve-node-protect/total.svg?style=flat-square&logo=download)](https://github.com/Corsinvest/cv4pve-node-protect/releases)
[![NuGet](https://img.shields.io/nuget/v/Corsinvest.ProxmoxVE.NodeProtect.Api.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Corsinvest.ProxmoxVE.NodeProtect.Api/)
[![WinGet](https://img.shields.io/winget/v/Corsinvest.cv4pve.nodeprotect?style=flat-square&logo=windows)](https://winstall.app/apps/Corsinvest.cv4pve.nodeprotect)
[![AUR](https://img.shields.io/aur/version/cv4pve-node-protect?style=flat-square&logo=archlinux)](https://aur.archlinux.org/packages/cv4pve-node-protect)

> **Configuration backup for Proxmox VE nodes** — connects to each node over SSH and saves the files you choose, from `/etc/network/interfaces` to the cluster database, in one `tar.gz` per node with automatic retention.
>
> **[Documentation](https://corsinvest.github.io/cv4pve-node-protect/)**
>
> Prefer a web interface with scheduled backups? cv4pve-node-protect also runs inside [cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin), as its [Node Protect](https://corsinvest.github.io/cv4pve-admin/modules/node-protect/) module.

---

## Why

Proxmox VE backup jobs save your VMs and containers, not the node they run on. Bridges, bonds and VLANs, `/etc/hosts`, storage definitions, the cluster configuration in `/etc/pve`, certificates, SSH keys, cron jobs and scripts live on the node. When its boot disk dies, rebuilding them by hand is slow and easy to get wrong.

cv4pve-node-protect copies those files from every node into a dated archive, on a schedule, so you can see what changed and put back exactly what was there. It **runs outside the nodes and connects over SSH** — not through the Proxmox VE API: nothing is installed or written on the nodes. It needs `root`, and the archives contain secrets: read [SSH access and security](https://corsinvest.github.io/cv4pve-node-protect/ssh-access/) first.

---

## Features

- **The whole cluster in one run** — every node in `--host`, one archive per node in the same dated folder.
- **You choose what goes in** — `/etc`, the readable `/etc/pve` files, the cluster database, crontabs, SSH keys, your scripts.
- **Nothing left on the nodes** — `tar` streams over SSH straight into your local file: no temporary files, no agent.
- **Retention** — `--keep` removes the oldest dated folders, never other folders.
- **Plain `tar.gz`** — restore with standard tools, [step by step](https://corsinvest.github.io/cv4pve-node-protect/restore/).
- **Password or SSH key**, custom port per host, IPv4, IPv6 and host names.
- **.NET library** — the engine is on NuGet as `Corsinvest.ProxmoxVE.NodeProtect.Api`.

---

## Quick start

```bash
# Windows
winget install Corsinvest.cv4pve.nodeprotect

# Linux (other platforms and packages: see the documentation)
wget https://github.com/Corsinvest/cv4pve-node-protect/releases/latest/download/cv4pve-node-protect-linux-x64.zip
unzip cv4pve-node-protect-linux-x64.zip && chmod +x cv4pve-node-protect

# Back up three nodes, keep a week
mkdir -p /srv/node-protect && chmod 700 /srv/node-protect
./cv4pve-node-protect --host=pve01,pve02,pve03 --username=root --private-key-file=/root/.ssh/id_ed25519 \
  backup --paths='/etc/.;/etc/pve/.;/var/lib/pve-cluster/.' --directory-work=/srv/node-protect --keep=7
```

`/etc/.` alone does not include `/etc/pve`: [what to back up](https://corsinvest.github.io/cv4pve-node-protect/what-to-back-up/) explains why and which paths to add.

---

## Documentation

| | |
|---|---|
| [Getting started](https://corsinvest.github.io/cv4pve-node-protect/getting-started/) | Install, first backup, hosts |
| [SSH access and security](https://corsinvest.github.io/cv4pve-node-protect/ssh-access/) | Authentication, options in a file, the account it needs, host keys, protecting the archives |
| [What to back up](https://corsinvest.github.io/cv4pve-node-protect/what-to-back-up/) | Recommended paths, `/etc/pve` and the cluster database |
| [Archives and retention](https://corsinvest.github.io/cv4pve-node-protect/archive/) | Layout, format, `--keep`, what happens when a run fails |
| [Scheduling](https://corsinvest.github.io/cv4pve-node-protect/scheduling/) | cron and Task Scheduler |
| [Restore](https://corsinvest.github.io/cv4pve-node-protect/restore/) | A single file, a reinstalled node, a lost node |
| [Options](https://corsinvest.github.io/cv4pve-node-protect/options/) | Every option, exit codes |
| [.NET library](https://corsinvest.github.io/cv4pve-node-protect/library/) | The engine in your own application |
| [Troubleshooting](https://corsinvest.github.io/cv4pve-node-protect/troubleshooting/) | Diagnostic options and common errors |

---

## Related tools

cv4pve-node-protect protects the node; [cv4pve-autosnap](https://github.com/Corsinvest/cv4pve-autosnap) takes scheduled snapshots of the guests, [cv4pve-report](https://github.com/Corsinvest/cv4pve-report) documents the whole cluster. The whole suite: [corsinvest.it/cv4pve](https://www.corsinvest.it/en/cv4pve/).

---

## Support

Professional support and consulting available through [Corsinvest](https://www.corsinvest.it/en/cv4pve/).

---

Part of [cv4pve](https://www.corsinvest.it/cv4pve) suite | Made with ❤️ in Italy by [Corsinvest](https://www.corsinvest.it)

Copyright © Corsinvest Srl
