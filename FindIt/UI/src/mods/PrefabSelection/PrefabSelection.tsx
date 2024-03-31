import { bindValue, useValue, trigger } from "cs2/api";
import { Entity } from "cs2/bindings";
import styles from "./prefabSelection.module.scss";
import { Scrollable } from "cs2/ui";
import mod from "../../../mod.json";
import { PrefabEntry } from "../../domain/prefabEntry";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";
import { ModuleRegistryExtend } from "cs2/modding";

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
export const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");

export const PrefabSelectionComponent: ModuleRegistryExtend = (Component) => {
  // I believe you should not put anything here.
  return (props) => {
    const { children, ...otherProps } = props || {};

    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$);
    const PrefabList = useValue(PrefabList$);
    const ActivePrefabId = useValue(ActivePrefabId$);

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!ShowFindItPanel) {
      return <Component {...otherProps}>{children}</Component>;
    }

    return (
      <>
        <Scrollable
          vertical={true}
          trackVisibility="always"
          className={styles.scrollableContainer}
        >
          <div className={styles.panelSection}>
            {PrefabList.map((prefab) => (
              <PrefabItemComponent
                id={prefab.id}
                src={prefab.thumbnail}
                text={prefab.name}
                favorited={prefab.favorited}
                selected={prefab.id == ActivePrefabId}
                onFavoriteClicked={() => (prefab.favorited = !prefab.favorited)}
              ></PrefabItemComponent>
            ))}
          </div>
        </Scrollable>
      </>
    );
  };
};
