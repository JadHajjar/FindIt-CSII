import iconStyles from "./findItIcon.module.scss";
import { useValue, trigger, bindValue } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";

// These contain the coui paths to Unified Icon Library svg assets
export const findItIconSrc =                         "coui://uil/Standard/Magnifier.svg";

// This functions trigger an event on C# side and C# designates the method to implement.
export const ShowFindItPanel$ =        bindValue<boolean> (mod.id, 'ShowFindItPanel');

export const GameMainScreneTheme: Theme | any = getModule(
    "game-ui/game/components/game-main-screen.module.scss",
    "classes"
  ); 

export const PanelTheme: Theme | any = getModule(
    "game-ui/common/panel/panel.module.scss",
    "classes"
)
  
export const FindItToolPanelComponent = () => {
    const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$); 


    if (!isPhotoMode && ShowFindItPanel) {
        return (
            <div className={GameMainScreneTheme.toolPanel}>
                <div className={PanelTheme.panel}>
                    <TopBarComponent></TopBarComponent>
                    <PrefabSelectionComponent></PrefabSelectionComponent>
                </div>
            </div>
                    
        );
    }
        
    return null;
    
}