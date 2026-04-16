# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.1] - 2026-04-16

### Fixed
- The NuGet package now publishes the `Corsinvest.ProxmoxVE.NodeProtect.Api` library instead of the console executable, so other projects can depend on the backup engine as a library

## [2.0.0] - 2026-04-15

### Added
- **SSH private key authentication** — log in with a private key instead of a password, using `--private-key-file` and optionally `--passphrase` if the key is encrypted

### Changed
- `--paths` and `--directory-work` are now required when running `backup`. Before, forgetting them would crash the tool with a confusing error; now you get a clear message
- The SSH port in `--host=host:port` is now actually used. Before, the port was ignored and every connection went to port 22
- Backup retention is safer: only folders with the dated backup name (like `2026-04-15-03-00-01`) can be deleted. Any other folder in your work directory is left alone
- Updated to run on .NET 10 (the library still works on .NET 8, 9 and 10)
- README rewritten to follow the same structure as the other tools in the cv4pve suite

### Removed
- **`upload` command** — it only copied the backup archive to the node without actually restoring anything, so it wasn't useful on its own. The README now explains how to restore a node manually with `scp` and `tar`, which keeps you in control during recovery

## [1.0.0] - Initial release

### Added
- Back up Proxmox VE node configuration over SSH
- Back up multiple nodes in one run using a comma-separated `--host` list
- Choose which folders to archive with `--paths`
- Keep the last N backups automatically with `--keep`
- Backups organised by date: one folder per run, one `.tar.gz` per node
- Cross-platform: Windows, Linux and macOS
