export function getInitials(displayName: string, email: string) {
    const words = displayName.trim().split(/\s+/).filter(Boolean);
    const initials = words.slice(0, 2).map((word) => word[0]).join("");
    return (initials || email[0] || "?").toUpperCase();
}
