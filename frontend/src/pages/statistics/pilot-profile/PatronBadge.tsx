import { TbBrandPatreonFilled } from 'react-icons/tb';

function PatronBadge() {
    return (
        <span
            title="Supports FPV Battle on Patreon"
            className="flex items-center gap-1.5 text-xs sm:text-sm font-medium text-amber-300 bg-amber-400/10 border border-amber-400/50 rounded-full pl-3 pr-4 py-1"
        >
            <TbBrandPatreonFilled className="size-3.5" />
            Patron
        </span>
    );
}

export default PatronBadge;
