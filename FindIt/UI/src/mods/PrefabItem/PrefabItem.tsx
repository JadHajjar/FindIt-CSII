import { trigger } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { Entity } from "cs2/bindings";

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

export interface IconButtonProps {
  src: string | undefined;
  text: string | undefined;
  selected?: boolean;
  prefabEntity? : Entity; // This is new.
}

export const PrefabItemComponent = (props: IconButtonProps) => {
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  return (
    <Button
      className={
        VanillaComponentResolver.instance.assetGridTheme.item +
        " " +
        styles.gridItem
      }
      selected={props.selected}
      variant="icon"
      onSelect={() => changePrefab(seaPrefab)}
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
    >
      <img
        src={props.src}
        className={
          VanillaComponentResolver.instance.assetGridTheme.thumbnail +
          " " +
          styles.gridThumbnail
        }
      ></img>

      <div className={styles.gridItemText}>
        <p>{props.text}</p>
      </div>

      <Button
        className={
          VanillaComponentResolver.instance.assetGridTheme.item +
          " " +
          styles.favoriteIcon
        }
        variant="icon"
        onSelect={() => {}}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      >
        <img src="coui://uil/Colored/StarOutline.svg"></img>
      </Button>
    </Button>
  );
};
