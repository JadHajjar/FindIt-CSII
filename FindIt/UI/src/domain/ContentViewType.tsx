export interface OptionSection {
  id: number;
  name: string;
  isToggle: boolean;
  isButton: boolean;
  isCheckbox: boolean;
  valueSign: number;
  options: OptionItem[];
}

export interface OptionItem {
  id: number;
  name: string;
  icon: string;
  selected: boolean;
  isValue: boolean;
  disabled: boolean;
  value: string;
}
