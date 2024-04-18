import { bindValue, trigger, useValue } from "cs2/api";
import styles from "./prefabSelection.module.scss";
import mod from "../../../mod.json";
import PrefabItemStyles from "../PrefabItem/prefabItem.module.scss";
import { useRef, useState } from "react";
import { PrefabEntry } from "../../domain/prefabEntry";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";

export interface PrefabSelectionProps {
  expanded: boolean;
}

// These establishes the binding with C# side.
export const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
export const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
export const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");
export const CurrentCategory$ = bindValue<number>(mod.id, "CurrentCategory");
export const ScrollIndex$ = bindValue<number>(mod.id, "ScrollIndex");
export const MaxScrollIndex$ = bindValue<number>(mod.id, "MaxScrollIndex");
export const RowCount$ = bindValue<number>(mod.id, "RowCount");
export const ColumnCount$ = bindValue<number>(mod.id, "ColumnCount");
export const ExpandedRowCount$ = bindValue<number>(mod.id, "ExpandedRowCount");
export const ExpandedColumnCount$ = bindValue<number>(
  mod.id,
  "ExpandedColumnCount"
);
export const ViewStyle$ = bindValue<string>(
  mod.id,
  "ViewStyle",
  "GridWithText"
);

export const PrefabSelectionComponent = (props: PrefabSelectionProps) => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const PrefabList = useValue(PrefabList$);
  const ActivePrefabId = useValue(ActivePrefabId$);
  const CurrentCategory = useValue(CurrentCategory$);
  const ScrollIndex = useValue(ScrollIndex$);
  const MaxScrollIndex = useValue(MaxScrollIndex$);
  const RowCount = props.expanded
    ? useValue(ExpandedRowCount$)
    : useValue(RowCount$);
  const ColumnCount = props.expanded
    ? useValue(ExpandedColumnCount$)
    : useValue(ColumnCount$);
  const ViewStyle = useValue(ViewStyle$);
  const scrollBarRef = useRef(null);
  const thumbRef = useRef(null);

  const [isDragging, setIsDragging] = useState(false);
  const [initialDivPos, setInitialDivPos] = useState(0);

  const panelHeight = RowCount * 98 + 9;
  const panelWidth = ColumnCount * 113 + 15;
  const scrollBarHeight = Math.max(
    30,
    (panelHeight * RowCount) / (MaxScrollIndex + RowCount)
  );

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
    setInitialDivPos(
      event.clientY - (thumbRef.current as any).getBoundingClientRect().y
    );
  }

  function handleMouseMove(event: any) {
    if (!isDragging) return;

    const thumbRect = (scrollBarRef.current as any).getBoundingClientRect();

    const diffY = event.clientY - thumbRect.y - initialDivPos;
    const scrollPerc =
      diffY /
      Number(thumbRect.height - (scrollBarHeight * window.innerHeight) / 1080);
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
      <div
        onWheel={OnWheel}
        className={styles.scrollableContainer}
        style={{ height: panelHeight + "rem" }}
      >
        <div
          className={styles.panelSection + " " + PrefabItemStyles[ViewStyle]}
          style={{
            margin: `${(ScrollIndex % 1) * -98}rem 0 0 0`,
            width: panelWidth + "rem",
          }}
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
            style={{ height: panelHeight - 20 + "rem" }}
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
                top:
                  (ScrollIndex / MaxScrollIndex) *
                    (panelHeight - 20 - scrollBarHeight) +
                  "rem",
                height: scrollBarHeight + "rem",
              }}
            ></div>
          </div>
        )}
      </div>
    </>
  );
};
