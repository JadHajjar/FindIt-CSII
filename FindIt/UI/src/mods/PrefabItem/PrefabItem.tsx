import { trigger, bindValue, useValue } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { Entity, prefab } from "cs2/bindings";
import { entityEquals } from "cs2/utils";

// This functions trigger an event on C# side and C# designates the method to implement.
export function changePrefab(prefabEntity : Entity) {
  trigger(mod.id, "PrefabChange", prefabEntity);
}

// These establishes the binding with C# side.
export const ActivePrefabEntity$ =        bindValue<Entity> (mod.id, 'ActivePrefabEntity');

export const PrefabItemComponent = (prefabEntity : Entity) => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const prefabDetails = prefab.prefabDetails$.getValue(prefabEntity);
  const ActivePrefabEntity = useValue(ActivePrefabEntity$); 
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  // These back up ideas aren't working yet. 
  var titleId = prefabDetails?.titleId;
  if (titleId == undefined) {
    titleId = "No Id found";
  }
  var label : string | undefined | null = translate(titleId, prefabDetails?.name);

  if (label == "" || label == null) {
    label = prefabDetails?.name;
  }
  var iconPath = prefabDetails?.icon;
  if (iconPath == "") {
    iconPath = "coui://uil/Standard/Magnifier.svg";
  }

  return (
    <Button
      className={
        VanillaComponentResolver.instance.assetGridTheme.item +
        " " +
        styles.gridItem
      }
      selected={entityEquals(prefabEntity, ActivePrefabEntity)}
      variant="icon"
      onSelect={() => changePrefab(prefabEntity)}
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
    >
      <img
        src={iconPath}
        className={
          VanillaComponentResolver.instance.assetGridTheme.thumbnail +
          " " +
          styles.gridThumbnail
        }
      ></img>

      <div className={styles.gridItemText}>
        <p>{label}</p>
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
