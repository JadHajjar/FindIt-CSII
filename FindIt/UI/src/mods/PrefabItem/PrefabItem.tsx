import { trigger } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button, Tooltip } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { useEffect, useRef, useState } from "react";
import { PrefabEntry } from "domain/prefabEntry";
import classNames from "classnames";

export interface PrefabButtonProps {
  prefab: PrefabEntry;
  selected: boolean;
  showCategory: boolean;
  width: string;
}

export const PrefabItemComponent = (props: PrefabButtonProps) => {
  const { translate } = useLocalization();
  const [favoriteFlip, setFavorited] = useState(false);
  const [thumbnailIndex, setThumbnailIndex] = useState(0);

  const mouseOver = () => {
    setThumbnailIndex(Math.floor(Math.random() * props.prefab.thumbnails.length));
  };

  function SetCurrentPrefab(id: number) {
    trigger(mod.id, "SetCurrentPrefab", id);
  }

  function ToggleFavorited(id: number) {
    trigger(mod.id, "ToggleFavorited", id);
    props.prefab.favorited = !props.prefab.favorited;
    setFavorited(!favoriteFlip);
  }

  if (thumbnailIndex >= props.prefab.thumbnails.length) setThumbnailIndex(0);

  return (
    <>
      <Button
        className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, props.selected && styles.selected)}
        variant="icon"
        onSelect={() => SetCurrentPrefab(props.prefab.id)}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
        style={{ width: props.width, height: props.width }}
        onMouseEnter={props.prefab.random ? mouseOver : undefined}
      >
        <Tooltip
          tooltip={
            props.prefab.favorited
              ? translate("Tooltip.LABEL[FindIt.RemoveFavorite]", "Remove from favorites")
              : translate("Tooltip.LABEL[FindIt.AddFavorite]", "Favorite this asset")
          }
        >
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
            <img src={props.prefab.favorited ? "coui://uil/Colored/StarFilled.svg" : "coui://uil/Colored/StarOutline.svg"}></img>
          </Button>
        </Tooltip>

        <img
          src={props.prefab.thumbnails[thumbnailIndex]}
          onError={({ currentTarget }) => {
            currentTarget.onerror = null; // prevents looping
            currentTarget.src = props.prefab.fallbackThumbnail;
          }}
          className={VanillaComponentResolver.instance.assetGridTheme.thumbnail + " " + styles.gridThumbnail}
          onMouseEnter={props.prefab.random ? mouseOver : undefined}
        ></img>

        <div className={styles.gridItemText}>
          <p>{props.prefab.name}</p>
        </div>

        <div className={styles.rightSideContainer}>
          {props.showCategory && <img src={props.prefab.categoryThumbnail}></img>}

          {props.prefab.dlcThumbnail && <img src={props.prefab.dlcThumbnail}></img>}

          {props.prefab.random && <img src="coui://uil/Colored/Dice.svg"></img>}
        </div>
      </Button>
    </>
  );
};
