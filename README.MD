# cv4pve-node-protect

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-node-protect.svg)](LICENSE.md)

```text
Description:
     ______                _                      __
    / ____/___  __________(_)___ _   _____  _____/ /_
   / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
  / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
  \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/


  Node protect for Proxmox VE                    (Made in Italy)

  cv4pve-node-protect is a part of suite cv4pve.
  For more information visit https://www.corsinvest.it/cv4pve

Usage:
  cv4pve-node-protect [command] [options]

Options:
  --username <username>     User name
  --password <password>     The Password
  --host <host> (REQUIRED)  The host name host[:port],host1[:port],host2[:port]
  --version                 Show version information
  -?, -h, --help            Show help and usage information

Commands:
  upload  Upload configuration tar.gz to node
  backup  Backup configuration form nodes using ssh
```

## Copyright and License

Copyright: Corsinvest Srl
For licensing details please visit [LICENSE.md](LICENSE.md)

## Commercial Support

This software is part of a suite of tools called cv4pve-tools. If you want commercial support, visit the [site](https://www.corsinvest.it/cv4pve)

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

## Web GUI Version in cv4pve-admin

[![cv4pve-admin](https://raw.githubusercontent.com/Corsinvest/cv4pve-admin/main/src/Corsinvest.ProxmoxVE.Admin/wwwroot/doc/images/screenshot/modules/node-protect/grid.png)](https://github.com/Corsinvest/cv4pve-admin)

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
