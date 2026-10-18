import { bindValue, trigger, useValue } from "cs2/api";
import { Button, Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { useEffect, useState } from "react";
import classNames from "classnames";
import mod from "../../../mod.json";
import styles from "./creatorProperties.module.scss";
import tagStyles from "../TagEditor/tagEditor.module.scss";
import { BasicButton } from "mods/BasicButton/BasicButton";
import { TagListEditor } from "mods/TagEditor/TagListEditor";
import { PrefabCategory } from "domain/category";
import { PrefabSubCategory } from "domain/subCategory";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";

const IsOpen$ = bindValue<boolean>(mod.id, "CreatorIsOpen");
const ShowInfo$ = bindValue<boolean>(mod.id, "CreatorShowInfo");
const PrefabName$ = bindValue<string>(mod.id, "CreatorPrefabName");
const UseComponent$ = bindValue<boolean>(mod.id, "CreatorUseComponent");
const Tags$ = bindValue<string[]>(mod.id, "CreatorTags");
const AllTags$ = bindValue<string[]>(mod.id, "CreatorAllTags");
const SubCategory$ = bindValue<number>(mod.id, "CreatorSubCategory");
const HideFromFindIt$ = bindValue<boolean>(mod.id, "CreatorHideFromFindIt");
const Categories$ = bindValue<PrefabCategory[]>(mod.id, "CreatorCategories");
const SubCategories$ = bindValue<PrefabSubCategory[]>(
  mod.id,
  "CreatorSubCategories",
);

const categoryOf = (id: number) => Math.floor(id / 100) * 100;

export const CreatorPropertiesComponent = () => {
  const { translate } = useLocalization();
  const IsOpen = useValue(IsOpen$);
  const ShowInfo = useValue(ShowInfo$);
  const PrefabName = useValue(PrefabName$);
  const UseComponent = useValue(UseComponent$);
  const Tags = useValue(Tags$);
  const AllTags = useValue(AllTags$);
  const SubCategory = useValue(SubCategory$);
  const HideFromFindIt = useValue(HideFromFindIt$);
  const Categories = useValue(Categories$);
  const SubCategories = useValue(SubCategories$);
  const [selectedCategory, setSelectedCategory] = useState(0);

  useEffect(() => {
    setSelectedCategory(SubCategory ? categoryOf(SubCategory) : 0);
  }, [SubCategory, PrefabName]);

  if (!IsOpen) {
    if (!ShowInfo) return null;

    const dismiss = () => trigger(mod.id, "CreatorDismissInfo");

    return (
      <div className={styles.container}>
        <div className={styles.panel}>
          <div className={tagStyles.header}>
            <div className={styles.title}>
              {translate(
                "Tooltip.LABEL[FindIt.CreatorProperties]",
                "Find It Properties",
              )}
            </div>
            <BasicButton
              className={tagStyles.closeButton}
              onClick={dismiss}
              mask="Media/Glyphs/Close.svg"
            />
          </div>

          <div className={styles.info}>
            {translate(
              "Tooltip.LABEL[FindIt.CreatorNoAsset]",
              "Select the asset you're editing first, in the scene or the hierarchy, then click this button again to edit its Find It properties.",
            )}
          </div>

          <Button
            variant="flat"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            className={styles.doneButton}
            onSelect={dismiss}
          >
            {translate("Tooltip.LABEL[FindIt.CreatorOk]", "OK")}
          </Button>
        </div>
      </div>
    );
  }

  const close = () => trigger(mod.id, "CreatorClose");

  return (
    <div className={styles.container}>
      <div className={styles.panel}>
        <div className={tagStyles.header}>
          <div className={styles.title}>
            {translate(
              "Tooltip.LABEL[FindIt.CreatorProperties]",
              "Find It Properties",
            )}
          </div>
          <BasicButton
            className={tagStyles.closeButton}
            onClick={close}
            mask="Media/Glyphs/Close.svg"
          />
        </div>

        <div className={styles.prefabName}>{PrefabName}</div>

        <div className={styles.modeToggle}>
          <Button
            variant="flat"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            className={classNames(UseComponent && styles.selected)}
            onSelect={() => trigger(mod.id, "CreatorSetUseComponent", true)}
          >
            {translate("Tooltip.LABEL[FindIt.CreatorStandard]", "Standard")}
          </Button>
          <Button
            variant="flat"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            className={classNames(!UseComponent && styles.selected)}
            onSelect={() => trigger(mod.id, "CreatorSetUseComponent", false)}
          >
            {translate(
              "Tooltip.LABEL[FindIt.CreatorCompatibility]",
              "More Compatibility",
            )}
          </Button>
        </div>

        <div className={styles.hint}>
          {UseComponent
            ? translate(
                "Tooltip.LABEL[FindIt.CreatorStandardHint]",
                "Saved in the FindItProperties component, with every option available. If the asset is edited and saved again without Find It enabled, these properties will be lost.",
              )
            : translate(
                "Tooltip.LABEL[FindIt.CreatorCompatibilityHint]",
                "Saved as '#' entries in the Editor Asset Category Override exclude list. Only tags and the category are available, but they're kept even if the asset is edited without Find It.",
              )}
        </div>

        <div className={styles.section}>
          {translate("Tooltip.LABEL[FindIt.Tags]", "Tags")}
        </div>
        <TagListEditor
          tags={Tags}
          allTags={AllTags}
          resetKey={PrefabName}
          onAdd={(tag) => trigger(mod.id, "CreatorAddTag", tag)}
          onRemove={(tag) => trigger(mod.id, "CreatorRemoveTag", tag)}
          onEscape={close}
          subtitleClassName={styles.subsection}
          hintClassName={styles.chipHint}
          chipListClassName={styles.chipList}
        />

        <div className={styles.section}>
          {translate("Tooltip.LABEL[FindIt.CreatorCategory]", "Category")}
        </div>
        <div className={styles.iconRow}>
          <Button
            variant="flat"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            className={classNames(
              styles.textButton,
              !SubCategory && styles.selected,
            )}
            onSelect={() => {
              setSelectedCategory(0);
              trigger(mod.id, "CreatorSetSubCategory", 0);
            }}
          >
            {translate(
              "Tooltip.LABEL[FindIt.CreatorAutomatic]",
              "Automatic",
            )}
          </Button>
          {Categories.map((category) => (
            <Tooltip key={category.id} tooltip={category.toolTip}>
              <Button
                variant="flat"
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                className={classNames(
                  styles.iconButton,
                  selectedCategory === category.id && styles.selected,
                )}
                onSelect={() => setSelectedCategory(category.id)}
              >
                <img src={category.icon} />
              </Button>
            </Tooltip>
          ))}
        </div>
        {selectedCategory !== 0 && (
          <div className={styles.iconRow}>
            {SubCategories.filter(
              (x) => categoryOf(x.id) === selectedCategory,
            ).map((subCategory) => (
              <Tooltip key={subCategory.id} tooltip={subCategory.toolTip}>
                <Button
                  variant="flat"
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                  className={classNames(
                    styles.iconButton,
                    SubCategory === subCategory.id && styles.selected,
                  )}
                  onSelect={() =>
                    trigger(
                      mod.id,
                      "CreatorSetSubCategory",
                      subCategory.id,
                    )
                  }
                >
                  <img src={subCategory.icon} />
                </Button>
              </Tooltip>
            ))}
          </div>
        )}

        {UseComponent && (
          <Button
            variant="flat"
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            className={styles.checkboxRow}
            onSelect={() =>
              trigger(mod.id, "CreatorSetHideFromFindIt", !HideFromFindIt)
            }
          >
            <div
              className={classNames(
                styles.checkbox,
                HideFromFindIt && styles.checked,
              )}
            >
              {HideFromFindIt && (
                <img
                  style={{ maskImage: "url(Media/Glyphs/Checkmark.svg)" }}
                />
              )}
            </div>
            <span>
              {translate(
                "Tooltip.LABEL[FindIt.CreatorHideFromFindIt]",
                "Hide this asset from Find It",
              )}
            </span>
          </Button>
        )}

        <Button
          variant="flat"
          focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          className={styles.doneButton}
          onSelect={close}
        >
          {translate("Tooltip.LABEL[FindIt.CreatorDone]", "Done")}
        </Button>
      </div>
    </div>
  );
};
