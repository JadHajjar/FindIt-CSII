import { bindValue, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { ModuleRegistryExtend } from "cs2/modding";
import styles from "./WrapToolOptionsPanel.module.scss";

// These establishes the binding with C# side.
const IsWindowLocked$ = bindValue<boolean>(mod.id, "IsWindowLocked");
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const AlignmentStyle$ = bindValue<string>(mod.id, "AlignmentStyle");
const PanelWidth$ = bindValue<number>(mod.id, "PanelWidth");

export const WrapToolOptionsPanel: ModuleRegistryExtend = (Component) => {
  // I believe you should not put anything here.
  return (props) => {
    const { children, ...otherProps } = props || {};

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const IsWindowLocked = useValue(IsWindowLocked$);
    const ShowFindItPanel = useValue(ShowFindItPanel$);
    const AlignmentStyle = useValue(AlignmentStyle$);
    const PanelWidth = useValue(PanelWidth$) + 15 + 20 + 20 + 15;

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!(ShowFindItPanel || IsWindowLocked) || AlignmentStyle != "Right") {
      return <Component {...otherProps}>{children}</Component>;
    }

    return (
      <div className={styles.wrapper} style={{ paddingRight: PanelWidth + "rem" }}>
        <Component {...otherProps}>{children}</Component>
      </div>
    );
  };
};
