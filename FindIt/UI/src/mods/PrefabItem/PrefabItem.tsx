import { bindValue, trigger, useValue } from "cs2/api";
import styles from "./prefabItem.module.scss";
import { Button, FOCUS_DISABLED, Tooltip } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useLocalization } from "cs2/l10n";
import { useEffect, useRef, useState } from "react";
import { PrefabEntry } from "domain/prefabEntry";
import classNames from "classnames";
import { BasicButton } from "mods/BasicButton/BasicButton";
import shuffle from "images/findit_shuffle.svg";

export interface PrefabButtonProps {
  noAssetImage: any;
  listView: any;
  prefab: PrefabEntry;
  selected: boolean;
  showCategory: boolean;
  width: string;
}

export const PrefabItemComponent = (props: PrefabButtonProps) => {
  const { translate } = useLocalization();
  const [favoriteFlip, setFavorited] = useState(false);
  const [hovered, setHovered] = useState(false);
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
        className={classNames(styles.gridItem, props.selected && styles.selected, props.prefab.favorited && styles.favorited)}
        variant="icon"
        onSelect={() => SetCurrentPrefab(props.prefab.id)}
        style={{ width: props.width, height: props.width }}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
        onMouseMove={props.prefab.random ? mouseOver : undefined}
        onMouseEnter={() => setHovered(true)}
        onMouseLeave={() => setHovered(false)}
      >
        {!props.listView && renderLeftSection(false)}

        <div className={styles.gridThumbnail}>
          {props.noAssetImage && !hovered ? (
            <img src={props.prefab.fallbackThumbnail} />
          ) : (
            <img
              src={props.prefab.thumbnails[thumbnailIndex]}
              onError={({ currentTarget }) => {
                currentTarget.onerror = null; // prevents looping
                currentTarget.src = props.prefab.fallbackThumbnail;
              }}
              className={styles.gridThumbnail}
              onMouseMove={props.prefab.random ? mouseOver : undefined}
            />
          )}
          {props.prefab.random && <img className={styles.shuffle} src={shuffle} />}
        </div>

        <div className={styles.gridItemText}>
          <p>{props.prefab.name}</p>
        </div>

        {props.listView && renderLeftSection(true)}

        {renderRightSection()}
      </Button>
    </>
  );

  function renderRightSection() {
    return (
      <div className={classNames(styles.rightSideContainer, styles.buttonsSection)}>
        {props.showCategory && !props.noAssetImage && <img src={props.prefab.categoryThumbnail} />}

        {props.prefab.dlcThumbnail && <img src={props.prefab.dlcThumbnail} />}

        {props.prefab.themeThumbnail && <img src={props.prefab.themeThumbnail} />}

        {props.prefab.packThumbnails && props.prefab.packThumbnails.map((x) => <img src={x} />)}
      </div>
    );
  }

  function renderLeftSection(addDiv: boolean) {
    return (
      <div className={classNames(styles.leftSideContainer, styles.buttonsSection)}>
        {addDiv && (props.prefab.placed || props.prefab.pdxId > 0) && <div className={styles.seperator} />}

        {props.prefab.pdxId > 0 && (
          <BasicButton
            tooltip={translate("Tooltip.LABEL[FindIt.ViewOnPdxMods]", "View on PdxMods")}
            className={styles.pdxModsButton}
            onClick={() => trigger(mod.id, "OnPdxModsButtonClicked", props.prefab.id)}
            src="assetdb://gameui/Media/Glyphs/ParadoxModsCloud.svg"
          />
        )}

        {props.prefab.placed && (
          <BasicButton
            tooltip={translate("Tooltip.LABEL[FindIt.Locate]", "Locate")}
            className={styles.placedMarker}
            onClick={() => trigger(mod.id, "OnLocateButtonClicked", props.prefab.id)}
            src="Media/Game/Icons/MapMarker.svg"
          />
        )}

        <BasicButton
          tooltip={
            props.prefab.favorited
              ? translate("Tooltip.LABEL[FindIt.RemoveFavorite]", "Remove from favorites")
              : translate("Tooltip.LABEL[FindIt.AddFavorite]", "Favorite this asset")
          }
          className={styles.favoriteIcon}
          onClick={() => ToggleFavorited(props.prefab.id)}
          src={props.prefab.favorited ? "coui://uil/Colored/StarFilled.svg" : "coui://uil/Colored/StarOutline.svg"}
        />
      </div>
    );
  }
};
