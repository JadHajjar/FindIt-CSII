import { trigger, bindValue, useValue } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { useState } from "react";

export interface PrefabButtonProps {
  id: number;
  src: string;
  text: string;
  favorited: boolean;
  selected?: boolean;
}

// These establishes the binding with C# side.

export const PrefabItemComponent = (props: PrefabButtonProps) => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  // translation handling. Translates using locale keys that are defined in C# or fallback string here.
  const { translate } = useLocalization();

  const [favorited, setFavorited] = useState(props.favorited);

  function SetCurrentPrefab(id: number) {
    trigger(mod.id, "SetCurrentPrefab", id);
  }

  function ToggleFavorited(id: number) {
    setFavorited(!favorited);
    trigger(mod.id, "ToggleFavorited", id);
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
      onSelect={() => SetCurrentPrefab(props.id)}
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
          styles.favoriteIcon +
          (favorited ? " " + styles.favorited : "")
        }
        variant="icon"
        onSelect={() => ToggleFavorited(props.id)}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      >
        <img
          src={
            favorited
              ? "coui://uil/Colored/StarFilled.svg"
              : "coui://uil/Colored/StarOutline.svg"
          }
        ></img>
      </Button>
    </Button>
  );
};
