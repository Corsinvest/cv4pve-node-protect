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

---

## Quick Start

```bash
# Check available releases at: https://github.com/Corsinvest/cv4pve-node-protect/releases
# Download specific version (replace VERSION with actual version number)
wget https://github.com/Corsinvest/cv4pve-node-protect/releases/download/VERSION/cv4pve-node-protect-linux-x64.zip
unzip cv4pve-node-protect-linux-x64.zip

# Create configuration backup
./cv4pve-node-protect --host=YOUR_HOST --username=root@pam --password=YOUR_PASSWORD backup --directory-work="/backup" --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=7

# Upload configuration to node
./cv4pve-node-protect --host=YOUR_HOST --username=root@pam --password=YOUR_PASSWORD upload --file-path="config.tar.gz"
```

---

## Features

### Core Capabilities

#### **Performance & Reliability**
- **Native C#** implementation
- **Cross-platform** (Windows, Linux, macOS)
- **SSH-based** operations for direct node access
- **Cluster support** with multi-node management
- **High availability** with multiple host support

#### **Flexible Targeting**
- **Host selection** via names or IP addresses
- **Bulk operations** across multiple nodes
- **Path customization** for selective backups
- **Flexible folder** selection options

#### **Advanced Management**
- **Intelligent retention** with configurable keep policies
- **Secure upload** of configuration archives
- **Automatic compression** in tar.gz format
- **Multiple schedules** for backup frequency
- **Comprehensive logging** and status reporting

#### **Enterprise Features**
- **SSL validation options**
- **Timeout management** for operations
- **Comprehensive logging** and status reporting

---

## Installation

### Linux Installation

```bash
# Check available releases and get the specific version number
# Visit: https://github.com/Corsinvest/cv4pve-node-protect/releases

# Download specific version (replace VERSION with actual version like v1.2.3)
wget https://github.com/Corsinvest/cv4pve-node-protect/releases/download/VERSION/cv4pve-node-protect-linux-x64.zip

# Alternative: Get latest release URL programmatically
LATEST_URL=$(curl -s https://api.github.com/repos/Corsinvest/cv4pve-node-protect/releases/latest | grep browser_download_url | grep linux-x64 | cut -d '"' -f 4)
wget "$LATEST_URL"

# Extract and make executable
unzip cv4pve-node-protect-linux-x64.zip
chmod +x cv4pve-node-protect

# Optional: Move to system path
sudo mv cv4pve-node-protect /usr/local/bin/
```

### Windows Installation

```powershell
# Check available releases at: https://github.com/Corsinvest/cv4pve-node-protect/releases
# Download specific version (replace VERSION with the actual number)
Invoke-WebRequest -Uri "https://github.com/Corsinvest/cv4pve-node-protect/releases/download/VERSION/cv4pve-node-protect-win-x64.zip" -OutFile "cv4pve-node-protect.zip"

# Extract
Expand-Archive cv4pve-node-protect.zip -DestinationPath "C:\Tools\cv4pve-node-protect"

# Add to PATH (optional)
$env:PATH += ";C:\Tools\cv4pve-node-protect"
```

### macOS Installation

```bash
# Check available releases at: https://github.com/Corsinvest/cv4pve-node-protect/releases
# Download specific version (replace VERSION with the actual number)
wget https://github.com/Corsinvest/cv4pve-node-protect/releases/download/VERSION/cv4pve-node-protect-osx-x64.zip
unzip cv4pve-node-protect-osx-x64.zip
chmod +x cv4pve-node-protect

# Move to applications
sudo mv cv4pve-node-protect /usr/local/bin/
```

---

## Configuration

### Authentication Methods

#### **Username/Password**
```bash
cv4pve-node-protect --host=192.168.1.100 --username=backup@pve --password=your_password [commands]
```

#### **Password from File**
```bash
# Use interactive password prompt that stores to file
cv4pve-node-protect --host=192.168.1.100 --username=backup@pve --password=file:/etc/cv4pve/password [commands]

# First run: prompts for password and saves to file
# Subsequent runs: reads password from file automatically
```

