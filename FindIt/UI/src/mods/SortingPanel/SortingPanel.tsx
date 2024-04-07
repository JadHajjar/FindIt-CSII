import { bindValue, trigger, useValue } from "cs2/api";
import styles from "./SortingPanel.module.scss";
import mod from "../../../mod.json";
import { useRef, useState } from "react";

// These establishes the binding with C# side.
const ShowSortingPanel$ = bindValue<boolean>(mod.id, "ShowSortingPanel");

export const SortingPanel = () => {
  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const ShowSortingPanel = useValue(ShowSortingPanel$);

  if (!ShowSortingPanel) return null;

  return (
    <>
      <div className={styles.sortPanel}>
        <div></div>
      </div>
    </>
  );
};
