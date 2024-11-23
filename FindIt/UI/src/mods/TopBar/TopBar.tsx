import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button, Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./topBar.module.scss";
import { useEffect, useRef, useState } from "react";
import { PrefabCategory } from "../../domain/category";
import { PrefabSubCategory } from "../../domain/subCategory";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import classNames from "classnames";
import { BasicButton } from "mods/BasicButton/BasicButton";
import find from "images/find.svg";
import random from "images/random.svg";
import shrink from "images/shrink.svg";
import expand from "images/expand.svg";
import filter from "images/filter.svg";
import filterX from "images/filterX.svg";
import finditAlignCenter from "images/finditAlignCenter.svg";
import finditAlignRight from "images/finditAlignRight.svg";
import sort from "images/sort.svg";
import { FOCUS_DISABLED } from "cs2/input";

export interface TopBarProps {
  sortingOpen: any;
  optionsOpen: boolean;
  expanded: boolean;
  small: boolean;
  large: boolean;
  toggleOptionsOpen: () => void;
  toggleSortingOpen: () => void;
  toggleEnlarge: () => void;
}

const TextInput = getModule("game-ui/common/input/text/text-input.tsx", "TextInput");

const TextInputTheme: Theme | any = getModule("game-ui/editor/widgets/item/editor-item.module.scss", "classes");

const AssetCategoryTabTheme: Theme | any = getModule(
  "game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.module.scss",
  "classes"
);

// These establishes the binding with C# side.
const IsSearchLoading$ = bindValue<boolean>(mod.id, "IsSearchLoading");
const ClearSearchBar$ = bindValue<boolean>(mod.id, "ClearSearchBar");
const FocusSearchBar$ = bindValue<boolean>(mod.id, "FocusSearchBar");
const AreFiltersSet$ = bindValue<boolean>(mod.id, "AreFiltersSet");
const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");
const CurrentSearch$ = bindValue<string>(mod.id, "CurrentSearch");
const CurrentSubCategory$ = bindValue<number>(mod.id, "CurrentSubCategory");
const CategoryList$ = bindValue<PrefabCategory[]>(mod.id, "CategoryList");
const SubCategoryList$ = bindValue<PrefabSubCategory[]>(mod.id, "SubCategoryList");
const AlignmentStyle$ = bindValue<string>(mod.id, "AlignmentStyle");

