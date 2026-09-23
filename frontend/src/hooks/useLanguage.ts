import { create } from 'zustand';
import { useLocation } from 'react-router-dom';
import { UKRAINIAN_PREFIX, pageLanguage } from '@/lib/siteMeta';

/** 'ua' is the key the guide translations use; URLs and the lang attribute use the ISO code 'uk'. */
export type Language = 'ua' | 'en';

const STORAGE_KEY = 'language';

function detectLanguage(): Language {
    const browserLang = navigator.language.toLowerCase();
    return browserLang.startsWith('uk') ? 'ua' : 'en';
}

function getStoredLanguage(): Language {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'en' || stored === 'ua' ? stored : detectLanguage();
}

interface LanguageStore {
    language: Language;
    setLanguage: (lang: Language) => void;
}

const useLanguageStore = create<LanguageStore>()((set) => ({
    language: getStoredLanguage(),
    setLanguage: (lang) => {
        localStorage.setItem(STORAGE_KEY, lang);
        set({ language: lang });
    },
}));

/** The visitor's preferred language, which decides where links into the guide lead. */
export function useLanguage() {
    return useLanguageStore();
}

/** The language a guide page is shown in, which comes from its URL so each version has an address of its own. */
export function useGuideLanguage(): Language {
    const { pathname } = useLocation();
    return pageLanguage(pathname) === 'uk' ? 'ua' : 'en';
}

export function guidePath(language: Language, page: string): string {
    return `${language === 'ua' ? UKRAINIAN_PREFIX : ''}/guide/${page}`;
}