### Configurable Backup Paths

The `--paths` parameter supports selection of specific paths for backup:

| Path | Description | Example |
|------|-------------|---------|
| **System Configuration** | Essential system config files | `--paths="/etc/."` |
| **Proxmox Cluster** | Proxmox VE cluster data | `--paths="/var/lib/pve-cluster/."` |
| **Root Home** | Root user configuration files | `--paths="/root/."` |
| **Ceph Configuration** | Ceph storage configuration | `--paths="/var/lib/ceph/."` |
| **Multiple Paths** | Comma-separated list | `--paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/."` |

### Path Examples

<details>
<summary><strong>Real-World Path Examples</strong></summary>

#### Common Configuration Paths
```bash
# Network configuration
--paths="/etc/network/interfaces"

# System configuration
--paths="/etc/fstab"
--paths="/etc/hosts"
--paths="/etc/resolv.conf"

# Proxmox VE configuration
--paths="/etc/pve/"
--paths="/etc/pve/storage.cfg"
--paths="/etc/pve/cluster.conf"
--paths="/etc/pve/priv/"
--paths="/etc/pve/nodes/"

# Firewall configuration
--paths="/etc/pve/firewall/"
--paths="/etc/pve/lxc/"
--paths="/etc/pve/qemu-server/"

# Service configuration
--paths="/etc/cron.d/"
--paths="/etc/logrotate.d/"
```

</details>

---

## Usage Examples

### Basic Backup Operations

<details>
<summary><strong>Create Backups</strong></summary>

#### Single node backup
```bash
cv4pve-node-protect --host=pve1.domain.com --username=root@pam --password=secret backup --directory-work="/backup" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/."
```

#### Backup with retention
```bash
cv4pve-node-protect --host=pve1.domain.com --username=backup@pve --password=secret backup --directory-work="/backup" --paths="/etc/." --keep=10
```

#### Multiple nodes backup
```bash
cv4pve-node-protect --host=192.168.1.100,192.168.1.101,192.168.1.102 --username=backup@pve --password=secret backup --directory-work="/backup" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/."
```

</details>

<details>
<summary><strong>Configuration Upload</strong></summary>

#### Upload configuration to node
```bash
cv4pve-node-protect --host=pve1.domain.com --username=root@pam --password=secret upload --file-path="config.tar.gz"
```

#### Upload to multiple nodes
```bash
cv4pve-node-protect --host=192.168.1.100,192.168.1.101 --username=backup@pve --password=secret upload --file-path="config.tar.gz"
```

</details>

<details>
<summary><strong>Verify Operations</strong></summary>

#### Test connection
```bash
cv4pve-node-protect --host=pve1.domain.com --username=root@pam --password=secret backup --directory-work="/tmp" --paths="/etc/." --dry-run
```

#### Dry run mode
```bash
# Test without making changes
cv4pve-node-protect --host=pve1.domain.com --username=backup@pve --password=secret backup --directory-work="/tmp" --paths="/etc/." --dry-run
```

</details>

### Advanced Targeting Examples

<details>
<summary><strong>Production Environment Patterns</strong></summary>

```bash
# Critical nodes backup
cv4pve-node-protect --host=cluster.company.com --username=backup@pve --password=secret --host=192.168.1.100,192.168.1.101 backup --directory-work="/backup/critical" --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=14

# Disaster recovery backup
cv4pve-node-protect --host=pve.company.com --username=backup@pve --password=secret --host="192.168.1.100:8006,192.168.1.101:8006" backup --directory-work="/backup/dr" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/." --keep=30

# Weekly retention backup
cv4pve-node-protect --host=pve-cluster.local --username=ops@pve --password=secret --host=192.168.1.100 backup --directory-work="/backup/weekly" --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=7
```

</details>

### Scheduling with Cron

<details>
<summary><strong>Cron Examples</strong></summary>

