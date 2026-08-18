import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { SITE_URL, resolvePageMeta } from '@/lib/siteMeta';

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

/**
 * Keeps the document title, description, canonical URL, Open Graph tags and
 * robots directive in sync with the current route.
 *
 * Called once from MainLayout: every page renders inside it, so pages don't
 * need to know about their own metadata. Route metadata lives in
 * `@/lib/siteMeta`.
 *
 * Note this runs in the browser, so it only reaches crawlers that execute
 * JavaScript (Googlebot does). Link-preview bots for Discord, Telegram and
 * Slack read the static tags in `index.html` instead.
 */
export function usePageMeta() {
    const { pathname } = useLocation();

    useEffect(() => {
        const { title, description, noIndex } = resolvePageMeta(pathname);
        const canonicalUrl = `${SITE_URL}${pathname}`;

        document.title = title;
        upsertMeta('name', 'description', description);
        upsertMeta('name', 'robots', noIndex ? 'noindex, follow' : 'index, follow');
        upsertMeta('property', 'og:title', title);
        upsertMeta('property', 'og:description', description);
        upsertMeta('property', 'og:url', canonicalUrl);
        upsertCanonical(canonicalUrl);
    }, [pathname]);
}
