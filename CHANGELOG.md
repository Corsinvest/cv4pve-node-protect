# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.2.0] - 2026-09-30

### Added
- **Documentation site**: [corsinvest.github.io/cv4pve-node-protect](https://corsinvest.github.io/cv4pve-node-protect/) with getting started, SSH access and security, connection, what to back up (and why `/etc/.` alone misses `/etc/pve`), archives and retention, scheduling, a step-by-step restore guide and the .NET library. Every page was checked against the code and a real backup ([#12](https://github.com/Corsinvest/cv4pve-node-protect/pull/12), [#16](https://github.com/Corsinvest/cv4pve-node-protect/pull/16))
- Library: `ProtectHelper` with host parsing, archive names and retention as the command line tool does them (`ParseHosts`, `ParseHostAndPort`, `GetArchiveFileName`, `IsBackupDirectoryName`, `GetBackupsToDelete`), for callers that want the same layout ([#15](https://github.com/Corsinvest/cv4pve-node-protect/pull/15))
- Unit tests (`tests/Corsinvest.ProxmoxVE.NodeProtect.Api.Tests`), run by the CI ([#15](https://github.com/Corsinvest/cv4pve-node-protect/pull/15))

### Changed
- **A node that fails no longer stops the run**: the other nodes are still backed up, each failure is printed as `ERROR [node]: …`, and the run ends with `Backup failed for N of M node(s): … Retention not applied.` and exit code 1. Retention is skipped when a node failed, so good backups are never deleted to make room for an incomplete run ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))
- **Skipped paths are reported**: a path that does not exist or cannot be read is still left out, but `tar`'s message is now printed as a warning instead of being discarded ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))
- **Archives readable only by their owner on Linux and macOS**: the dated folder is created with mode `700` and the archives with `600`, since they contain password hashes, keys and the cluster database ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))
- `--username` is now always required, also with `--private-key-file`: the error is shown before anything is created ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))
- A path containing a single quote is rejected before connecting to the node ([#15](https://github.com/Corsinvest/cv4pve-node-protect/pull/15))
- Updated Corsinvest.ProxmoxVE.Api.Extension and Api.Console from 9.1.15 to 9.2.4 ([#11](https://github.com/Corsinvest/cv4pve-node-protect/pull/11), [#18](https://github.com/Corsinvest/cv4pve-node-protect/pull/18))
- Updated SSH.NET from 2025.1.0 to 2026.0.0 (fixes GHSA-mggc-4xg6-vcxf and GHSA-q939-rpr3-3284 in `ScpClient`, which node-protect does not use) ([#11](https://github.com/Corsinvest/cv4pve-node-protect/pull/11))
- Product icon (Lucide `shield-check`), Windows executable icon and NuGet package description ([#11](https://github.com/Corsinvest/cv4pve-node-protect/pull/11))
- Project metadata, symbols (Source Link, `.snupkg`) and code style aligned with the other cv4pve tools; CI on the shared cv4pve workflow ([#10](https://github.com/Corsinvest/cv4pve-node-protect/pull/10), [#11](https://github.com/Corsinvest/cv4pve-node-protect/pull/11))
- README shortened: the details are on the documentation site ([#12](https://github.com/Corsinvest/cv4pve-node-protect/pull/12))

### Fixed
- **A path in `--paths` starting with `-` is now always a path**: `tar` gets `--` before the paths. Before, it was read as a `tar` option, so a value like `--checkpoint-action=exec=…` could run a command on the node ([#17](https://github.com/Corsinvest/cv4pve-node-protect/pull/17))
- **Restore instructions**: the README extracted archives with `tar -xvzPf … -C /root/restore`. With `-P`, `tar` ignores `-C` and writes to the absolute paths, overwriting the live files of the node. The restore guide on the documentation site extracts without `-P` into a staging folder ([#12](https://github.com/Corsinvest/cv4pve-node-protect/pull/12))
- `--debug` and `--log-level` now show the backup engine's log: the `tar` command run on each node and how long the transfer took. Before, they only added the stack trace to errors ([#13](https://github.com/Corsinvest/cv4pve-node-protect/pull/13))
- A failed run no longer leaves an empty dated folder, which counted as a backup for `--keep` ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))
- IPv6 hosts on Windows: the `:` of the address is replaced by `_` in the archive name, since Windows does not allow it in file names. Before, the run failed ([#14](https://github.com/Corsinvest/cv4pve-node-protect/pull/14))

## [2.1.1] - 2026-04-20

### Changed
- **Backup no longer fails when a listed path is missing on the node**: tar now runs with `--ignore-failed-read`, so optional paths (like `/root/scripts` or `/var/lib/ceph/.` on nodes without Ceph) are silently skipped instead of aborting the whole run

## [2.1.0] - 2026-04-17

### Added
- **IPv6 support** in `--host`: you can now target nodes by IPv6 address. Use brackets if you also specify a port: `[fe80::1]:2222`. Without a port, brackets are optional: `fe80::1`

### Changed
- **Backup no longer uses temporary files on the node**: the archive is streamed directly from the Proxmox node into your local file. Nothing is written to `/tmp` on the node, so crashes or interrupted runs don't leave leftover files behind
- Hosts, paths and SSH ports that contain unusual characters are now handled safely
- Console output is shorter and easier to read: "Create config" and "Delete Backup" show the folder name instead of the full path

### Fixed
- The NuGet package now publishes the `Corsinvest.ProxmoxVE.NodeProtect.Api` library instead of the console executable, so other projects can depend on the backup engine as a library

## [2.0.0] - 2026-04-15

### Added
- **SSH private key authentication**: log in with a private key instead of a password, using `--private-key-file` and optionally `--passphrase` if the key is encrypted

### Changed
- `--paths` and `--directory-work` are now required when running `backup`. Before, forgetting them would crash the tool with a confusing error; now you get a clear message
- The SSH port in `--host=host:port` is now actually used. Before, the port was ignored and every connection went to port 22
- Backup retention is safer: only folders with the dated backup name (like `2026-04-15-03-00-01`) can be deleted. Any other folder in your work directory is left alone
- Updated to run on .NET 10 (the library still works on .NET 8, 9 and 10)
- README rewritten to follow the same structure as the other tools in the cv4pve suite

### Removed
- **`upload` command**: it only copied the backup archive to the node without actually restoring anything, so it wasn't useful on its own. The README now explains how to restore a node manually with `scp` and `tar`, which keeps you in control during recovery

## [1.0.0] - Initial release

### Added
- Back up Proxmox VE node configuration over SSH
- Back up multiple nodes in one run using a comma-separated `--host` list
- Choose which folders to archive with `--paths`
- Keep the last N backups automatically with `--keep`
- Backups organised by date: one folder per run, one `.tar.gz` per node
- Cross-platform: Windows, Linux and macOS
