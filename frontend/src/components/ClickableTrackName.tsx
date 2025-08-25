import { useState } from 'react'
import { Check, Copy } from 'lucide-react'

interface ClickableTrackNameProps {
    mapName: string
    trackName: string
}

const ClickableTrackName: React.FC<ClickableTrackNameProps> = ({ mapName, trackName }: ClickableTrackNameProps) => {
    const [copied, setCopied] = useState(false)

    const copyToClipboard = async () => {
        try {
            await navigator.clipboard.writeText(trackName);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch (err) {
            console.error('Failed to copy text: ', err);
        }
    }

    return (
        <div>
            <div className="text-slate-400 mb-2">{mapName}</div>
            <button
                onClick={copyToClipboard}
                className="text-xl font-semibold text-white flex items-center gap-2 hover:text-emerald-400 transition-colors duration-200 group w-full max-w-[500px]"
                title={trackName}
            >
                <span className="truncate">{trackName}</span>
                {copied ? (
                    <Check className="h-5 w-5 text-emerald-400 shrink-0" />
                ) : (
                    <Copy className="h-5 w-5 text-gray-500 group-hover:text-emerald-400 shrink-0 transition-colors" />
                )}
            </button>
        </div>
    )
}

export default ClickableTrackName;