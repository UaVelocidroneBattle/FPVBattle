import { create } from 'zustand';

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

export function useLanguage() {
    return useLanguageStore();
}
