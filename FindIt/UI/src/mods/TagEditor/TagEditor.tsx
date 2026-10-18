import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import { Button, Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { FOCUS_DISABLED } from "cs2/input";
import { useEffect, useRef, useState } from "react";
import classNames from "classnames";
import mod from "../../../mod.json";
import styles from "./tagEditor.module.scss";
import { BasicButton } from "mods/BasicButton/BasicButton";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";

const TextInput = getModule(
  "game-ui/common/input/text/text-input.tsx",
  "TextInput",
);

const TextInputTheme: Theme | any = getModule(
  "game-ui/editor/widgets/item/editor-item.module.scss",
  "classes",
);

const TagEditorPrefabId$ = bindValue<number>(mod.id, "TagEditorPrefabId");
const TagEditorPrefabName$ = bindValue<string>(mod.id, "TagEditorPrefabName");
const TagEditorTags$ = bindValue<string[]>(mod.id, "TagEditorTags");
const TagEditorCreatorTags$ = bindValue<string[]>(
  mod.id,
  "TagEditorCreatorTags",
);
const AllTags$ = bindValue<string[]>(mod.id, "AllTags");

const MaxSuggestions = 40;

export const TagEditorComponent = () => {
  const { translate } = useLocalization();
  const PrefabId = useValue(TagEditorPrefabId$);
  const PrefabName = useValue(TagEditorPrefabName$);
  const CreatorTags = useValue(TagEditorCreatorTags$);
  const creatorTagSet = new Set(CreatorTags.map((x) => x.toLowerCase()));
  const Tags = useValue(TagEditorTags$).filter(
    (x) => !creatorTagSet.has(x.toLowerCase()),
  );
  const AllTags = useValue(AllTags$);
  const [text, setText] = useState("");
  const inputRef = useRef(null);

  useEffect(() => {
    setText("");
    (inputRef.current as any)?.focus();
  }, [PrefabId]);

  const tagText = text.trim().replace(/^#+/, "");
  const query = tagText.toLowerCase();
  const assignedTags = new Set(
    CreatorTags.concat(Tags).map((x) => x.toLowerCase()),
  );
  const canCreate =
    query !== "" && !AllTags.some((x) => x.toLowerCase() === query);
  const suggestions = AllTags.filter(
    (x) =>
      !assignedTags.has(x.toLowerCase()) &&
      (query === "" || x.toLowerCase().includes(query)),
  )
    .sort(
      (a, b) =>
        Number(!a.toLowerCase().startsWith(query)) -
        Number(!b.toLowerCase().startsWith(query)),
    )
    .slice(0, MaxSuggestions);

  const addTags = (value: string) => {
    value
      .split(",")
      .map((x) => x.trim())
      .filter((x) => x !== "")
      .forEach((x) => trigger(mod.id, "AddTag", x));

    setText("");
  };

  const handleChange = (event: Event) => {
    const value = (event?.target as HTMLTextAreaElement | null)?.value ?? "";

    if (value.includes("\n")) {
      addTags(value.replace(/\n/g, ""));
    } else {
      setText(value.replace(/\s+/g, "-"));
    }
  };

  const handleKeyDown = (event: KeyboardEvent) => {
    if (event.key === "Enter") {
      event.preventDefault();
      addTags(text);
    } else if (event.key === "Escape") {
      trigger(mod.id, "CloseTagEditor");
    }
  };

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

      <div className={styles.chipList}>
        {Tags.length === 0 && CreatorTags.length === 0 && (
          <span className={styles.hint}>
            {translate(
              "Tooltip.LABEL[FindIt.NoTags]",
              "This asset has no tags yet.",
            )}
          </span>
        )}
        {CreatorTags.map((tag) => (
          <Tooltip
            key={tag}
            tooltip={translate(
              "Tooltip.LABEL[FindIt.CreatorTag]",
              "Added by the asset's creator, it can't be removed.",
            )}
          >
            <div className={classNames(styles.chip, styles.creator)}>
              <img style={{ maskImage: "url(Media/Glyphs/Lock.svg)" }} />
              <span>{tag}</span>
            </div>
          </Tooltip>
        ))}
        {Tags.map((tag) => (
          <div key={tag} className={classNames(styles.chip, styles.assigned)}>
            <span>{tag}</span>
            <BasicButton
              tooltip={translate("Tooltip.LABEL[FindIt.RemoveTag]", "Remove tag")}
              className={styles.removeButton}
              onClick={() => trigger(mod.id, "RemoveTag", tag)}
              mask="Media/Glyphs/Close.svg"
            />
          </div>
        ))}
      </div>

      <div className={styles.inputRow}>
        <TextInput
          ref={inputRef}
          multiline={1}
          value={text}
          disabled={false}
          type="text"
          className={classNames(TextInputTheme.input, styles.textBox)}
          focusKey={FOCUS_DISABLED}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder={translate(
            "Tooltip.LABEL[FindIt.AddTagPlaceholder]",
            "Add a tag...",
          )}
        ></TextInput>
        <BasicButton
          tooltip={translate("Tooltip.LABEL[FindIt.AddTag]", "Add tag")}
          className={styles.addButton}
          disabled={query === ""}
          onClick={() => addTags(text)}
          mask="Media/Glyphs/Plus.svg"
        />
      </div>

      {(suggestions.length > 0 || canCreate) && (
        <>
          <div className={styles.subtitle}>
            {translate(
              "Tooltip.LABEL[FindIt.PreviousTags]",
              "Previously used tags",
            )}
          </div>
          <div className={styles.chipList}>
            {canCreate && (
              <Button
                variant="flat"
                className={classNames(styles.chip, styles.create)}
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                onSelect={() => addTags(text)}
              >
                {translate(
                  "Tooltip.LABEL[FindIt.CreateTag]",
                  'Create "{0}"',
                )?.replace("{0}", tagText)}
              </Button>
            )}
            {suggestions.map((tag) => (
              <Button
                key={tag}
                variant="flat"
                className={styles.chip}
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                onSelect={() => addTags(tag)}
              >
                {tag}
              </Button>
            ))}
          </div>
        </>
      )}
    </div>
  );
};
