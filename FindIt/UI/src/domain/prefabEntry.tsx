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
  tags: string[];
  creatorTags: string[];
  random: boolean;
  placed: number;
  hot: boolean;
  pdxId: number;
}
