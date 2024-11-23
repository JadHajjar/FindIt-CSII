import { bindValue, useValue } from "cs2/api";
import { ModuleRegistryExtend } from "cs2/modding";
import mod from "../../../mod.json";

const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const AlignmentStyle$ = bindValue<string>(mod.id, "AlignmentStyle");

export const RemoveVanillaRightToolbar: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { children, ...otherProps } = props || {};

    const ShowFindItPanel = useValue(ShowFindItPanel$);
    const AlignmentStyle = useValue(AlignmentStyle$);

    if (!ShowFindItPanel || AlignmentStyle != "Right") {
      return <Component {...otherProps}>{children}</Component>;
    }

    return <></>;
  };
};
