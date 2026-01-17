import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// https://astro.build/config
export default defineConfig({
    site: "https://lines-of-codes.github.io",
    base: "QSMSharp",
    integrations: [
        starlight({
            title: "QSM",
            customCss: ["./src/styles/custom.css"],
            social: [
                {
                    icon: "github",
                    label: "GitHub",
                    href: "https://github.com/lines-of-codes/QSMSharp",
                },
            ],
            defaultLocale: "root",
            locales: {
                root: {
                    label: "English",
                    lang: "en",
                },
                th: {
                    label: "ไทย",
                },
            },
            sidebar: [
                {
                    label: "Common Guides",
                    translations: {
                        th: "ไกด์ทั่วไป",
                    },
                    items: [
                        {
                            slug: "common/guides/introduction",
                        },
                        {
                            slug: "common/guides/choosing-a-software",
                        },
                    ],
                },
                {
                    label: "QSM.Windows Guides",
                    translations: {
                        th: "ไกด์ QSM.Windows",
                    },
                    items: [
                        {
                            slug: "qsmwin/guides/introduction",
                        },
                        {
                            slug: "qsmwin/guides/create-new-server",
                        },
                    ],
                },
                {
                    label: "QSM.Windows Reference",
                    translations: {
                        th: "เอกสารอ้างอิง QSM.Windows",
                    },
                    collapsed: true,
                    autogenerate: { directory: "QSMWin/reference" },
                },
                {
                    label: "QSM.Web Guides",
                    translations: {
                        th: "ไกด์ QSM.Web",
                    },
                    items: [
                        {
                            slug: "qsmweb/guides/introduction",
                        },
                    ],
                },
                {
                    label: "QSM.Web Reference",
                    translations: {
                        th: "เอกสารอ้างอิง QSM.Web",
                    },
                    collapsed: true,
                    autogenerate: { directory: "QSMWeb/reference" },
                },
            ],
        }),
    ],
});
