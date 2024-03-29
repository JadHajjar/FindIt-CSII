import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button, Panel, Portal, Scrollable } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./topBar.module.scss";
import { useState } from "react";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";

export enum TextInputType {
  Text = "text",
  Password = "password",
}

export type PropsTextInput = {
  focusKey?: FocusKey;
  debugName?: string;
  type?: TextInputType | string;
  value?: string;
  selectAllOnFocus?: boolean;
  placeholder?: string;
  vkTitle?: string;
  vkDescription?: string;
  disabled?: boolean;
  className?: string;
  multiline: number;
  onFocus?: (value: Event) => void;
  onBlur?: (value: Event) => void;
  onKeyDown?: (value: Event) => void;
  onChange?: (value: Event) => void;
  onMouseUp?: (value: Event) => void;
};

export function TextInput(props: PropsTextInput): JSX.Element {
  return getModule(
    "game-ui/common/input/text/text-input.tsx",
    "TextInput"
  ).render(props);
}

export const TextInputTheme: Theme | any = getModule(
  "game-ui/editor/widgets/item/editor-item.module.scss",
  "classes"
);

export const FocusDisabled$: FocusKey = getModule(
  "game-ui/common/focus/focus-key.ts",
  "FOCUS_DISABLED"
);

// This functions trigger an event on C# side and C# designates the method to implement.
export function changePrefab(prefab: string) {
  trigger(mod.id, eventName, prefab);
}

// These establishes the binding with C# side. Without C# side game ui will crash.
// export const ActivePrefabName$ =        bindValue<string> (mod.id, 'ActivePrefabName');

// defines trigger event names.
export const eventName = "PrefabChange";

export const TopBarComponent = () => {
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  const [searchQuery, setQuery] = useState("");

  function handleInputChange(value: Event) {
    if (value?.target instanceof HTMLTextAreaElement) {
      setQuery(value.target.value);
    }
  }

  return (
    <>
      <div className={styles.topBar}>
        <div className={styles.topBarSection}>
          <img
            src="coui://uil/Standard/Magnifier.svg"
            className={styles.searchIcon}
          ></img>
          <div className={styles.searchArea}>
            <TextInput
              multiline={1}
              value={searchQuery}
              disabled={false}
              type={TextInputType.Text}
              className={TextInputTheme.input + " " + styles.textBox}
              focusKey={FocusDisabled$}
              onChange={handleInputChange}
              placeholder="Search..."
            ></TextInput>

            <Button
              className={
                VanillaComponentResolver.instance.assetGridTheme.item +
                " " +
                styles.clearIcon
              }
              variant="icon"
              onSelect={() => {}}
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            >
              <img src="coui://uil/Standard/ArrowLeftClear.svg"></img>
            </Button>
          </div>
        </div>

        <div className={styles.topBarSection}>
          <div className={styles.categorySection}>
            <VanillaComponentResolver.instance.ToolButton
              selected={true}
              onSelect={() => {}}
              src="coui://uil/Colored/TreeVanilla.svg"
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
              className={
                VanillaComponentResolver.instance.toolButtonTheme.button
              }
            ></VanillaComponentResolver.instance.ToolButton>

            <VanillaComponentResolver.instance.ToolButton
              selected={false}
              onSelect={() => {}}
              src="Media/Game/Icons/Roads.svg"
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
              className={
                VanillaComponentResolver.instance.toolButtonTheme.button +
                " " +
                styles.hasAction
              }
            ></VanillaComponentResolver.instance.ToolButton>

            <VanillaComponentResolver.instance.ToolButton
              selected={false}
              onSelect={() => {}}
              src="Media/Game/Icons/ZoneSignature.svg"
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
              className={
                VanillaComponentResolver.instance.toolButtonTheme.button +
                " " +
                styles.hasAction
              }
            ></VanillaComponentResolver.instance.ToolButton>
          </div>

          <Button
            className={
              VanillaComponentResolver.instance.assetGridTheme.item +
              " " +
              styles.closeIcon
            }
            variant="icon"
            onSelect={() => {}}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img src="coui://uil/Standard/XClose.svg"></img>
          </Button>
        </div>
      </div>

      <div className="asset-category-tab-bar_IGA">
        <div className="items_gPf">
          <Button
            className={
              VanillaComponentResolver.instance.assetGridTheme.item +
              " " +
              styles.tabButton
            }
            selected={true}
            variant="icon"
            onSelect={() => {}}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img
              src="coui://uil/Colored/TreeVanilla.svg"
              className={
                VanillaComponentResolver.instance.assetGridTheme.thumbnail +
                " " +
                styles.gridThumbnail
              }
            ></img>
          </Button>
          <Button
            className={
              VanillaComponentResolver.instance.assetGridTheme.item +
              " " +
              styles.tabButton
            }
            selected={false}
            variant="icon"
            onSelect={() => {}}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img
              src="Media/Game/Icons/Vegetation.svg"
              className={
                VanillaComponentResolver.instance.assetGridTheme.thumbnail +
                " " +
                styles.gridThumbnail
              }
            ></img>
          </Button>
        </div>
      </div>
    </>
  );
};
