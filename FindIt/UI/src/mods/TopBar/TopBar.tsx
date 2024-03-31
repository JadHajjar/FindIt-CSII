import { bindValue, trigger, useValue } from "cs2/api";
import { Theme, prefabProperties } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button, Panel, Portal, Scrollable } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend, getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./topBar.module.scss";
import { useState } from "react";
import { PrefabCategory } from "../../domain/category";
import { PrefabSubCategory } from "../../domain/subCategory";
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

export const AssetCategoryTabTheme: Theme | any = getModule(
  "game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.module.scss",
  "classes"
);

// These establishes the binding with C# side.
export const IsSearchLoading$ = bindValue<boolean>(mod.id, "IsSearchLoading");
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");
export const CurrentSubCategory$ = bindValue<number>(
  mod.id,
  "CurrentSubCategory"
);
export const CategoryList$ = bindValue<PrefabCategory[]>(
  mod.id,
  "CategoryList"
);
export const SubCategoryList$ = bindValue<PrefabSubCategory[]>(
  mod.id,
  "SubCategoryList"
);

// defines trigger event names.
export const eventName = "PrefabChange";

export const TopBarComponent: ModuleRegistryExtend = (Component) => {
  // I believe you should not put anything here.
  return (props) => {
    const { children, ...otherProps } = props || {};

    // These get the value of the bindings. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$); // To be replaced with UseValue(ShowFindItPanels$); Without C# side game ui will crash.
    const CurrentCategory = useValue(CurrentCategory$);
    const CurrentSubCategory = useValue(CurrentSubCategory$);
    const CategoryList = useValue(CategoryList$);
    const SubCategoryList = useValue(SubCategoryList$);
    const IsSearchLoading = useValue(IsSearchLoading$);

    // translation handling. Translates using locale keys that are defined in C# or fallback string here.
    const { translate } = useLocalization();

    const [searchQuery, setQuery] = useState("");

    function handleInputChange(value: Event) {
      if (value?.target instanceof HTMLTextAreaElement) {
        setSearchText(value.target.value);
      }
    }

    function setSearchText(value: string) {
      setQuery(value);
      trigger(mod.id, "SearchChanged", value);
    }

    function setCurrentCategory(id: number) {
      trigger(mod.id, "SetCurrentCategory", id);
    }

    function setCurrentSubCategory(id: number) {
      trigger(mod.id, "SetCurrentSubCategory", id);
    }

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!ShowFindItPanel) {
      return <Component {...otherProps}>{children}</Component>;
    }

    return (
      <>
        <div className={styles.topBar}>
          <div className={styles.topBarSection}>
            {IsSearchLoading && (
              <img
                src="coui://uil/Standard/HalfCircleProgress.svg"
                className={styles.loadingIcon}
              ></img>
            )}
            {!IsSearchLoading && (
              <img
                src="coui://uil/Standard/Magnifier.svg"
                className={styles.searchIcon}
              ></img>
            )}
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

              {searchQuery.trim() !== "" && (
                <Button
                  className={
                    VanillaComponentResolver.instance.assetGridTheme.item +
                    " " +
                    styles.clearIcon
                  }
                  variant="icon"
                  onSelect={() => {
                    setSearchText("");
                  }}
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                >
                  <img src="coui://uil/Standard/ArrowLeftClear.svg"></img>
                </Button>
              )}
            </div>
          </div>

          <div className={styles.topBarSection}>
            <div className={styles.categorySection}>
              {CategoryList.map((element) => (
                <VanillaComponentResolver.instance.ToolButton
                  selected={element.id == CurrentCategory}
                  onSelect={() => setCurrentCategory(element.id)}
                  src={element.icon}
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                  className={
                    VanillaComponentResolver.instance.toolButtonTheme.button
                  }
                ></VanillaComponentResolver.instance.ToolButton>
              ))}
            </div>

            <Button
              className={
                VanillaComponentResolver.instance.assetGridTheme.item +
                " " +
                styles.closeIcon
              }
              variant="icon"
              onSelect={() => trigger(mod.id, "FindItIconToggled")}
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            >
              <img src="coui://uil/Standard/XClose.svg"></img>
            </Button>
          </div>
        </div>

        <div className={AssetCategoryTabTheme.assetCategoryTabBar}>
          <div className={AssetCategoryTabTheme.items}>
            {SubCategoryList.map((element) => (
              <Button
                className={
                  VanillaComponentResolver.instance.assetGridTheme.item +
                  " " +
                  styles.tabButton
                }
                selected={element.id == CurrentSubCategory}
                variant="icon"
                onSelect={() => setCurrentSubCategory(element.id)}
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
              >
                <img
                  src={element.icon}
                  className={
                    VanillaComponentResolver.instance.assetGridTheme.thumbnail +
                    " " +
                    styles.gridThumbnail
                  }
                ></img>
              </Button>
            ))}
          </div>
        </div>
      </>
    );
  };
};
