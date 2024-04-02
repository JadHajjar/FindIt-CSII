import { bindValue, useValue } from "cs2/api";
import styles from "./prefabSelection.module.scss";
import { Scrollable } from "cs2/ui";
import mod from "../../../mod.json";
import { PrefabEntry } from "../../domain/prefabEntry";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
export const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");
export const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");

export const PrefabSelectionComponent = () => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const PrefabList = useValue(PrefabList$);
  const ActivePrefabId = useValue(ActivePrefabId$);
  const CurrentCategory = useValue(CurrentCategory$);

  // Do not put any Hooks (i.e. UseXXXX) after this point.
  if (!ShowFindItPanel) {
    return null;
  }

  return (
    <>
      <Scrollable
        vertical={true}
        trackVisibility="scrollable"
        className={styles.scrollableContainer}>
        <div className={styles.panelSection}>
          {PrefabList.map((prefab) => (
            <PrefabItemComponent
              prefab={prefab}
              selected={prefab.id == ActivePrefabId}
              showCategory={CurrentCategory === -1}></PrefabItemComponent>
          ))}
        </div>
      </Scrollable>
    </>
  );
};
