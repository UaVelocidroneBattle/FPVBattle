import { create } from 'zustand';
import { getApiCountriesAll, getApiCountriesPilots } from '@/api/client';
import type { CountryModel, CountryPilotModel } from '@/api/client/types.gen';

interface CountriesState {
    countries: CountryModel[];
    pilots: CountryPilotModel[];
    loadingCountries: boolean;
    loadingPilots: boolean;
    error: string | null;
}

interface CountriesActions {
    fetchCountries: () => Promise<void>;
    fetchPilots: (countryCode: string) => Promise<void>;
}

type CountriesStore = CountriesState & CountriesActions;

export const useCountriesStore = create<CountriesStore>()((set) => ({
    countries: [],
    pilots: [],
    loadingCountries: false,
    loadingPilots: false,
    error: null,

    fetchCountries: async () => {
        set({ loadingCountries: true, error: null });
        try {
            const response = await getApiCountriesAll();
            set({ countries: response.data ?? [], loadingCountries: false });
        } catch {
            set({ error: 'Failed to load countries', loadingCountries: false });
        }
    },

    fetchPilots: async (countryCode: string) => {
        set({ loadingPilots: true, error: null, pilots: [] });
        try {
            const response = await getApiCountriesPilots({ query: { countryCode } });
            set({ pilots: response.data ?? [], loadingPilots: false });
        } catch {
            set({ error: 'Failed to load pilots', loadingPilots: false });
        }
    },
}));
