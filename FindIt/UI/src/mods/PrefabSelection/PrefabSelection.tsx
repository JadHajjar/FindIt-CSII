import { bindValue, trigger, useValue } from "cs2/api";
import styles from "./prefabSelection.module.scss";
import mod from "../../../mod.json";
import { useRef, useState } from "react";
import { PrefabEntry } from "../../domain/prefabEntry";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
export const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");
export const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");
export const ScrollIndex$ = bindValue<number>(mod.id, "ScrollIndex");
export const MaxScrollIndex$ = bindValue<number>(mod.id, "MaxScrollIndex");

export const PrefabSelectionComponent = () => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowFindItPanel = useValue(ShowFindItPanel$);
  const PrefabList = useValue(PrefabList$);
  const ActivePrefabId = useValue(ActivePrefabId$);
  const CurrentCategory = useValue(CurrentCategory$);
  const ScrollIndex = useValue(ScrollIndex$);
  const MaxScrollIndex = useValue(MaxScrollIndex$);
  const scrollBarRef = useRef(null);
  const thumbRef = useRef(null);

  const [isDragging, setIsDragging] = useState(false);
  const [initialDivPos, setInitialDivPos] = useState(0);

  if (!ShowFindItPanel) {
    return null;
  }

  function OnWheel(obj: any) {
    trigger(mod.id, "OnScroll", obj.deltaY);
  }

  function handleScrollContainerClick(event: any) {
    const thumbRect = (thumbRef.current as any).getBoundingClientRect();
    if (event.clientY > thumbRect.bottom) {
      trigger(mod.id, "SetScrollIndex", ScrollIndex + 2);
    } else if (event.clientY < thumbRect.top) {
      trigger(mod.id, "SetScrollIndex", ScrollIndex - 2);
    }
  }

  function handleMouseDown(event: any) {
    setIsDragging(true);
    console.log(event.clientY);
    setInitialDivPos(
      event.clientY - (thumbRef.current as any).getBoundingClientRect().y
    );
  }

  function handleMouseMove(event: any) {
    if (!isDragging) return;

    const thumbRect = (scrollBarRef.current as any).getBoundingClientRect();

    const diffY = event.clientY - thumbRect.y - initialDivPos;
    const scrollPerc = diffY / (thumbRect.height - 30);
    const index = scrollPerc * MaxScrollIndex;

    trigger(mod.id, "SetScrollIndex", index);
  }

  function handleMouseUp() {
    setIsDragging(false);
  }

  return (
    <>
      {isDragging && (
        <div
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          className={styles.scrollBlocker}
        ></div>
      )}
      <div onWheel={OnWheel} className={styles.scrollableContainer}>
        <div
          className={styles.panelSection}
          style={{ margin: `${(ScrollIndex % 1) * -120}rem 0 0 0` }}
        >
          {PrefabList.map((prefab) => (
            <PrefabItemComponent
              prefab={prefab}
              selected={prefab.id == ActivePrefabId}
              showCategory={CurrentCategory === -1}
            ></PrefabItemComponent>
          ))}
        </div>
        {MaxScrollIndex > 0 && (
          <div
            className={styles.scrollContainer}
            ref={scrollBarRef}
            onClick={handleScrollContainerClick}
          >
            <div
              onMouseDown={handleMouseDown}
              onMouseMove={handleMouseMove}
              onMouseUp={handleMouseUp}
              className={styles.scrollBar}
              ref={thumbRef}
              style={{
                top: (ScrollIndex / MaxScrollIndex) * (185 - 30) + "rem",
                height: "30rem",
              }}
            ></div>
          </div>
        )}
      </div>
    </>
  );
};
