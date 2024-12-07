import { bindValue, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { ModuleRegistryExtend } from "cs2/modding";

// These establishes the binding with C# side.
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const IsWindowLocked$ = bindValue<boolean>(mod.id, "IsWindowLocked");

export const RemoveVanillaAssetMenuComponent: ModuleRegistryExtend = (Component) => {
  // I believe you should not put anything here.
  return (props) => {
    const { children, ...otherProps } = props || {};

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const IsWindowLocked = useValue(IsWindowLocked$);
    const ShowFindItPanel = useValue(ShowFindItPanel$);

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (ShowFindItPanel || IsWindowLocked) {
      return <></>;
    }

    return <Component {...otherProps}>{children}</Component>;
  };
};
