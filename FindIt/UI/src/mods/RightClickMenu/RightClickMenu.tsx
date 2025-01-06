import { useValue, bindValue, trigger } from "cs2/api";
import { game, Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import mod from "../../../mod.json";
import { TopBarComponent } from "mods/TopBar/TopBar";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { useState, useRef, useEffect } from "react";
import styles from "./mainContainer.module.scss";
import { OptionsPanelComponent } from "mods/OptionsPanel/OptionsPanel";

import React, { createContext } from "react";

// Create a context with a default value
const RightClickContext = createContext(false);

const PanelWidth$ = bindValue<number>(mod.id, "PanelWidth");
const IsExpanded$ = bindValue<boolean>(mod.id, "IsExpanded");

export default RightClickContext;

export const RightClickMenuComponent = () => {
  const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;

  const containerRef = useRef(null);

  const [optionsOpen, setOptionsOpen] = useState(false);
  const [containerLeft, setContainerLeft] = useState(0);

  // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
  const IsExpanded = useValue(IsExpanded$);
  const PanelWidth = useValue(PanelWidth$) + 15 + 20;

  const optionsOverflow = () => window.innerWidth < containerLeft + ((PanelWidth + 300) * window.innerHeight) / 1080;

  useEffect(() => {
    var newLeft = (containerRef.current as any)?.getBoundingClientRect().left ?? 0;

    if (newLeft !== 0) setContainerLeft(newLeft);
  });

  return (
    <>
      <div
        style={{
          position: "fixed",
          left: 100 + "px",
          top: 100 + "px",
          width: "100px",
          height: "100px",
          background: "red",
          zIndex: 9999,
        }}
      ></div>
    </>
  );
};
