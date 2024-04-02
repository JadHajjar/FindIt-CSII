import { bindValue, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { ModuleRegistryExtend } from "cs2/modding";

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");

export const RemoveVanillaAssetGridComponent: ModuleRegistryExtend = (
  Component
) => {
  // I believe you should not put anything here.
  return (props) => {
    const { children, ...otherProps } = props || {};

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$);

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!ShowFindItPanel) {
      return <Component {...otherProps}>{children}</Component>;
    }

    return <></>;
  };
};
