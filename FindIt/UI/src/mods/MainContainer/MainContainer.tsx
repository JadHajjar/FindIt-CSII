import { useValue, bindValue } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { ContentViewType } from "../../domain/ContentViewType";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { Portal } from "cs2/ui";
import { useState } from "react";
import styles from "./mainContainer.module.scss";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";

// These contain the coui paths to Unified Icon Library svg assets
const findItIconSrc = "coui://uil/Standard/Magnifier.svg";

const ColumnCount$ = bindValue<number>(mod.id, "ColumnCount");

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

  const [viewType, setViewType] = useState(ContentViewType.GridWithText);

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const ColumnCount = useValue(ColumnCount$);
  const panelWidth = ColumnCount * 113 + 15 + 20;

  if (isPhotoMode || !ShowFindItPanel) return null;

  return (
    <Portal>
      <div className={styles.findItMainContainer}>
        <div className={GameMainScreneTheme.toolLayout}>
          <div
            className={GameMainScreneTheme.toolMainColumn}
            style={{ position: "relative" }}
          >
            <div
              className={GameMainScreneTheme.toolPanel}
              style={{ width: panelWidth + "rem" }}
            >
              <div>
                <div className={DefaultMainTheme.header}>
                  <TopBarComponent></TopBarComponent>
                </div>
                <div
                  className={
                    DefaultMainTheme.content + " " + AssetMenuTheme.assetPanel
                  }
                >
                  <PrefabSelectionComponent
                    viewType={viewType}
                  ></PrefabSelectionComponent>
                </div>
              </div>
              <div
                className={styles.rightPanel}
                style={{ left: panelWidth + "rem" }}
              >
                <div>
                  <OptionsPanelComponent
                    viewType={viewType}
                    setViewType={setViewType}
                  ></OptionsPanelComponent>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Portal>
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
