import type { Plugin } from 'vite';
import { PAGE_META, SITE_URL, type PageMeta } from '../src/lib/siteMeta';

/**
 * Link-preview bots (Discord, Telegram, Slack, X, Facebook) never run
 * JavaScript, so the tags usePageMeta() sets at runtime are invisible to them.
 * This plugin writes the same tags into static HTML at build time: one
 * `<route>/index.html` per entry in PAGE_META, which the host serves before
 * falling back to the SPA rewrite. The sitemap is built from the same table,
 * so neither can drift from the routes.
 */

const MARKER = '<!-- page-meta -->';

function escapeHtml(text: string): string {
    return text
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function renderPageMeta(path: string, { title, description, noIndex }: PageMeta): string {
    const url = `${SITE_URL}${path}`;

    return [
        `<title>${escapeHtml(title)}</title>`,
        `<meta name="description" content="${escapeHtml(description)}" />`,
        `<meta name="robots" content="${noIndex ? 'noindex, follow' : 'index, follow'}" />`,
        `<link rel="canonical" href="${url}" />`,
        `<meta property="og:title" content="${escapeHtml(title)}" />`,
        `<meta property="og:description" content="${escapeHtml(description)}" />`,
        `<meta property="og:url" content="${url}" />`,
    ].join('\n    ');
}

function htmlFileFor(path: string): string {
    return path === '/' ? 'index.html' : `${path.slice(1)}/index.html`;
}

function renderSitemap(): string {
    const urls = Object.entries(PAGE_META)
        .filter(([, meta]) => !meta.noIndex && !meta.parentOnly)
        .map(([path]) => `  <url><loc>${SITE_URL}${path}</loc></url>`);

    return [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">',
        ...urls,
        '</urlset>',
        '',
    ].join('\n');
}

export function pageMetaPlugin(): Plugin {
    return {
        name: 'page-meta',

        // The dev server serves index.html for every route, so it gets the landing defaults.
        transformIndexHtml(html, { server }) {
            return server ? html.replace(MARKER, renderPageMeta('/', PAGE_META['/'])) : html;
        },

        generateBundle: {
            order: 'post',
            handler(_, bundle) {
                const template = bundle['index.html'];
                if (template?.type !== 'asset') {
                    this.error('index.html is missing from the bundle');
                }

                const html = String(template.source);

                for (const [path, meta] of Object.entries(PAGE_META)) {
                    const source = html.replace(MARKER, renderPageMeta(path, meta));

                    if (path === '/') {
                        template.source = source;
                    } else {
                        this.emitFile({ type: 'asset', fileName: htmlFileFor(path), source });
                    }
                }

                this.emitFile({ type: 'asset', fileName: 'sitemap.xml', source: renderSitemap() });
            },
        },
    };
}
