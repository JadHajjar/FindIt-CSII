import { bindValue, useValue } from "cs2/api";
import { ModuleRegistryExtend } from "cs2/modding";
import mod from "../../../mod.json";

const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const IsWindowLocked$ = bindValue<boolean>(mod.id, "IsWindowLocked");
const AlignmentStyle$ = bindValue<string>(mod.id, "AlignmentStyle");

export const RemoveVanillaRightToolbar: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { children, ...otherProps } = props || {};

    const IsWindowLocked = useValue(IsWindowLocked$);
    const ShowFindItPanel = useValue(ShowFindItPanel$);
    const AlignmentStyle = useValue(AlignmentStyle$);

    if (!(ShowFindItPanel || IsWindowLocked) || AlignmentStyle === "Center") {
      return <Component {...otherProps}>{children}</Component>;
    }

    return <></>;
  };
};
