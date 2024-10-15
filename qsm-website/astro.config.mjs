import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	site: 'https://lines-of-codes.github.io',
	base: 'QSMSharp',
	integrations: [
		starlight({
			title: 'QSM',
			social: {
				github: 'https://github.com/lines-of-codes/QSMSharp',
			},
			sidebar: [
				{
					label: 'Guides',
					items: [
						{ label: 'Introduction', slug: 'guides/introduction' },
						{ label: 'Create new server', slug: 'guides/create-new-server' },
						{ label: 'Choosing a software', slug: 'guides/choosing-a-software' }
					],
				},
				{
					label: 'Reference',
					autogenerate: { directory: 'reference' },
				},
			],
		}),
	],
});
