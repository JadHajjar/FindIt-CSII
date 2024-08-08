import style from "./ToolbarIcon.module.scss";
import { useValue, trigger, bindValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import { getModule, ModuleRegistryExtend } from "cs2/modding";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";

const PickerIconSrc = "coui://uil/Standard/PickerPipette.svg"; // Your tool's icon source
const FindItIconSrc = "coui://uil/Standard/Magnifier.svg"; // Your tool's icon source
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel"); // Binding to get if your tool is active
const PickerEnabled$ = bindValue<boolean>(mod.id, "PickerEnabled"); // Binding to get if your tool is active

// Getting the vanilla theme css for compatibility
const ToolBarButtonTheme: Theme | any = getModule(
  "game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss",
  "classes"
);
// Getting the vanilla theme css for compatibility
const ToolBarTheme: Theme | any = getModule("game-ui/game/components/toolbar/toolbar.module.scss", "classes");
const EditorButtonTheme: Theme | any = getModule("game-ui/editor/themes/editor-tool-button.module.scss", "classes");

// Trigger Icon Click on the C# side
function HandleFindItClick() {
  trigger(mod.id, "FindItIconToggled");
}
function HandlePickerClick() {
  trigger(mod.id, "PickerIconToggled");
}

export const ToolbarIconComponent: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { children, ...otherProps } = props || {};
    const ShowFindItPanel = useValue(ShowFindItPanel$); // Get if your tool is active
    const PickerEnabled = useValue(PickerEnabled$); // Get if your tool is active

    return (
      <>
        <Button
          src={PickerIconSrc}
          className={ToolBarButtonTheme.button}
          variant="icon"
          focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          selected={PickerEnabled}
          onSelect={HandlePickerClick}
        ></Button>

        <div className={ToolBarTheme.divider}></div>

        <Button
          src={FindItIconSrc}
          className={ToolBarButtonTheme.button + " " + style.ToolbarIcon}
          variant="icon"
          focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          selected={ShowFindItPanel}
          onSelect={HandleFindItClick}
        ></Button>

        <div className={ToolBarTheme.divider}></div>

        <Component {...otherProps}></Component>
      </>
    );
  };
};
