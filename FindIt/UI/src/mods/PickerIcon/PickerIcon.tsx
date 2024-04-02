import iconStyles from "./pickerIcon.module.scss";
import { useValue, trigger, bindValue } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import src from "./picker.svg";

// These contain the coui paths to Unified Icon Library svg assets
export const PickerIconSrc = "coui://uil/Standard/Picker.svg";

// This functions trigger an event on C# side and C# designates the method to implement.
export const PickerEnabled$ = bindValue<boolean>(mod.id, "PickerEnabled");

export const ToolBarButtonTheme: Theme | any = getModule(
  "game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss",
  "classes"
);

export function handleClick() {
  trigger(mod.id, "PickerIconToggled");
}

export const PickerIconComponent = () => {
  const isPhotoMode =
    useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const PickerEnabled = useValue(PickerEnabled$);

  if (!isPhotoMode) {
    return (
      <>
        <Button
          src={src}
          className={
            ToolBarButtonTheme.button + " " + iconStyles.pickerToolbarIcon
          }
          variant="icon"
          focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          selected={PickerEnabled}
          onSelect={handleClick}></Button>
      </>
    );
  }

  return null;
};
