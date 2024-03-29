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

// This functions trigger an event on C# side and C# designates the method to implement.
export function changePrefab(prefab: string) {
  trigger(mod.id, eventName, prefab);
}

// These establishes the binding with C# side. Without C# side game ui will crash.
// export const ActivePrefabName$ =        bindValue<string> (mod.id, 'ActivePrefabName');

// defines trigger event names.
export const eventName = "PrefabChange";
export const streamPrefab = "WaterSource Stream";
export const lakePrefab = "WaterSource Lake";
export const riverPrefab = "WaterSource River";
export const seaPrefab = "WaterSource Sea";

export const PrefabSelectionComponent = () => {
  // These get the value of the bindings.
  var ActivePrefabName = streamPrefab;

  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  const UnSelectedButtonTheme =
    VanillaComponentResolver.instance.assetGridTheme.item +
    " " +
    styles.gridItem;
  const SelectedButtonTheme = UnSelectedButtonTheme + " selected";

  var buttons = [];

  for (var i = 0; i < 25; i++) {
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

  return (
    <>
      {
        <Scrollable
          vertical={true}
          trackVisibility="always"
          className={styles.scrollableContainer}
        >
          <div className={styles.panelSection}>{buttons}</div>
        </Scrollable>
      }
    </>
  );
};
