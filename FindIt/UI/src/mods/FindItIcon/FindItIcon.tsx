import iconStyles from "./findItIcon.module.scss";
import { useValue, trigger, bindValue } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";

// These contain the coui paths to Unified Icon Library svg assets
export const findItIconSrc =                         "coui://uil/Standard/Magnifier.svg";

// This functions trigger an event on C# side and C# designates the method to implement.
export const ShowFindItPanel$ =        bindValue<boolean> (mod.id, 'ShowFindItPanel');

export const ToolBarButtonTheme: Theme | any = getModule(
    "game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss",
    "classes"
  ); 

export function handleClick() {
    trigger(mod.id, "FindItIconToggled");
}

export const FindItIconComponent = () => {
    const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$); 


    if (!isPhotoMode) {
        return (
            <>
                <Button 
                    src={findItIconSrc} 
                    className ={ToolBarButtonTheme.button + " " + iconStyles.findItToolbarIcon} 
                    variant="icon"
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    selected={ShowFindItPanel}
                    onSelect={handleClick}
                    >
                </Button>
            </>
                    
        );
    }
        
    return null;
    
}