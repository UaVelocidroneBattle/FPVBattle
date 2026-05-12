import { useState } from 'react';

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

export function useLanguage() {
    const [language, setLanguageState] = useState<Language>(getStoredLanguage);

    function setLanguage(lang: Language) {
        localStorage.setItem(STORAGE_KEY, lang);
        setLanguageState(lang);
    }

    return { language, setLanguage };
}
