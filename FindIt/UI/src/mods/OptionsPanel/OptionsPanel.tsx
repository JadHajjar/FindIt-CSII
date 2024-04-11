import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { Button, Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/bindings";
import styles from "./OptionsPanel.module.scss";
import { useEffect, useRef, useState } from "react";
import { optionSection, optionItem } from "../../domain/ContentViewType";
import { PrefabCategory } from "../../domain/category";
import { PrefabSubCategory } from "../../domain/subCategory";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";

const ToolOptionsTheme: Theme | any = getModule(
  "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss",
  "classes"
);

const OptionsList$ = bindValue<optionSection[]>(mod.id, "OptionsList");

export const OptionsPanelComponent = () => {
  const OptionsList = useValue(OptionsList$);

  return (
    <>
      {OptionsList.map((section) => (
        <div className={ToolOptionsTheme.item}>
          <div className={ToolOptionsTheme.itemContent}>
            <div className={ToolOptionsTheme.label}>{section.name}</div>
            <div className={ToolOptionsTheme.content}>
              {section.options.map((option) => (
                <VanillaComponentResolver.instance.ToolButton
                  selected={option.selected}
                  onSelect={() =>
                    trigger(mod.id, "OptionClicked", section.id, option.id)
                  }
                  src={option.icon}
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                  className={
                    VanillaComponentResolver.instance.toolButtonTheme.button
                  }
                ></VanillaComponentResolver.instance.ToolButton>
              ))}
            </div>
          </div>
        </div>
      ))}
    </>
  );
};
