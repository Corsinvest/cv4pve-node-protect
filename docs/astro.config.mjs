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
      // Brand, product icon, GitHub link, the Corsinvest sidebar group and
      // external links in a new tab come from the shared cv4pve theme.
      plugins: [
        corsinvestTheme({
          repo: 'cv4pve-node-protect',
          // Product icon: favicon and header, dark variant for the dark theme.
          icon: { light: '/icon.svg', dark: '/icon-dark.svg' },
          // Button in the home hero: the same engine runs inside cv4pve-admin.
          admin: { module: 'node-protect' },
          // Visits, without cookies.
          matomo: { url: 'https://matomo.corsinvest.it/', siteId: 11 },
          // Steps panel in the home hero: the same steps, in the same order and words, as Getting started.
          // The commands are in the pages (CliInstall; the WinGet id has no dash, see packaging/config).
          steps: {
            items: [
              'Install cv4pve-node-protect',
              { text: 'Set up SSH access', href: 'ssh-access/' },
              'Run `cv4pve-node-protect backup`',
              'Check the archive',
            ],
          },
        }),
      ],
      sidebar: [
        { label: 'Start here', items: ['getting-started', 'ssh-access', 'connection', 'ai-agents', 'troubleshooting'] },
        { label: 'Backup', items: ['what-to-back-up', 'archive', 'scheduling'] },
        { label: 'Restore', items: ['restore'] },
        { label: 'Reference', items: [{ label: '.NET library', slug: 'library' }] },
      ],
    }),
  ],
});
