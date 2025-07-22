import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	site: 'https://lines-of-codes.github.io',
	base: 'QSMSharp',
	integrations: [
		starlight({
			title: 'QSM',
			social: [
				{ icon: 'github', label: 'GitHub', href: "https://github.com/lines-of-codes/QSMSharp" },
			],
			sidebar: [
				{
					label: 'Common Guides',
					items: [
						{ label: 'Introduction', slug: 'common/guides/introduction' },
						{ label: 'Choosing a software', slug: 'common/guides/choosing-a-software' }
					],
				},
                {
                    label: 'QSM.Win Guides',
                    items: [
                        { label: 'Introduction', slug: 'qsmwin/guides/introduction'  },
                        { label: 'Create a server', slug: 'qsmwin/guides/create-new-server'  }
                    ],
                },
				{
					label: 'QSM.Win Reference',
                    collapsed: true,
					autogenerate: { directory: 'QSMWin/reference' },
				},
                {
                    label: 'QSM.Web Guides',
                    items: [
                        { label: 'Introduction', slug: 'qsmweb/guides/introduction'  }
                    ],
                },
				{
					label: 'QSM.Web Reference',
                    collapsed: true,
					autogenerate: { directory: 'QSMWeb/reference' },
				},

			],
		}),
	],
});