```bash
# Edit crontab
crontab -e

# Daily backup at 3 AM (keep 7 days)
0 3 * * * /usr/local/bin/cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret --host=192.168.1.100 backup --directory-work="/backup/daily" --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=7

# Weekly backup on Sunday at 4 AM (keep 4 weeks)
0 4 * * 0 /usr/local/bin/cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret --host=192.168.1.100,192.168.1.101,192.168.1.102 backup --directory-work="/backup/weekly" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/." --keep=4

# Monthly backup on the 1st day of month at 5 AM
0 5 1 * * /usr/local/bin/cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret --host=192.168.1.100 backup --directory-work="/backup/monthly" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/." --paths="/var/lib/ceph/." --keep=12
```

</details>

---

## Security & Permissions

### Required Permissions

| Permission | Purpose | Scope |
|------------|---------|-------|
| **Sys.Modify** | Modify system configuration | Nodes |
| **Datastore.Read** | Read data from datastore | Storage systems |
| **Sys.Audit** | View system information | Nodes |
| **VM.Audit** | Read VM information | Virtual machines (for some cluster data) |

### Authentication Setup

<details>
<summary><strong>Setting up Authentication</strong></summary>

#### 1. Configure User Permissions
```bash
# Follow Proxmox VE official documentation for:
# - User creation with appropriate privileges
# - Permission assignment for required roles
# - Required permissions: Sys.Modify, Datastore.Read, Sys.Audit, VM.Audit
# Refer to official Proxmox VE documentation for detailed steps
```

#### 2. Use Credentials in Commands
```bash
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret backup --directory-work="/backup" --paths="/etc/."
```

</details>

---

## Advanced Features

### Selective Backup

<details>
<summary><strong>Custom Path Selection</strong></summary>

cv4pve-node-protect allows selecting specific paths for backup:

```bash
# Set custom paths
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret --host=192.168.1.100 backup --directory-work="/backup/custom" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/.ssh" --paths="/etc/pve/firewall/"
```

#### Example of Custom Backup Paths
```
----------------------------------------------------------------------------
| Path                      | Type    | Description                          |
----------------------------------------------------------------------------
| /etc/.                   | Config  | General system configuration         |
| /var/lib/pve-cluster/.   | Storage | Proxmox VE cluster data              |
| /root/.                  | Home    | Personal and root configuration      |
| /etc/pve/.               | Config  | Complete Proxmox VE configuration    |
| /etc/pve/firewall/.      | Config  | Specific firewall rules              |
| /etc/pve/lxc/.           | Config  | LXC container templates              |
| /etc/pve/qemu-server/.   | Config  | QEMU VM configuration                |
----------------------------------------------------------------------------
```

</details>

### Advanced Backup Management

<details>
<summary><strong>Retention and Naming Policies</strong></summary>

Advanced backup retention policy management:

```bash
# Set custom retention
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret --host=192.168.1.100 backup --directory-work="/backup" --paths="/etc/." --keep=30
```

#### Naming Policies

The backup directory contains files with format: `{ip_address}-config.tar.gz`

The format is fixed: {ip_address}-config.tar.gz where {ip_address} is the IP address of the Proxmox VE node.

#### Directory Organization

Backups are organized by date in a hierarchical structure:
```
/backup/
├── 2025-01-15-14-30-22/
│   ├── 192.168.1.100-config.tar.gz
│   ├── 192.168.1.101-config.tar.gz
│   └── 192.168.1.102-config.tar.gz
├── 2025-01-16-14-30-22/
│   ├── 192.168.1.100-config.tar.gz
│   ├── 192.168.1.101-config.tar.gz
│   └── 192.168.1.102-config.tar.gz
```

</details>

### Configuration Files

<details>
<summary><strong>Parameter Files for Complex Setup</strong></summary>

Use parameter files for complex configurations:

#### Create Parameter File
```bash
# /etc/cv4pve/production.conf
--host=cluster.company.com
--username=backup@pve
--password-file=/etc/cv4pve/password
--host=192.168.1.100:8006,192.168.1.101:8006
--directory-work=/backup/production
--timeout=300
--validate-certificate
```

