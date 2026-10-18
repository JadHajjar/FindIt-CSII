import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import { Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { getModule, ModuleRegistryExtend } from "cs2/modding";
import { Children, cloneElement, isValidElement, ReactNode } from "react";
import classNames from "classnames";
import mod from "../../../mod.json";
import tag from "images/findit_tag.svg";
import find from "images/findit_find.svg";
import picker from "images/findit_pickerGlyph.svg";
import styles from "./creatorProperties.module.scss";

const IconButton = getModule(
  "game-ui/common/input/button/icon-button.tsx",
  "IconButton",
);

const BottomBarTheme: Theme | any = getModule(
  "game-ui/editor/components/bottom-bar/bottom-bar.module.scss",
  "classes",
);

const BottomBarButtonTheme: Theme | any = getModule(
  "game-ui/editor/components/bottom-bar/bottom-bar-floating-hint-button.module.scss",
  "classes",
);

const IsOpen$ = bindValue<boolean>(mod.id, "CreatorIsOpen");
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const IsWindowLocked$ = bindValue<boolean>(mod.id, "IsWindowLocked");
const PickerActive$ = bindValue<boolean>(mod.id, "PickerActive");

type _ButtonProps = {
  tooltip: string | null;
  src: string;
  highlighted?: boolean;
  onSelect: () => void;
};

const BottomBarButton = (props: _ButtonProps) => (
  <Tooltip tooltip={props.tooltip}>
    <div className={styles.bottomBarButton}>
      <IconButton
        tinted
        src={props.src}
        theme={BottomBarButtonTheme}
        className={classNames(
          BottomBarTheme.button,
          props.highlighted && BottomBarTheme.buttonHighlight,
        )}
        onSelect={props.onSelect}
      />
    </div>
  </Tooltip>
);

const FindItBottomBarButtons = () => {
  const { translate } = useLocalization();
  const IsOpen = useValue(IsOpen$);
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const IsWindowLocked = useValue(IsWindowLocked$);
  const PickerActive = useValue(PickerActive$);

  return (
    <>
      <BottomBarButton
        tooltip={translate("Tooltip.LABEL[FindIt.Picker]", "Picker")}
        src={picker}
        highlighted={PickerActive}
        onSelect={() => trigger(mod.id, "PickerIconToggled")}
      />
      <BottomBarButton
        tooltip={translate("Tooltip.LABEL[FindIt.FindIt]", "Find It")}
        src={find}
        highlighted={ShowFindItPanel || IsWindowLocked}
        onSelect={() => trigger(mod.id, "FindItIconToggled")}
      />
      <BottomBarButton
        tooltip={translate(
          "Tooltip.LABEL[FindIt.CreatorProperties]",
          "Find It Properties",
        )}
        src={tag}
        highlighted={IsOpen}
        onSelect={() => trigger(mod.id, "CreatorToggle")}
      />
    </>
  );
};

const insertButton = (node: ReactNode): ReactNode => {
  if (!isValidElement(node)) return node;

  const props = node.props as any;

  if (props.className === BottomBarTheme.inspectorModes) {
    return cloneElement(
      node,
      undefined,
      ...Children.toArray(props.children),
      <FindItBottomBarButtons key="findit-bottom-bar-buttons" />,
    );
  }

  if (props.children === undefined) return node;

  if (Array.isArray(props.children)) {
    const children = props.children.map(insertButton);

    return children.some((x: ReactNode, i: number) => x !== props.children[i])
      ? cloneElement(node, undefined, ...children)
      : node;
  }

  const child = insertButton(props.children);

  return child !== props.children ? cloneElement(node, undefined, child) : node;
};

export const BottomBarExtension: ModuleRegistryExtend = (Component: any) => {
  return (props: any) => insertButton(Component(props)) as JSX.Element;
};
