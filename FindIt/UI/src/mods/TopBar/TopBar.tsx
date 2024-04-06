import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./topBar.module.scss";
import { useEffect, useRef, useState } from "react";
import { PrefabCategory } from "../../domain/category";
import { PrefabSubCategory } from "../../domain/subCategory";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";

// In order to use forwardRef, don't wrap a layer of
interface PropsTextInput {
  focusKey?: FocusKey;
  debugName?: string;
  type?: "text" | "password";
  value?: string;
  selectAllOnFocus?: boolean;
  placeholder?: string;
  vkTitle?: string;
  vkDescription?: string;
  disabled?: boolean;
  className?: string;
  multiline: number;
  ref?: any;
  onFocus?: (value: Event) => void;
  onBlur?: (value: Event) => void;
  onKeyDown?: (value: Event) => void;
  onChange?: (value: Event) => void;
  onMouseUp?: (value: Event) => void;
}

const TextInput = getModule(
  "game-ui/common/input/text/text-input.tsx",
  "TextInput"
);

const TextInputTheme: Theme | any = getModule(
  "game-ui/editor/widgets/item/editor-item.module.scss",
  "classes"
);

const FocusDisabled$: FocusKey = getModule(
  "game-ui/common/focus/focus-key.ts",
  "FOCUS_DISABLED"
);

const AssetCategoryTabTheme: Theme | any = getModule(
  "game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.module.scss",
  "classes"
);

// These establishes the binding with C# side.
const IsSearchLoading$ = bindValue<boolean>(mod.id, "IsSearchLoading");
const ClearSearchBar$ = bindValue<boolean>(mod.id, "ClearSearchBar");
const FocusSearchBar$ = bindValue<boolean>(mod.id, "FocusSearchBar");
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");
const CurrentSearch$ = bindValue<string>(mod.id, "CurrentSearch");
const CurrentSubCategory$ = bindValue<number>(mod.id, "CurrentSubCategory");
const CategoryList$ = bindValue<PrefabCategory[]>(mod.id, "CategoryList");
const SubCategoryList$ = bindValue<PrefabSubCategory[]>(
  mod.id,
  "SubCategoryList"
);

export const TopBarComponent = () => {
  // These get the value of the bindings. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$); // To be replaced with UseValue(ShowFindItPanels$); Without C# side game ui will crash.
  const CurrentCategory = useValue(CurrentCategory$);
  const CurrentSubCategory = useValue(CurrentSubCategory$);
  const CategoryList = useValue(CategoryList$);
  const SubCategoryList = useValue(SubCategoryList$);
  const IsSearchLoading = useValue(IsSearchLoading$);
  const CurrentSearch = useValue(CurrentSearch$);
  const ClearSearchBar = useValue(ClearSearchBar$);
  const FocusSearchBar = useValue(FocusSearchBar$);
  const searchRef = useRef(null);
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  const handleInputChange = (value: Event) => {
    if (value?.target instanceof HTMLTextAreaElement) {
      setSearchText(value.target.value);
    }
  };

  const setSearchText = (value: string) => {
    trigger(mod.id, "SearchChanged", value);
  };

  const setCurrentCategory = (id: number) => {
    trigger(mod.id, "SetCurrentCategory", id);
  };

  const setCurrentSubCategory = (id: number) => {
    trigger(mod.id, "SetCurrentSubCategory", id);
  };

  const setFocus = () => {
    if (searchRef === null || searchRef.current === null) return;
    (searchRef.current as any).focus();
    (searchRef.current as any).select();
  };

  if (FocusSearchBar) {
    trigger(mod.id, "OnSearchFocused");
    setFocus();
  }

  if (ClearSearchBar) {
    trigger(mod.id, "OnSearchCleared");
    setSearchText("");
  }

  // Do not put any Hooks (i.e. UseXXXX) after this point.
  if (!ShowFindItPanel) {
    return <></>;
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
              ref={searchRef}
              multiline={1}
              value={CurrentSearch}
              disabled={false}
              type={"text"}
              className={TextInputTheme.input + " " + styles.textBox}
              focusKey={FocusDisabled$}
              onChange={handleInputChange}
              placeholder={translate("Editor.SEARCH_PLACEHOLDER", "Search...")}
            ></TextInput>

            {CurrentSearch.trim() !== "" && (
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
            onSelect={() => trigger(mod.id, "FindItCloseToggled")}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img src="coui://uil/Standard/XClose.svg"></img>
          </Button>
        </div>
      </div>

      <div className={AssetCategoryTabTheme.assetCategoryTabBar}>
        <div
          className={
            AssetCategoryTabTheme.items + " " + styles.subCategoryContainer
          }
        >
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
