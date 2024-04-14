import { useValue, bindValue, trigger } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { useState, useRef, useEffect } from "react";
import styles from "./mainContainer.module.scss";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";

const ColumnCount$ = bindValue<number>(mod.id, "ColumnCount");
const ExpandedColumnCount$ = bindValue<number>(mod.id, "ExpandedColumnCount");
const IsExpanded$ = bindValue<boolean>(mod.id, "IsExpanded");

// This functions trigger an event on C# side and C# designates the method to implement.
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");

const GameMainScreneTheme: Theme | any = getModule(
  "game-ui/game/components/game-main-screen.module.scss",
  "classes"
);

const PanelTheme: Theme | any = getModule(
  "game-ui/common/panel/panel.module.scss",
  "classes"
);

const AssetMenuTheme: Theme | any = getModule(
  "game-ui/game/components/asset-menu/asset-menu.module.scss",
  "classes"
);

const DefaultMainTheme: Theme | any = getModule(
  "game-ui/common/panel/themes/default.module.scss",
  "classes"
);

export const FindItMainContainerComponent = () => {
  const isPhotoMode =
    useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;

  const containerRef = useRef(null);

  const [optionsOpen, setOptionsOpen] = useState(false);
  const [containerLeft, setContainerLeft] = useState(0);

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const IsExpanded = useValue(IsExpanded$);
  const ColumnCount = IsExpanded
    ? useValue(ExpandedColumnCount$)
    : useValue(ColumnCount$);
  const panelWidth = ColumnCount * 113 + 15 + 20;

  const optionsOverflow = () =>
    window.innerWidth < panelWidth + containerLeft + 280;

  useEffect(() => {
    var newLeft =
      (containerRef.current as any)?.getBoundingClientRect().left ?? 0;

    if (newLeft !== 0) setContainerLeft(newLeft);
  });

  if (isPhotoMode || !ShowFindItPanel) return null;

  return (
    <>
      <div className={styles.findItMainContainer}>
        <div className={GameMainScreneTheme.toolLayout}>
          <div
            className={GameMainScreneTheme.toolMainColumn}
            style={{ position: "relative" }}
          >
            <div
              className={GameMainScreneTheme.toolPanel}
              ref={containerRef}
              style={{ width: panelWidth + "rem" }}
            >
              <div>
                {optionsOpen && optionsOverflow() && (
                  <div style={{ position: "relative" }}>
                    <div className={styles.topPanel}>
                      <div>
                        <OptionsPanelComponent></OptionsPanelComponent>
                      </div>
                    </div>
                  </div>
                )}
                <div className={styles.topBar}>
                  <TopBarComponent
                    optionsOpen={optionsOpen}
                    expanded={IsExpanded}
                    small={ColumnCount <= 5}
                    large={ColumnCount >= 8}
                    toggleOptionsOpen={() => setOptionsOpen(!optionsOpen)}
                    toggleEnlarge={() =>
                      trigger(mod.id, "SetIsExpanded", !IsExpanded)
                    }
                  ></TopBarComponent>
                </div>
                <div
                  className={styles.content + " " + AssetMenuTheme.assetPanel}
                >
                  <PrefabSelectionComponent
                    expanded={IsExpanded}
                  ></PrefabSelectionComponent>
                </div>
              </div>
              {optionsOpen && !optionsOverflow() && (
                <div
                  className={styles.rightPanel}
                  style={{ left: panelWidth + "rem" }}
                >
                  <div>
                    <OptionsPanelComponent></OptionsPanelComponent>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

/*
<div className={styles.findItMainContainer}>
<div className={GameMainScreneTheme.toolLayout}>
  <div>
    <div className={styles.toolContainer}>
      <div>
        <div className={styles.toolContent}>
          <div>
            <div style={{ width: panelWidth + "rem" }}>
              <div className={DefaultMainTheme.header}>
                <TopBarComponent
                  viewType={viewType}
                  setViewType={setViewType}
                ></TopBarComponent>
              </div>
              <div
                className={
                  DefaultMainTheme.content +
                  " " +
                  AssetMenuTheme.assetPanel
                }
              >
                <PrefabSelectionComponent
                  viewType={viewType}
                ></PrefabSelectionComponent>
              </div>
            </div>
            <div className={styles.rightPanel}></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
</div>
*/
