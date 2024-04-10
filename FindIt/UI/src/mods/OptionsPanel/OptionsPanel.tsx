import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button, Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./OptionsPanel.module.scss";
import { useEffect, useRef, useState } from "react";
import { ContentViewType } from "../../domain/ContentViewType";
import { PrefabCategory } from "../../domain/category";
import { PrefabSubCategory } from "../../domain/subCategory";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";

export interface TopBarProps {
  viewType: ContentViewType;
  setViewType: (viewType: ContentViewType) => void;
}

export const OptionsPanelComponent = (props: TopBarProps) => {
  return (
    <>
      <VanillaComponentResolver.instance.ToolButton
        selected={false}
        onSelect={() =>
          props.setViewType(
            props.viewType == ContentViewType.GridNoText
              ? ContentViewType.GridWithText
              : ContentViewType.GridNoText
          )
        }
        src="coui://uil/Standard/XClose.svg"
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
        className={VanillaComponentResolver.instance.toolButtonTheme.button}
      ></VanillaComponentResolver.instance.ToolButton>
    </>
  );
};
