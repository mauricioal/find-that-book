export interface BookCandidate {
    title: string;
    authors: string[];
    firstPublishYear: number | null;
    openLibraryId: string;
    coverUrl: string | null;
    explanation: string;
    rank: number;
}