#### Execute with Parameter File
```bash
cv4pve-node-protect @/etc/cv4pve/production.conf backup --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=30
```

#### Multiple Environment Configurations
```bash
# Development environment
cv4pve-node-protect @/etc/cv4pve/dev.conf backup --paths="/etc/." --keep=3

# Staging environment
cv4pve-node-protect @/etc/cv4pve/staging.conf backup --paths="/etc/." --paths="/var/lib/pve-cluster/." --keep=5

# Production environment
cv4pve-node-protect @/etc/cv4pve/production.conf backup --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/." --keep=30
```

</details>

---

## Best Practices

### Backup Strategy Guidelines

#### **Retention Schedules**
- **Daily**: Keep 7-14 backups (1-2 weeks)
- **Weekly**: Keep 4-8 backups (1-2 months)
- **Monthly**: Keep 12 backups (1 year)
- **Yearly**: Keep 3-5 backups (long-term archival)

#### **Directory Organization**
- Use descriptive paths (`/backup/daily`, `/backup/weekly`)
- Include environment in path (`/backup/prod-daily`, `/backup/dev-weekly`)
- Consider compliance requirements (`/backup/retention-7y`)

#### **Performance Optimization**
- **Schedule during low activity** periods
- **Stagger backup times** across nodes
- **Monitor storage growth** regularly
- **Use selective paths** to reduce backup size
- **Implement cleanup routines** for old backups

#### **Reliability Measures**
- **Test restore procedures** regularly
- **Monitor backup creation** for failures
- **Implement alerting** for storage thresholds
- **Document recovery procedures**
- **Verify SSH access** to nodes

---

## Troubleshooting

### Common Issues & Solutions

<details>
<summary><strong>Authentication Problems</strong></summary>

#### Issue: "Authentication failed"
```bash
# Check credentials
cv4pve-node-protect --host=pve.local --username=root@pam --password=test backup --directory-work="/tmp" --paths="/etc/."
```

#### Solution: Verify permissions
```bash
# Check user permissions in Proxmox
pveum user list
pveum user permissions backup@pve
```

</details>

<details>
<summary><strong>Connection Issues</strong></summary>

#### Issue: "Connection timeout" or "Host unreachable"
```bash
# Test connectivity
ping pve.local
telnet pve.local 8006

# Check SSL certificate
cv4pve-node-protect --host=pve.local --validate-certificate --username=root@pam --password=secret backup --directory-work="/tmp" --paths="/etc/."
```

#### Solution: Network and certificate verification
```bash
# Use IP instead of hostname
cv4pve-node-protect --host=192.168.1.100 backup --directory-work="/tmp" --paths="/etc/."

# Disable SSL validation for testing (not recommended for production)
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret backup --directory-work="/tmp" --paths="/etc/."
```

</details>

<details>
<summary><strong>Disk Space Issues</strong></summary>

#### Issue: "Insufficient disk space"
```bash
# Check disk space status
df -h
ls -la /backup/
```

#### Solution: Disk space management
```bash
# Manually delete old backups
rm -rf /backup/*old*/
```

</details>

<details>
<summary><strong>Path Selection Issues</strong></summary>

#### Issue: "Cannot access specified paths"
```bash
# Check paths
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret backup --directory-work="/tmp" --paths="/nonexistent/path/"
```

#### Solution: Verify existing paths
```bash
# Verify SSH access and paths
ssh root@pve.local "ls -la /etc/."
ssh root@pve.local "ls -la /var/lib/pve-cluster/."
```

</details>

### Debug Mode

<details>
<summary><strong>Enable Detailed Logging</strong></summary>

```bash
# Enable debug output
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret backup --directory-work="/tmp" --paths="/etc/." --debug

# Test without making changes
cv4pve-node-protect --host=pve.local --username=backup@pve --password=secret backup --directory-work="/tmp" --paths="/etc/." --dry-run
```

