import { useEffect } from 'react';
import { useShallow } from 'zustand/shallow';
import { useLandingStore } from '@/store/landingStore';
import { Spinner } from '@/components/ui/spinner';
import { Error } from '@/components/ui/error';
import HeroSection from './HeroSection';
import PilotMapSection from './PilotMapSection';
import CtaSection from './CtaSection';

function LandingPage() {
    const { state, data } = useLandingStore(
        useShallow(s => ({ state: s.state, data: s.data }))
    );

    // Loads on every visit until it succeeds, but never retries in a loop while the server is unreachable
    useEffect(() => {
        const { state, fetch } = useLandingStore.getState();
        if (state === 'Idle' || state === 'Error') {
            fetch();
        }
    }, []);

    if (state === 'Loading' || state === 'Idle') {
        return <Spinner />;
    }

    if (state === 'Error' || data == null) {
        return <Error />;
    }

    return (
        <div className="flex flex-col gap-12">
            <HeroSection data={data} />
            <PilotMapSection data={data} />
            <CtaSection />
        </div>
    );
}

export default LandingPage;
