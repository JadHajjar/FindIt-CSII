import { trigger } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { useState } from "react";
import { PrefabEntry } from "domain/prefabEntry";

export interface PrefabButtonProps {
  prefab: PrefabEntry;
  selected: boolean;
  showCategory: boolean;
}

// These establishes the binding with C# side.

export const PrefabItemComponent = (props: PrefabButtonProps) => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  const [favoriteFlip, setFavorited] = useState(false);

  function SetCurrentPrefab(id: number) {
    trigger(mod.id, "SetCurrentPrefab", id);
  }

  function ToggleFavorited(id: number) {
    trigger(mod.id, "ToggleFavorited", id);
    props.prefab.favorited = !props.prefab.favorited;
    setFavorited(!favoriteFlip);
  }

  return (
    <Button
      className={
        VanillaComponentResolver.instance.assetGridTheme.item +
        " " +
        styles.gridItem
      }
      selected={props.selected}
      variant="icon"
      onSelect={() => SetCurrentPrefab(props.prefab.id)}
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
    >
      <img
        src={props.prefab.thumbnail}
        onError={({ currentTarget }) => {
          currentTarget.onerror = null; // prevents looping
          currentTarget.src = props.prefab.fallbackThumbnail;
        }}
        className={
          VanillaComponentResolver.instance.assetGridTheme.thumbnail +
          " " +
          styles.gridThumbnail +
          " " +
          (props.prefab.thumbnail.endsWith(".jpeg") ||
          props.prefab.thumbnail.endsWith(".jpg")
            ? styles.jpgThumb
            : null)
        }
      ></img>

      <div className={styles.gridItemText}>
        <p>{props.prefab.name}</p>
      </div>

      <Button
        className={
          VanillaComponentResolver.instance.assetGridTheme.item +
          " " +
          styles.favoriteIcon +
          (props.prefab.favorited ? " " + styles.favorited : "")
        }
        variant="icon"
        onSelect={() => ToggleFavorited(props.prefab.id)}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      >
        <img
          src={
            props.prefab.favorited
              ? "coui://uil/Colored/StarFilled.svg"
              : "coui://uil/Colored/StarOutline.svg"
          }
        ></img>
      </Button>

      <div className={styles.rightSideContainer}>
        {props.showCategory && (
          <img
            src={props.prefab.categoryThumbnail}
            className={styles.categoryThumbnail}
          ></img>
        )}

        {props.prefab.dlcThumbnail && (
          <img
            src={props.prefab.dlcThumbnail}
            className={styles.dlcThumbnail}
          ></img>
        )}
      </div>
    </Button>
  );
};
