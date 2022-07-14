# cv4pve-node-protect

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-node-protect.svg)](LICENSE.md)

```text
    ______                _                      __
   / ____/___  __________(_)___ _   _____  _____/ /_
  / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
 / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
 \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Node protect for Proxmox VE         (Made in Italy)

Usage: cv4pve-node-protect [options]

Options:
  -?|-h|--help      Show help information
  --version         Show version information
  --host            The host name host[:port],host1[:port],host2[:port]
  --username        User name
  --password        The password

Commands:
  app-check-update  Check update application
  app-upgrade       Upgrade application
  backup            Backup configuration form nodes using ssh
  upload            Upload configuration tar.gz to node

Run 'cv4pve-node-protect [command] --help' for more information about a command.

cv4pve-node-protect is a part of suite cv4pve-tools.
For more information visit https://www.cv4pve-tools.com
```

## Copyright and License

Copyright: Corsinvest Srl
For licensing details please visit [LICENSE.md](LICENSE.md)

## Commercial Support

This software is part of a suite of tools called cv4pve-tools. If you want commercial support, visit the [site](https://www.cv4pve-tools.com)

## Introduction

Node protect for Proxmox VE.

this software create backup tar,gz file from any nodes Proxmox VE and download in local work directory.
The idea are download configuration file from nodes Proxmox VE.

## Main features

* Completely written in C#
* Use native api REST Proxmox VE (library C#)
* Independent os (Windows, Linux, Macosx)
* Installation unzip file extract binary
* Not require installation in Proxmox VE
* Execute out side Proxmox VE
* Connect SSH

## Configuration

E.g. install on linux 64

Download last package e.g. Debian cv4pve-node-protect-linux-x64.zip, on your os and install:

```sh
root@debian:~# unzip cv4pve-node-protect-linux-x64.zip
```

Backup

```sh
root@debian:~# cv4pve-node-protect --host=192.168.0.100,192.168.0.101 --username=root@pam --password=fagiano backup --keep 10 --directory-work "/mnt/data" --paths "/etc/." --paths "/var/lib/pve-cluster/." --paths "/root/." --paths "/var/lib/ceph/."
```

List file download

```sh
root@debian:~# ll /mnt/data
drwxrwxrwx 1 frank frank 512 Apr 16 18:38  2020-04-16-18-38-36

root@debian:~# ll /mnt/data/2020-04-16-18-38-36
-rwxrwxrwx 1 frank frank 783K Apr 16 18:38 192.168.0.100-config.tar.gz
-rwxrwxrwx 1 frank frank 783K Apr 16 18:38 192.168.0.101-config.tar.gz
```
