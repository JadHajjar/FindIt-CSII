import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";
import { OptionSection } from "domain/ContentViewType";
import { tool } from "cs2/bindings";

const PickerOptionsList$ = bindValue<OptionSection[]>(mod.id, "PickerOptionsList");

export const PickerComponent: ModuleRegistryExtend = (Component: any) => {
  // I believe you should not put anything here.
  return (props) => {
    // This defines aspects of the components.
    const { children, ...otherProps } = props || {};
    const PickerOptionsList = useValue(PickerOptionsList$);

    // This gets the original component that we may alter and return.
    var result: JSX.Element = Component();

    function OnOptionClicked(x: number, y: number, z: number) {
      trigger(mod.id, "PickerOptionClicked", x, y, z);
    }

    if (tool.activeTool$.value.id === "FindIt.Picker") {
      result.props.children?.push(<OptionsPanelComponent options={PickerOptionsList} OnChange={OnOptionClicked}></OptionsPanelComponent>);
    }

    return result;
  };
};
