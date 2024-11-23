import { trigger } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button, FOCUS_DISABLED, Tooltip } from "cs2/ui";
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
    const date = new Date();

    setThumbnailIndex(Math.floor(date.getSeconds() * 10 + date.getMilliseconds() / 250) % props.prefab.thumbnails.length);
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
        className={classNames(
          VanillaComponentResolver.instance.assetGridTheme.item,
          styles.gridItem,
          props.selected && styles.selected,
          props.prefab.favorited && styles.favorited
        )}
        variant="icon"
        onSelect={() => SetCurrentPrefab(props.prefab.id)}
        style={{ width: props.width, height: props.width }}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
        onMouseMove={props.prefab.random ? mouseOver : undefined}
      >
        <Tooltip
          tooltip={
            props.prefab.favorited
              ? translate("Tooltip.LABEL[FindIt.RemoveFavorite]", "Remove from favorites")
              : translate("Tooltip.LABEL[FindIt.AddFavorite]", "Favorite this asset")
          }
        >
          <Button
            className={styles.favoriteIcon}
            variant="icon"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            onSelect={() => ToggleFavorited(props.prefab.id)}
          >
            <img src={props.prefab.favorited ? "coui://uil/Colored/StarFilled.svg" : "coui://uil/Colored/StarOutline.svg"} />
          </Button>
        </Tooltip>

        {props.prefab.placed && (
          <Tooltip tooltip={translate("Tooltip.LABEL[FindIt.Locate]", "Locate")}>
            <Button
              className={styles.placedMarker}
              variant="icon"
              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
              onSelect={() => trigger(mod.id, "OnLocateButtonClicked", props.prefab.id)}
            >
              <img src="Media/Game/Icons/MapMarker.svg" />
            </Button>
          </Tooltip>
        )}

        <img
          src={props.prefab.thumbnails[thumbnailIndex]}
          onError={({ currentTarget }) => {
            currentTarget.onerror = null; // prevents looping
            currentTarget.src = props.prefab.fallbackThumbnail;
          }}
          className={VanillaComponentResolver.instance.assetGridTheme.thumbnail + " " + styles.gridThumbnail}
          onMouseEnter={props.prefab.random ? mouseOver : undefined}
        />

        <div className={styles.gridItemText}>
          <p>{props.prefab.name}</p>
        </div>

        <div className={styles.rightSideContainer}>
          {props.showCategory && <img src={props.prefab.categoryThumbnail} />}

          {props.prefab.dlcThumbnail && <img src={props.prefab.dlcThumbnail} />}

          {props.prefab.random && <img src="coui://uil/Colored/Dice.svg" />}

          {props.prefab.themeThumbnail && <img src={props.prefab.themeThumbnail} />}

          {props.prefab.packThumbnails && props.prefab.packThumbnails.map((x) => <img src={x} />)}
        </div>
      </Button>
    </>
  );
};
