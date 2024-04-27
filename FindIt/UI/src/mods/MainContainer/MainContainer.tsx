import { useValue, bindValue, trigger } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import {
  PrefabSelectionComponent,
  ViewStyle$,
} from "mods/PrefabSelection/PrefabSelection";
import { useState, useRef, useEffect } from "react";
import styles from "./mainContainer.module.scss";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";

const PanelWidth$ = bindValue<number>(mod.id, "PanelWidth");
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
  const PanelWidth = useValue(PanelWidth$) + 15 + 20;

  const optionsOverflow = () =>
    window.innerWidth <
    containerLeft + ((PanelWidth + 300) * window.innerHeight) / 1080;

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
              style={{ width: PanelWidth + "rem" }}
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
                    small={PanelWidth <= 685}
                    large={PanelWidth >= 850}
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
                  style={{ left: PanelWidth + "rem" }}
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
