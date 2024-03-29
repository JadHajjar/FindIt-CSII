import iconStyles from "./findItIcon.module.scss";
import { useValue, trigger } from "cs2/api";
import { game, PrefabRequirement, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";

// These contain the coui paths to Unified Icon Library svg assets
export const findItIconSrc =                         "coui://uil/Standard/Magnifier.svg";

export const ToolBarButtonTheme: Theme | any = getModule(
    "game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss",
    "classes"
  ); 

export function handleClick() {
    trigger(mod.id, "FintItIconToggled");
}

export const FindItIconComponent = () => {

    const activeGamePanel = useValue(game.activeGamePanel$);
    const isPhotoMode : boolean = activeGamePanel?.__Type == game.GamePanelType.PhotoMode;

    if (!isPhotoMode) {
        return (
            <>
                <Button 
                    src={findItIconSrc} 
                    className ={ToolBarButtonTheme.button + " " + iconStyles.findItToolbarIcon} 
                    variant="icon"
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    >
                </Button>
            </>
                    
        );
    }
        
    return null;
    
}