import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import styles from "./OptionsPanel.module.scss";
import { optionSection, optionItem } from "../../domain/ContentViewType";
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
        <div className={styles.optionRow}>
          <div className={styles.optionSection}>
            <div className={styles.optionLabel}>{section.name}</div>
            <div className={styles.optionContent}>
              {section.options.map((option) =>
                option.isValue ? (
                  <>
                    <VanillaComponentResolver.instance.ToolButton
                      onSelect={() =>
                        trigger(
                          mod.id,
                          "OptionClicked",
                          section.id,
                          option.id,
                          -1
                        )
                      }
                      src="Media/Glyphs/ThickStrokeArrowDown.svg"
                      focusKey={
                        VanillaComponentResolver.instance.FOCUS_DISABLED
                      }
                      className={
                        VanillaComponentResolver.instance.toolButtonTheme
                          .button +
                        " " +
                        VanillaComponentResolver.instance.mouseToolOptionsTheme
                          .startButton
                      }
                    ></VanillaComponentResolver.instance.ToolButton>

                    <div
                      className={
                        VanillaComponentResolver.instance.mouseToolOptionsTheme
                          .numberField
                      }
                    >
                      {option.value}
                    </div>

                    <VanillaComponentResolver.instance.ToolButton
                      onSelect={() =>
                        trigger(
                          mod.id,
                          "OptionClicked",
                          section.id,
                          option.id,
                          1
                        )
                      }
                      src="Media/Glyphs/ThickStrokeArrowUp.svg"
                      focusKey={
                        VanillaComponentResolver.instance.FOCUS_DISABLED
                      }
                      className={
                        VanillaComponentResolver.instance.toolButtonTheme
                          .button +
                        " " +
                        VanillaComponentResolver.instance.mouseToolOptionsTheme
                          .endButton
                      }
                    ></VanillaComponentResolver.instance.ToolButton>
                  </>
                ) : (
                  <VanillaComponentResolver.instance.ToolButton
                    selected={option.selected}
                    onSelect={() =>
                      trigger(mod.id, "OptionClicked", section.id, option.id, 0)
                    }
                    src={option.icon}
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    className={
                      VanillaComponentResolver.instance.toolButtonTheme.button +
                      " " +
                      styles.singleButton
                    }
                  ></VanillaComponentResolver.instance.ToolButton>
                )
              )}
            </div>
          </div>
        </div>
      ))}
    </>
  );
};
