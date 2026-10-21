import { useValue, bindValue, trigger } from "cs2/api";
import { game, selectedInfo, Theme, tool } from "cs2/bindings";
import { InputActionConsumer } from "cs2/input";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { useState, useRef, useEffect } from "react";
import styles from "./mainContainer.module.scss";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";
import { TagEditorComponent } from "mods/TagEditor/TagEditor";
import { OptionSection } from "domain/ContentViewType";
import { useLocalization } from "cs2/l10n";
import classNames from "classnames";

const PanelWidth$ = bindValue<number>(mod.id, "PanelWidth");
const IsExpanded$ = bindValue<boolean>(mod.id, "IsExpanded");
const IsPrefabSelection$ = bindValue<boolean>(mod.id, "IsPrefabSelection");
const AlignmentStyle$ = bindValue<string>(mod.id, "AlignmentStyle");
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const IsWindowLocked$ = bindValue<boolean>(mod.id, "IsWindowLocked");
const OptionsList$ = bindValue<OptionSection[]>(mod.id, "OptionsList");
const TagEditorPrefabId$ = bindValue<number>(mod.id, "TagEditorPrefabId");

const GameMainScreneTheme: Theme | any = getModule(
  "game-ui/game/components/game-main-screen.module.scss",
  "classes",
);

const PanelTheme: Theme | any = getModule(
  "game-ui/common/panel/panel.module.scss",
  "classes",
);

const AssetMenuTheme: Theme | any = getModule(
  "game-ui/game/components/asset-menu/asset-menu.module.scss",
  "classes",
);

const DefaultMainTheme: Theme | any = getModule(
  "game-ui/common/panel/themes/default.module.scss",
  "classes",
);

const BackActions = { Back: () => trigger(mod.id, "FindItCloseToggled") };

export const FindItMainContainerComponent = () => {
  const activeGamePanel = useValue(game.activeGamePanel$);
  const isPhotoMode = activeGamePanel?.__Type == game.GamePanelType.PhotoMode;
  const activeToolId = useValue(tool.activeTool$).id;
  const hasSelection = useValue(selectedInfo.activeSelection$);
  const { translate } = useLocalization();

  const containerRef = useRef(null);

  const [sortingOpen, setSortingOpen] = useState(false);
  const [optionsOpen, setOptionsOpen] = useState(false);
  const [containerLeft, setContainerLeft] = useState(0);

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const IsWindowLocked = useValue(IsWindowLocked$);
  const IsPrefabSelection = useValue(IsPrefabSelection$);
  const IsExpanded = useValue(IsExpanded$);
  const PanelWidth = useValue(PanelWidth$) + 15 + 20;
  const OptionsList = useValue(OptionsList$);
  const AlignmentStyle = useValue(AlignmentStyle$);
  const Alignment = IsPrefabSelection ? "Center" : AlignmentStyle;
  const TagEditorPrefabId = useValue(TagEditorPrefabId$);
  const tagEditorOpen = TagEditorPrefabId !== 0;
  const sidePanelOpen = optionsOpen || sortingOpen || tagEditorOpen;

  useEffect(() => {
    if (!tagEditorOpen) return;

    setOptionsOpen(false);
    setSortingOpen(false);
  }, [TagEditorPrefabId]);

  const optionsOverflow = () =>
    Alignment !== "Center" ||
    window.innerWidth <
      containerLeft + ((PanelWidth + 300) * window.innerHeight) / 1080;

  useEffect(() => {
    var newLeft =
      (containerRef.current as any)?.getBoundingClientRect().left ?? 0;

    if (newLeft !== 0) setContainerLeft(newLeft);
  });

  if (isPhotoMode || !(ShowFindItPanel || IsWindowLocked)) return null;

  function onOptionClicked(x: number, y: number, z: number): void {
    trigger(mod.id, "OptionClicked", x, y, z);
  }

  function toggleSortingOpen(): void {
    setSortingOpen(!sortingOpen);
    setOptionsOpen(false);
    if (tagEditorOpen) trigger(mod.id, "CloseTagEditor");
  }

  function toggleOptionsOpen(): void {
    setOptionsOpen(!optionsOpen);
    setSortingOpen(false);
    if (tagEditorOpen) trigger(mod.id, "CloseTagEditor");
  }

  function renderSidePanelContent(): JSX.Element {
    if (tagEditorOpen) return <TagEditorComponent />;

    return (
      <>
        <div className={styles.title}>
          {optionsOpen
            ? translate("Tooltip.LABEL[FindIt.Filters]", "Filters")
            : translate("Tooltip.LABEL[FindIt.Sorting]", "Sorting")}
        </div>
        <OptionsPanelComponent
          options={OptionsList.filter((x) => x.id < 0 !== optionsOpen)}
          OnChange={onOptionClicked}
        ></OptionsPanelComponent>
      </>
    );
  }

  function toggleEnlarge(): void {
    trigger(mod.id, "SetIsExpanded", !IsExpanded);
  }

  // Escape is the game's Back action, and claiming Back keeps Escape from opening the pause menu.
  // So the panel claims it only when it would close and the game has nothing to close first.
  // While a tool is active, the game's Back drops the tool, which closes the panel.
  // The last consumer attached gets Escape, so this one mounts when it claims Back rather than changing its actions.
  const closesOnBack =
    !IsWindowLocked &&
    activeToolId === tool.DEFAULT_TOOL &&
    activeGamePanel === null &&
    !hasSelection;

  return (
    <div
      className={classNames(
        styles.findItMainContainer,
        styles["align" + Alignment],
      )}
    >
      {closesOnBack && (
        <InputActionConsumer actions={BackActions} ignoreFocusState={true} />
      )}
      <div className={styles.toolLayout}>
        <div
          className={
            Alignment !== "Center"
              ? styles.toolMainColumn
              : GameMainScreneTheme.toolMainColumn
          }
          style={
            Alignment === "Center" ? undefined : { width: PanelWidth + "rem" }
          }
        >
          <div
            className={classNames(
              GameMainScreneTheme.toolPanel,
              IsPrefabSelection && styles.prefabSelection,
            )}
            ref={containerRef}
            style={
              Alignment !== "Center" ? undefined : { width: PanelWidth + "rem" }
            }
          >
            <div className={styles.toolContainer}>
              {sidePanelOpen && optionsOverflow() && (
                <div className={styles.topPanel}>
                  <div>
                    <div>{renderSidePanelContent()}</div>
                  </div>
                </div>
              )}
              <div className={styles.topBar}>
                <TopBarComponent
                  sortingOpen={sortingOpen}
                  optionsOpen={optionsOpen}
                  expanded={IsExpanded}
                  small={PanelWidth <= 685}
                  large={PanelWidth >= 850}
                  toggleSortingOpen={toggleSortingOpen}
                  toggleOptionsOpen={toggleOptionsOpen}
                  toggleEnlarge={toggleEnlarge}
                />
              </div>
              <div
                className={classNames(
                  styles.content,
                  AssetMenuTheme.assetPanel,
                )}
              >
                <PrefabSelectionComponent
                  expanded={IsExpanded}
                ></PrefabSelectionComponent>
              </div>
            </div>
            {sidePanelOpen && !optionsOverflow() && (
              <div
                className={styles.rightPanel}
                style={{ left: PanelWidth + "rem" }}
              >
                <div>{renderSidePanelContent()}</div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