</details>

---

#### **Web GUI Version**

[![cv4pve-admin](https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/src/Corsinvest.ProxmoxVE.Admin/wwwroot/doc/images/screenshot/modules/node-protect/grid.png)](https://github.com/Corsinvest/cv4pve-admin)

**Web interface for cv4pve-node-protect**

---

## SSH Integration

### Why SSH Access Matters

> **Important:** cv4pve-node-protect uses SSH to access Proxmox VE nodes and retrieve configuration files. SSH must be enabled and accessible on all target nodes.

#### **Without SSH Access**
- Cannot retrieve configuration files
- Backup operations fail
- Unable to access specified paths
- Node protection impossible

#### **With SSH Access**
- Secure file transfer
- Reliable backup operations
- Automated scheduled backups
- Centralized configuration management

### Verify SSH Access

```bash
# Test SSH connectivity
ssh root@pve-node "ls -la /etc/."

# Verify with dry run
cv4pve-node-protect --host=pve-node --username=root@pam --password=password backup --directory-work="/tmp" --paths="/etc/." --dry-run
```

---

## Command Reference

### Global Options

<details>
<summary><strong>Complete Parameter List</strong></summary>

```bash
cv4pve-node-protect [global-options] [command] [command-options]
```

#### Authentication Options
| Parameter | Description | Example |
|-----------|-------------|---------|
| `--host` | Proxmox host(s) | `--host=pve.local:8006` |
| `--username` | Username@realm | `--username=root@pam` |
| `--password` | Password or file | `--password=secret` or `--password=file:/path` |

#### Connection Options
| Parameter | Description | Default |
|-----------|-------------|---------|
| `--timeout` | Operation timeout (seconds) | `30` |
| `--validate-certificate` | Validate SSL certificate | `false` |

#### Target Selection
| Parameter | Description | Example |
|-----------|-------------|---------|
| `--host` | Target node(s) | `--host=192.168.1.100:8006,192.168.1.101:8006` |

#### Storage & Format
| Parameter | Description | Default |
|-----------|-------------|---------|
| `--directory-work` | Backup destination directory | Required |

</details>

### Backup Command

<details>
<summary><strong>Configuration Backup Options</strong></summary>

```bash
cv4pve-node-protect [global-options] backup [backup-options]
```

#### Backup-Specific Options
| Parameter | Description | Required |
|-----------|-------------|----------|
| `--paths` | Configuration paths to backup | Yes |
| `--directory-work` | Backup destination directory | Yes |
| `--keep` | Retention count (backups to preserve) | No |
| `--debug` | Enable debug output | No |

#### Examples
```bash
# Basic backup with retention
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret backup --directory-work="/backup" --paths="/etc/." --keep=7

# Multiple paths backup
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret backup --directory-work="/backup" --paths="/etc/." --paths="/var/lib/pve-cluster/." --paths="/root/."

# Debug mode backup
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret backup --directory-work="/backup" --paths="/etc/." --debug
```

</details>

### Upload Command

<details>
<summary><strong>Configuration Upload Options</strong></summary>

```bash
cv4pve-node-protect [global-options] upload [upload-options]
```

#### Upload-Specific Options
| Parameter | Description | Required |
|-----------|-------------|----------|
| `--file-path` | Configuration archive to upload | Yes |
| `--debug` | Enable debug output | No |

#### Examples
```bash
# Upload configuration archive
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret upload --file-path="config.tar.gz"

# Upload with debug output
cv4pve-node-protect --host=pve.local --username=root@pam --password=secret upload --file-path="config.tar.gz" --debug
```

</details>

---

## Support

Professional support and consulting available through [Corsinvest](https://www.corsinvest.it/cv4pve).

---

Part of [cv4pve](https://www.corsinvest.it/cv4pve) suite | Made with ❤️ in Italy by [Corsinvest](https://www.corsinvest.it)

Copyright © Corsinvest Srl
