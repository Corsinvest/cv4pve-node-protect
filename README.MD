# cv4pve-node-protect

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

> **Configuration backup for Proxmox VE nodes** — connects via SSH, archives the paths you choose into a single `tar.gz` per node, applies retention, done.

**Cluster-aware** — target multiple nodes in one run, one archive per node, organized by timestamp. No agents on the nodes, nothing installed, just SSH.

---

## Where cv4pve-node-protect fits

The cv4pve suite follows the Unix philosophy — each tool does one thing and does it well. `cv4pve-node-protect` is focused on **node-level configuration** (the files on the hypervisor itself), not on VMs or containers.

| Tool | Protects | How |
|---|---|---|
| [**cv4pve-node-protect**](https://github.com/Corsinvest/cv4pve-node-protect) | **Node config files** (`/etc`, `/var/lib/pve-cluster`, …) | SSH + tar.gz |
| [cv4pve-autosnap](https://github.com/Corsinvest/cv4pve-autosnap) | VM / CT snapshots | PVE API |
| [Proxmox vzdump](https://pve.proxmox.com/wiki/Backup_and_Restore) | VM / CT full backup | PVE built-in |

> Node backups protect the *hypervisor configuration*. If you lose a node, rebuilding `/etc/pve`, network config, firewall rules and cluster state from scratch is painful — this tool makes it a file copy.

---

## Quick Start

```bash
wget https://github.com/Corsinvest/cv4pve-node-protect/releases/download/VERSION/cv4pve-node-protect-linux-x64.zip
unzip cv4pve-node-protect-linux-x64.zip
./cv4pve-node-protect --host=YOUR_HOST --username=root@pam --password=YOUR_PASSWORD \
  backup --directory-work=/backup --paths="/etc/.;/var/lib/pve-cluster/." --keep=7
```

With SSH key (recommended):

```bash
./cv4pve-node-protect --host=YOUR_HOST --username=root --private-key-file=/root/.ssh/id_rsa \
  backup --directory-work=/backup --paths="/etc/.;/var/lib/pve-cluster/." --keep=7
```

---

## Response Files

Arguments can be stored in a response file and referenced with `@filename`. Useful to avoid repeating connection parameters on every run.

```text
# config.rsp
--host
192.168.1.100,192.168.1.101,192.168.1.102
--username
root
--private-key-file
/root/.ssh/id_rsa
```

```bash
cv4pve-node-protect @config.rsp backup --directory-work=/backup --paths="/etc/." --keep=7
```

- One token per line (option name and value on separate lines)
- Lines starting with `#` are comments
- Response files can be nested (a line starting with `@` references another file)

---

## Features

- **Cluster-aware** — pass a comma-separated list of hosts, one archive per node
- **Selective paths** — choose exactly what to archive with `--paths` (semicolon separated)
- **Retention** — `--keep=N` automatically prunes older backup folders
- **Multiple auth** — password, password from file, or SSH private key (with optional passphrase)
- **Custom SSH port** — per host: `host:port,host2:port` (IPv6 addresses use brackets: `[::1]:2222`)
- **IPv6 support** — IPv4, IPv6 and hostname all accepted
- **Timestamp folders** — backups organized as `YYYY-MM-DD-HH-mm-ss/{host}-config.tar.gz`
- **Agentless** — only SSH required on the target nodes, no software installed
- **Cross-platform** — Windows, Linux, macOS — native single binary

---

## Installation

| Platform | Command |
|----------|---------|
| **Linux** | `wget .../cv4pve-node-protect-linux-x64.zip && unzip cv4pve-node-protect-linux-x64.zip && chmod +x cv4pve-node-protect` |
| **Windows WinGet** | `winget install Corsinvest.cv4pve.nodeprotect` |
| **Windows manual** | Download `cv4pve-node-protect-win-x64.zip` from [Releases](https://github.com/Corsinvest/cv4pve-node-protect/releases) |
| **Arch Linux** | `yay -S cv4pve-node-protect` |
| **Debian/Ubuntu** | `sudo dpkg -i cv4pve-node-protect-VERSION-ARCH.deb` |
| **RHEL/Fedora** | `sudo rpm -i cv4pve-node-protect-VERSION-ARCH.rpm` |
| **macOS** | `wget .../cv4pve-node-protect-osx-x64.zip && unzip cv4pve-node-protect-osx-x64.zip && chmod +x cv4pve-node-protect` |

All binaries on the [Releases page](https://github.com/Corsinvest/cv4pve-node-protect/releases).

---

<details>
<summary><strong>Security &amp; Permissions</strong></summary>

### SSH Access

The tool uses **plain SSH** — it does not use the Proxmox API. Any Linux user with shell access and read permission on the target paths will work. For full `/etc` and `/var/lib/pve-cluster/` coverage, `root` (or sudo without password) is the practical choice.

### Authentication

| Method | Flags |
|--------|-------|
| **Password** | `--username=USER --password=SECRET` |
| **Password from file** | `--username=USER --password=file:/path/to/secret` |
| **SSH key** | `--username=USER --private-key-file=/path/to/id_rsa` |
| **SSH key + passphrase** | `--username=USER --private-key-file=/path/to/id_rsa --passphrase=SECRET` |

### Host Syntax

Comma-separated list of hosts. Each entry is `host[:port]`. Default port is `22`.

```
--host=host1                              # single host, default port 22
--host=host1:2222                         # single host, custom port
--host=192.168.1.100,192.168.1.101        # multiple hosts (IPv4)
--host=host1:2222,host2:2222              # multiple hosts, custom ports
--host=fe80::1                            # single IPv6 address, default port
--host=[fe80::1]:2222                     # IPv6 with custom port (brackets required)
--host=[::1]:22,pve01,192.168.1.10        # mix IPv6, hostname and IPv4
```

**IPv6 note**: when specifying a port with an IPv6 address, the address **must** be enclosed in square brackets to disambiguate the `:` used as port separator from the `:` inside the address (same convention as URLs, e.g. `http://[::1]:8080`). Without a port, brackets are optional.

</details>

---

## Backup Command

```bash
cv4pve-node-protect [global-options] backup [backup-options]
```

### Options

| Option | Description | Required |
|--------|-------------|----------|
| `--directory-work` | Local destination directory (must exist) | Yes |
| `--paths` | Remote paths to archive, `;` separated | Yes |
| `--keep` | Number of timestamped folders to retain (1–100) | Yes |

### Global Options

| Option | Description |
|--------|-------------|
| `--host` | Target host(s): `host[:port],host2[:port],…` (IPv4, IPv6 in brackets `[::1]:22`, or hostname) |
| `--username` | SSH username |
| `--password` | SSH password or `file:/path` |
| `--private-key-file` | Path to SSH private key |
| `--passphrase` | Passphrase for the private key (if any) |
| `--timeout` | SSH connection timeout in seconds |
| `--debug` | Verbose logging |

---

## Output Layout

Each run creates a new timestamped folder under `--directory-work`, containing one `tar.gz` per host:

```
/backup/
├── 2025-01-15-03-00-01/
│   ├── 192.168.1.100-config.tar.gz
│   ├── 192.168.1.101-config.tar.gz
│   └── 192.168.1.102-config.tar.gz
├── 2025-01-16-03-00-01/
│   ├── 192.168.1.100-config.tar.gz
│   └── …
```

After the backup completes, folders beyond `--keep` (oldest first) are deleted.

**Archive content** — whatever you pass in `--paths`. Common selections:

| Path | Contents |
|------|----------|
| `/etc/.` | System configuration (network, hosts, fstab, resolv.conf, …) |
| `/var/lib/pve-cluster/.` | Proxmox cluster database |
| `/etc/pve/` | Proxmox cluster config (storage, firewall, VMs, HA, SDN, …) |
| `/root/.` | Root user config (incl. `.ssh`) |
| `/var/lib/ceph/.` | Ceph configuration (if used) |

---

## Scheduling with Cron

```bash
# Daily at 3 AM, keep 7 days
0 3 * * * /usr/local/bin/cv4pve-node-protect @/etc/cv4pve/nodes.rsp \
  backup --directory-work=/backup/daily --paths="/etc/.;/var/lib/pve-cluster/." --keep=7

# Weekly on Sunday, keep 4 weeks
0 4 * * 0 /usr/local/bin/cv4pve-node-protect @/etc/cv4pve/nodes.rsp \
  backup --directory-work=/backup/weekly --paths="/etc/.;/var/lib/pve-cluster/.;/root/." --keep=4

# Monthly on the 1st, keep 12 months
0 5 1 * * /usr/local/bin/cv4pve-node-protect @/etc/cv4pve/nodes.rsp \
  backup --directory-work=/backup/monthly --paths="/etc/.;/var/lib/pve-cluster/.;/root/." --keep=12
```

---

## Restoring a Node

This tool does **not** include a restore command — restore is manual on purpose, so you stay in control during disaster recovery.

> ⚠️ **The archive contains absolute paths** (`tar -P`). Extracting it on the wrong machine can overwrite system files. **Always extract on the intended target node**, and preferably on a fresh / reinstalled node.

Recommended flow:

```bash
# 1. Copy the archive to the target node
scp /backup/2025-01-15-03-00-01/192.168.1.100-config.tar.gz root@target-node:/tmp/

# 2. Inspect the archive before extracting — make sure it's the right host
ssh root@target-node "tar -tzf /tmp/192.168.1.100-config.tar.gz | head -30"

# 3. Extract to a staging directory first, then copy what you need
ssh root@target-node "mkdir -p /root/restore && tar -xvzPf /tmp/192.168.1.100-config.tar.gz -C /root/restore"

# 4. Review, then move files into place manually (or use tar -xvzPf -C / only when confident)
```

Typical follow-up after placing files: `systemctl restart pve-cluster pveproxy pvedaemon`.

---

## Web GUI Version

[![cv4pve-admin](https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/src/Corsinvest.ProxmoxVE.Admin/wwwroot/doc/images/screenshot/modules/node-protect/grid.png)](https://github.com/Corsinvest/cv4pve-admin)

A web interface for cv4pve-node-protect is available as part of [cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin).

---

## Support

Professional support and consulting available through [Corsinvest](https://www.corsinvest.it/cv4pve).

---

Part of [cv4pve](https://www.corsinvest.it/cv4pve) suite | Made with ❤️ in Italy by [Corsinvest](https://www.corsinvest.it)

Copyright © Corsinvest Srl
