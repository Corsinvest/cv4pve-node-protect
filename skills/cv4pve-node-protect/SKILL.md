---
name: cv4pve-node-protect
description: "Back up the configuration files of Proxmox VE nodes over SSH with cv4pve-node-protect, and work with its archives (find a file, extract it, compare it, prepare a restore). Use it when the user asks to back up the configuration of the nodes, to recover or compare a configuration file of a node, or to schedule that backup. It connects as root over SSH, not through the Proxmox VE API."
---

# cv4pve-node-protect

`cv4pve-node-protect backup` connects to each node over SSH, runs `tar` there and streams the archive to
the local machine. It reads the nodes and writes nothing on them. It has one command: no restore, no dry
run, no list. The connection options (hosts, user, password or key) are in a file the user names: if you
do not know its path, ask.

## Rules

- Connect only with that file, passed with `@`. Do not print it. It holds root access to every node: use
  it only for `cv4pve-node-protect backup`, never to open an SSH session or run anything else on a node.
- There is no dry run, and retention deletes the oldest backups on the local disk. Before a backup, list
  the dated folders in `--directory-work` and work out which ones `--keep` will delete. If it deletes any,
  name them and wait for the user's agreement, even when the user gave every option. If it deletes none
  and the user gave hosts, paths, folder and `--keep`, run it.
- Use the `--keep` and the folder the user gives. Never lower `--keep` on your own.
- The archives hold secrets: `/etc/shadow`, SSH keys, `/etc/pve/priv/`, the cluster database. Do not print
  those files, do not copy the archives elsewhere, and extract only into a staging folder the user agrees
  on. Delete what you extracted when you are done.
- Never write on a node. For a restore, find and extract the files, show the difference, and give the user
  the steps of https://corsinvest.github.io/cv4pve-node-protect/restore/ to run.
- Exit code 0 means every node was backed up; 1 that at least one failed: `ERROR [<host>]: …` for each,
  then `Backup failed for N of M node(s) … Retention not applied.` The other nodes are still backed up.
- With a user other than root the run ends with 0 but files are left out: read the `tar: … Permission
  denied` warnings before saying that the backup is complete.
- In Git Bash on Windows prefix the backup with `MSYS_NO_PATHCONV=1`: without it `--paths` is rewritten
  into Windows paths, the archives come out empty and the exit code is still 0. Give `tar` the archive by
  a relative path: it takes `C:` for a remote host.
- If an option is refused, check `cv4pve-node-protect backup --help`: this skill can be newer than the tool.

## Back up

The connection options go before `backup`; `--keep`, `--paths` and `--directory-work`, all required,
after it. `--paths` is one value, separated by `;`: quote it. The folder must exist.

```bash
ls <folder>                                              # dated folders: which ones --keep will delete
cv4pve-node-protect @<options-file> backup --keep=<n> \
    --paths='/etc/.;/etc/pve/.;/var/lib/pve-cluster/.' --directory-work=<folder>
```

Each run creates `<folder>/yyyy-MM-dd-HH-mm-ss/` with one `<host>-config.tar.gz` per node, the host as
written in `--host`. It prints `Create config: …` for each archive and `Delete Backup: …` for each old
folder it removes. Recommended paths: https://corsinvest.github.io/cv4pve-node-protect/what-to-back-up/

## Read an archive

```bash
tar -tzf <archive> | grep network/interfaces                      # find the exact name of an entry
mkdir -p <staging>
tar -xzf <archive> -C <staging> /etc/./network/interfaces         # one file, with the name as listed
diff -u <staging-of-an-older-run>/etc/network/interfaces <staging>/etc/network/interfaces
```

Entries keep their absolute path (`/etc/./hostname`); `tar` removes the leading `/`, so they land under the
staging folder. `/etc/hostname` inside an archive says which node it came from.

Documentation: https://corsinvest.github.io/cv4pve-node-protect/
