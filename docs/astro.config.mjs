// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import corsinvestTheme from '@corsinvest/cv4pve-docs-theme';

export default defineConfig({
  site: 'https://corsinvest.github.io',
  base: '/cv4pve-node-protect',
  integrations: [
    starlight({
      title: 'cv4pve-node-protect',
      description: 'Configuration backup for Proxmox VE nodes over SSH: network, storage, cluster and certificates in one archive per node.',
      // Brand, logo, GitHub and "Edit page" links, the Corsinvest sidebar group and
      // external links in a new tab come from the shared cv4pve theme.
      plugins: [
        corsinvestTheme({
          repo: 'cv4pve-node-protect',
          // Product icon: favicon and header, dark variant for the dark theme.
          icon: { light: '/icon.svg', dark: '/icon-dark.svg' },
          // Banner on the home page: the same engine runs inside cv4pve-admin.
          admin: { module: 'node-protect' },
          // Visits, without cookies.
          matomo: { url: 'https://matomo.corsinvest.it/', siteId: 11 },
          // Install-and-run panel in the home hero. The WinGet id has no dash (packaging/config).
          install: {
            targets: ['linux', 'macos', 'windows'],
            winget: 'Corsinvest.cv4pve.nodeprotect',
            run: [
              '--host=pve01,pve02,pve03',
              '--username=root --private-key-file=id_ed25519',
              "backup --paths='/etc/.;/etc/pve/.;/var/lib/pve-cluster/.'",
              '--directory-work=. --keep=7',
            ],
            output: [{ text: 'Create config: 2026-09-29-03-00-01/pve01-config.tar.gz', tone: 'ok' }],
          },
        }),
      ],
      lastUpdated: true,
      sidebar: [
        { label: 'Start here', items: ['getting-started', 'ssh-access', 'connection', 'troubleshooting'] },
        { label: 'Backup', items: ['what-to-back-up', 'archive', 'scheduling'] },
        { label: 'Restore', items: ['restore'] },
        { label: 'Reference', items: [{ label: '.NET library', slug: 'library' }] },
      ],
    }),
  ],
});
