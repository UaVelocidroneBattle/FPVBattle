import {CloudAlert} from "lucide-react";

export function Error() {
    return (
        <div className="flex items-center justify-center py-8">
            <CloudAlert className="h-10 w-10 text-red-400 animate-bounce" />
        </div>
    );
}
