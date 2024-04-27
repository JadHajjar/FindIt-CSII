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
const ShowFindItPanel$ = bindValue<boolean>(mod.id, "ShowFindItPanel");
const PrefabList$ = bindValue<PrefabEntry[]>(mod.id, "PrefabList");
const ActivePrefabId$ = bindValue<number>(mod.id, "ActivePrefabId");
const CurrentSubCategory$ = bindValue<number>(mod.id, "CurrentSubCategory");
const ScrollIndex$ = bindValue<number>(mod.id, "ScrollIndex");
const MaxScrollIndex$ = bindValue<number>(mod.id, "MaxScrollIndex");
const PanelWidth$ = bindValue<number>(mod.id, "PanelWidth");
const PanelHeight$ = bindValue<number>(mod.id, "PanelHeight");
const RowCount$ = bindValue<number>(mod.id, "RowCount");
const ColumnCount$ = bindValue<number>(mod.id, "ColumnCount");
export const ViewStyle$ = bindValue<string>(
  mod.id,
  "ViewStyle",
  "GridWithText"
);

export const PrefabSelectionComponent = (props: PrefabSelectionProps) => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const PrefabList = useValue(PrefabList$);
  const ActivePrefabId = useValue(ActivePrefabId$);
  const CurrentSubCategory = useValue(CurrentSubCategory$);
  const ScrollIndex = useValue(ScrollIndex$);
  const MaxScrollIndex = useValue(MaxScrollIndex$);
  const ViewStyle = useValue(ViewStyle$);
  const RowCount = useValue(RowCount$);
  const ColumnCount = useValue(ColumnCount$);
  const PanelWidth = useValue(PanelWidth$) + 15;
  const PanelHeight = useValue(PanelHeight$) + 9;
  const scrollBarRef = useRef(null);
  const thumbRef = useRef(null);
  var itemMargin = 8;
  var PanelItemHeight = 0;

  switch (ViewStyle) {
    case "ListSimple":
      itemMargin = 2.5;
      break;
    case "GridSmall":
      itemMargin = 4;
      break;
  }

  const PanelItemWidth = (PanelWidth - 15) / ColumnCount - itemMargin + "rem";

  switch (ViewStyle) {
    case "ListSimple":
      PanelItemHeight = 22.5;
      break;
    case "GridWithText":
      PanelItemHeight = 98;
      break;
    default:
      PanelItemHeight = (PanelWidth - 15) / ColumnCount;
      break;
  }

  const [isDragging, setIsDragging] = useState(false);
  const [initialDivPos, setInitialDivPos] = useState(0);

  const scrollBarHeight = Math.max(
    30,
    (PanelHeight * RowCount) / (MaxScrollIndex + RowCount)
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
        style={{ height: PanelHeight + "rem" }}
      >
        <div
          className={styles.panelSection + " " + PrefabItemStyles[ViewStyle]}
          style={{
            margin: `${(ScrollIndex % 1) * -PanelItemHeight}rem 0 0 0`,
            width: PanelWidth + "rem",
          }}
        >
          {PrefabList.map((prefab) => (
            <PrefabItemComponent
              prefab={prefab}
              selected={prefab.id == ActivePrefabId}
              showCategory={CurrentSubCategory === -1}
              width={PanelItemWidth}
            ></PrefabItemComponent>
          ))}
        </div>
        {MaxScrollIndex > 0 && (
          <div
            className={styles.scrollContainer}
            style={{ height: PanelHeight - 20 + "rem" }}
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
                    (PanelHeight - 20 - scrollBarHeight) +
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
