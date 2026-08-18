import { useEffect, useState } from 'react';

const DESKTOP_QUERY = '(min-width: 1024px)';

export function useIsDesktop() {
    const [isDesktop, setIsDesktop] = useState(() => window.matchMedia(DESKTOP_QUERY).matches);

    useEffect(() => {
        const mql = window.matchMedia(DESKTOP_QUERY);
        const handleChange = () => setIsDesktop(mql.matches);
        mql.addEventListener('change', handleChange);
        return () => mql.removeEventListener('change', handleChange);
    }, []);

    return isDesktop;
}
