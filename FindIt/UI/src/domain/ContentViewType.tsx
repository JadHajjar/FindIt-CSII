export interface optionSection {
  id: number;
  name: string;
  options: optionItem[];
}

export interface optionItem {
  id: number;
  name: string;
  icon: string;
  selected: boolean;
  isValue: boolean;
  value: string;
}
