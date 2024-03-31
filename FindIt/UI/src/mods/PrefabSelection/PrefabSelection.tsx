import { bindValue, useValue, trigger } from "cs2/api";
import { Entity } from "cs2/bindings";
import styles from "./prefabSelection.module.scss";
import { Scrollable } from "cs2/ui";
import mod from "../../../mod.json";
import { PrefabEntry } from "../../domain/prefabEntry";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
export const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");

export const PrefabSelectionComponent = () => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const PrefabList = useValue(PrefabList$);
  const ActivePrefabId = useValue(ActivePrefabId$);

  // Do not put any Hooks (i.e. UseXXXX) after this point.
  if (!ShowFindItPanel) {
    return null;
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
              prefab={prefab}
              selected={prefab.id == ActivePrefabId}
            ></PrefabItemComponent>
          ))}
        </div>
      </Scrollable>
    </>
  );
};
