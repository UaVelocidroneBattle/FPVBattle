import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { SITE_URL, pageAlternates, pageLanguage, resolvePageMeta, type PageAlternate } from '@/lib/siteMeta';

function upsertMeta(attribute: 'name' | 'property', key: string, content: string) {
    const selector = `meta[${attribute}="${key}"]`;
    let tag = document.head.querySelector<HTMLMetaElement>(selector);

    if (!tag) {
        tag = document.createElement('meta');
        tag.setAttribute(attribute, key);
        document.head.appendChild(tag);
    }

    tag.content = content;
}

function upsertCanonical(url: string) {
    let link = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');

    if (!link) {
        link = document.createElement('link');
        link.rel = 'canonical';
        document.head.appendChild(link);
    }

    link.href = url;
}

function replaceAlternates(alternates: PageAlternate[]) {
    document.head.querySelectorAll('link[rel="alternate"][hreflang]').forEach(link => link.remove());

    for (const { hreflang, path } of alternates) {
        const link = document.createElement('link');
        link.rel = 'alternate';
        link.hreflang = hreflang;
        link.href = `${SITE_URL}${path}`;
        document.head.appendChild(link);
    }
}

/**
 * Keeps the document title, description, canonical URL, Open Graph tags,
 * robots directive and page language in sync with the current route.
 *
 * Called once from MainLayout: every page renders inside it, so pages don't
 * need to know about their own metadata. Route metadata lives in
 * `@/lib/siteMeta`.
 *
 * Link-preview bots never run this; they read the same tags, prerendered per
 * route by `plugins/pageMetaPlugin.ts`.
 */
export function usePageMeta() {
    const { pathname } = useLocation();

    useEffect(() => {
        const { title, description, noIndex } = resolvePageMeta(pathname);
        const canonicalUrl = `${SITE_URL}${pathname}`;

        document.title = title;
        document.documentElement.lang = pageLanguage(pathname);
        upsertMeta('name', 'description', description);
        upsertMeta('name', 'robots', noIndex ? 'noindex, follow' : 'index, follow');
        upsertMeta('property', 'og:title', title);
        upsertMeta('property', 'og:description', description);
        upsertMeta('property', 'og:url', canonicalUrl);
        upsertCanonical(canonicalUrl);
        replaceAlternates(pageAlternates(pathname));
    }, [pathname]);
}
