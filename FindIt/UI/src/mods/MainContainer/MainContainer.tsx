import { useValue, bindValue } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { SortingPanel } from "../SortingPanel/SortingPanel";
import { Portal } from "cs2/ui";
import styles from "./mainContainer.module.scss";

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

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const ColumnCount = useValue(ColumnCount$);
  const panelWidth = ColumnCount * 113 + 10 + 20;

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
                    DefaultMainTheme.content +
                    " " +
                    PanelTheme.panel +
                    " " +
                    AssetMenuTheme.assetPanel
                  }
                >
                  <PrefabSelectionComponent></PrefabSelectionComponent>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Portal>
  );
};
