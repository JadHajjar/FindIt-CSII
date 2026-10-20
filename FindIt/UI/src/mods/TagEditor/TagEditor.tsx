import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import mod from "../../../mod.json";
import styles from "./tagEditor.module.scss";
import { BasicButton } from "mods/BasicButton/BasicButton";
import { TagListEditor } from "./TagListEditor";

const TagEditorPrefabId$ = bindValue<number>(mod.id, "TagEditorPrefabId");
const TagEditorPrefabName$ = bindValue<string>(mod.id, "TagEditorPrefabName");
const TagEditorTags$ = bindValue<string[]>(mod.id, "TagEditorTags");
const TagEditorCreatorTags$ = bindValue<string[]>(
  mod.id,
  "TagEditorCreatorTags",
);
const AllTags$ = bindValue<string[]>(mod.id, "AllTags");

export const TagEditorComponent = () => {
  const { translate } = useLocalization();
  const PrefabId = useValue(TagEditorPrefabId$);
  const PrefabName = useValue(TagEditorPrefabName$);
  const Tags = useValue(TagEditorTags$);
  const CreatorTags = useValue(TagEditorCreatorTags$);
  const AllTags = useValue(AllTags$);

  return (
    <div className={styles.tagEditor}>
      <div className={styles.header}>
        <div className={styles.title}>
          {translate("Tooltip.LABEL[FindIt.Tags]", "Tags")}
        </div>
        <BasicButton
          className={styles.closeButton}
          onClick={() => trigger(mod.id, "CloseTagEditor")}
          mask="Media/Glyphs/Close.svg"
        />
      </div>

      <div className={styles.prefabName}>{PrefabName}</div>

      <TagListEditor
        tags={Tags}
        lockedTags={CreatorTags}
        allTags={AllTags}
        resetKey={PrefabId}
        onAdd={(tag) => trigger(mod.id, "AddTag", tag)}
        onRemove={(tag) => trigger(mod.id, "RemoveTag", tag)}
        onEscape={() => trigger(mod.id, "CloseTagEditor")}
      />

      <div className={styles.searchHint}>
        {translate(
          "Tooltip.LABEL[FindIt.TagSearchHint]",
          "Type #tag in the search bar to only show assets with that tag.\nCombine several tags with text, for example: #park #bench modern",
        )
          ?.split("\n")
          .map((line) => <div key={line}>{line}</div>)}
      </div>
    </div>
  );
};
