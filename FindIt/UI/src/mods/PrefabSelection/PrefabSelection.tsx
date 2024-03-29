import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import styles from "./prefabSelection.module.scss";
import { Button, Panel, Portal, Scrollable } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import StreamSrc from "./WaterSourceStream.svg";
import LakeSrc from "./WaterSourceLake.svg";
import RiverSrc from "./WaterSourceRiver.svg";
import SeaSrc from "./WaterSourceSea.svg";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";
import { ModuleRegistryExtend } from "cs2/modding";

// This functions trigger an event on C# side and C# designates the method to implement.
export function changePrefab(prefab: string) {
  trigger(mod.id, eventName, prefab);
}

// These establishes the binding with C# side.
export const ActivePrefabName$ =        bindValue<string> (mod.id, 'ActivePrefabName');
export const ShowFindItPanel$ =        bindValue<boolean> (mod.id, 'ShowFindItPanel');

// defines trigger event names.
export const eventName = "PrefabChange";
export const streamPrefab = "WaterSource Stream";
export const lakePrefab = "WaterSource Lake";
export const riverPrefab = "WaterSource River";
export const seaPrefab = "WaterSource Sea";

export const PrefabSelectionComponent : ModuleRegistryExtend = (Component) => 
{
  // I believe you should not put anything here.
  return (props) => 
  {
    const {children, ...otherProps} = props || {};
    
    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$); 
    var ActivePrefabName = streamPrefab;  // To be replaced with sseValue(ActivePrefabName$); and can be const.

    // translation handling. Translates using locale keys that are defined in C# or fallback string here.
    const { translate } = useLocalization();

    const UnSelectedButtonTheme =
      VanillaComponentResolver.instance.assetGridTheme.item +
      " " +
      styles.gridItem;
    const SelectedButtonTheme = UnSelectedButtonTheme + " selected";

    var buttons = [];

    for (var i = 0; i < 25; i++) 
    {
      buttons.push(
        <PrefabItemComponent
          src={StreamSrc}
          text="test"
          selected={true}
        ></PrefabItemComponent>
      );
      buttons.push(
        <PrefabItemComponent
          src={RiverSrc}
          text="longer informational text"
        ></PrefabItemComponent>
      );
      buttons.push(
        <PrefabItemComponent src={LakeSrc} text="abc"></PrefabItemComponent>
      );
      buttons.push(
        <PrefabItemComponent src={SeaSrc} text=""></PrefabItemComponent>
      );
    }

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!ShowFindItPanel) 
    {
      return (
        <Component {...otherProps}>
                {children}
        </Component>
      );
    }

    return (
      <>
          <Scrollable
            vertical={true}
            trackVisibility="always"
            className={styles.scrollableContainer}
          >
            <div className={styles.panelSection}>{buttons}</div>
          </Scrollable>
      </>
    );
  }
};
