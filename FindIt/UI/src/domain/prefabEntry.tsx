export interface PrefabEntry {
  id: number;
  name: string;
  thumbnails: string[];
  fallbackThumbnail: string;
  dlcThumbnail: string;
  categoryThumbnail: string;
  packThumbnails: string[];
  themeThumbnail: string;
  favorited: boolean;
  random: boolean;
  placed: boolean;
  pdxId: number;
}