export const TopBarComponent = (props: TopBarProps) => {
  // These get the value of the bindings. Or they will when we have bindings.
  const CurrentCategory = useValue(CurrentCategory$);
  const CurrentSubCategory = useValue(CurrentSubCategory$);
  const CategoryList = useValue(CategoryList$);
  const SubCategoryList = useValue(SubCategoryList$);
  const IsSearchLoading = useValue(IsSearchLoading$);
  const AreFiltersSet = useValue(AreFiltersSet$);
  const CurrentSearch = useValue(CurrentSearch$);
  const ClearSearchBar = useValue(ClearSearchBar$);
  const FocusSearchBar = useValue(FocusSearchBar$);
  const AlignmentStyle = useValue(AlignmentStyle$);
  const searchRef = useRef(null);
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  finditAlignRight + finditAlignCenter;

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

  function RenderCategoryList(): JSX.Element {
    return (
      <div className={styles.categorySection}>
        {CategoryList.map((element) => (
          <>
            {element.id == 0 && <span style={{ flex: 1 }} />}
            <BasicButton
              tooltip={element.toolTip}
              src={element.icon}
              onClick={element.id == CurrentCategory ? undefined : () => setCurrentCategory(element.id)}
              className={classNames(VanillaComponentResolver.instance.toolButtonTheme.button, element.id == CurrentCategory && styles.selected)}
            >
              <span />
            </BasicButton>
          </>
        ))}
      </div>
    );
  }

  function RenderButtonSection(): JSX.Element {
    return (
      <div className={styles.buttonsSection}>
        {AlignmentStyle !== "Right" && (
          <BasicButton
            tooltip={props.expanded ? translate("Tooltip.LABEL[FindIt.Shrink]", "Shrink") : translate("Tooltip.LABEL[FindIt.Expand]", "Expand")}
            onClick={props.toggleEnlarge}
            mask={props.expanded ? shrink : expand}
            className={props.expanded && styles.selected}
          />
        )}

        <BasicButton
          tooltip={translate("Tooltip.LABEL[FindIt.ToggleSorting]", "Sorting")}
          onClick={props.toggleSortingOpen}
          mask={sort}
          className={props.sortingOpen && styles.selected}
        />

        <BasicButton
          tooltip={translate("Tooltip.LABEL[FindIt.ToggleFilters]", "Filters")}
          onClick={props.toggleOptionsOpen}
          mask={filter}
          className={props.optionsOpen && styles.selected}
        />

        <div className={styles.seperator} />

        <BasicButton
          tooltip={translate("Tooltip.LABEL[FindIt.ClearFilter]", "Clear Filters")}
          disabled={!AreFiltersSet}
          onClick={() => trigger(mod.id, "ClearFilters")}
          mask={filterX}
        />

        <BasicButton
          tooltip={translate("Tooltip.LABEL[FindIt.Random]", "Random")}
          onClick={() => trigger(mod.id, "OnRandomButtonClicked")}
          mask={random}
        />
      </div>
    );
  }

  return (
    <>
      <div className={classNames(props.large && styles.large, props.small && styles.small)}>
        <div className={styles.topBar}>
          <div className={classNames(styles.topBarSection, AlignmentStyle === "Right" && styles.expandedSearchArea)}>
            {IsSearchLoading && <img style={{ maskImage: "url(coui://uil/Standard/HalfCircleProgress.svg)" }} className={styles.loadingIcon}></img>}
            {!IsSearchLoading && <img style={{ maskImage: `url(${find})` }} className={styles.searchIcon}></img>}
            <div className={styles.searchArea}>
              <TextInput
                ref={searchRef}
                multiline={1}
                value={CurrentSearch}
                disabled={false}
                type="text"
                className={classNames(TextInputTheme.input, styles.textBox)}
                focusKey={FOCUS_DISABLED}
                onChange={handleInputChange}
                placeholder={translate("Editor.SEARCH_PLACEHOLDER", "Search...")}
              ></TextInput>

              {CurrentSearch.trim() !== "" && (
                <Button
                  className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.clearIcon)}
                  variant="icon"
                  onSelect={() => {
                    setSearchText("");
                  }}
                >
                  <img src="coui://uil/Standard/ArrowLeftClear.svg"></img>
                </Button>
              )}
            </div>

            {AlignmentStyle !== "Right" && RenderButtonSection()}
          </div>

          <div className={styles.topBarSection}>
            <Tooltip tooltip={translate("Tooltip.LABEL[FindIt.ClosePanel]", "Close Panel")}>
              <Button
                className={VanillaComponentResolver.instance.assetGridTheme.item + " " + styles.closeIcon}
                variant="icon"
                onSelect={() => trigger(mod.id, "FindItCloseToggled")}
              >
                <img src="coui://uil/Standard/XClose.svg"></img>
              </Button>
            </Tooltip>
          </div>
        </div>

        {AlignmentStyle === "Right" && <div className={styles.lowerButtonSection}>{RenderButtonSection()}</div>}

        <div className={styles.rowCategoryBar}>{RenderCategoryList()}</div>

        <div className={classNames(AssetCategoryTabTheme.assetCategoryTabBar, styles.subCategoryContainer)}>
          <div className={AssetCategoryTabTheme.items}>
            {SubCategoryList.map((element) => (
              <BasicButton
                tooltip={element.toolTip}
                onClick={element.id == CurrentSubCategory ? undefined : () => setCurrentSubCategory(element.id)}
                className={classNames(
                  VanillaComponentResolver.instance.assetGridTheme.item,
                  styles.tabButton,
                  element.id == CurrentSubCategory && styles.selected
                )}
              >
                <img src={element.icon} className={VanillaComponentResolver.instance.assetGridTheme.thumbnail + " " + styles.gridThumbnail}></img>
              </BasicButton>
            ))}
          </div>
        </div>
      </div>
    </>
  );
};
