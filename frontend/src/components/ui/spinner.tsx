import {Drone} from "lucide-react";

export function Spinner() {
    return (
        <div className="flex items-center justify-center py-8">
            <Drone className="h-10 w-10 animate-spin" />
        </div>
    );
}
