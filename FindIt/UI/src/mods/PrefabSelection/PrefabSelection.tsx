import { bindValue, useValue } from "cs2/api";
import { Entity  } from "cs2/bindings";
import styles from "./prefabSelection.module.scss";
import { Scrollable } from "cs2/ui";
import mod from "../../../mod.json";
import { PrefabItemComponent } from "../PrefabItem/PrefabItem";
import { ModuleRegistryExtend } from "cs2/modding";

// These establishes the binding with C# side.
export const ActivePrefabEntity$ =        bindValue<Entity> (mod.id, 'ActivePrefabEntity');
export const ShowFindItPanel$ =        bindValue<boolean> (mod.id, 'ShowFindItPanel');

export const PrefabSelectionComponent : ModuleRegistryExtend = (Component) => 
{
  // I believe you should not put anything here.
  return (props) => 
  {
    const {children, ...otherProps} = props || {};
    
    // These get the value of the bindings. Without C# side game ui will crash. Or they will when we have bindings.
    const ShowFindItPanel = useValue(ShowFindItPanel$); 
    const ActivePrefabEntity = useValue(ActivePrefabEntity$); 

    var buttons = [];

    for (var i = 0; i < 25; i++) 
    {
      buttons.push(PrefabItemComponent({ index: 12561, version: 1}));
      buttons.push(PrefabItemComponent({index: 15985, version: 1}));
      buttons.push(PrefabItemComponent({index: 12472, version: 1}));
      buttons.push(PrefabItemComponent({index: 18210, version: 1}));
      buttons.push(PrefabItemComponent({index: 12623, version: 1}));
      buttons.push(PrefabItemComponent({index: 12624, version: 1}));
    }

    // Do not put any Hooks (i.e. UseXXXX) after this point.
    if (!ShowFindItPanel) 
    {
      return (
        <Component {...otherProps}>
                {children}
        </Component>
      );
    }

    return (
      <>
          <Scrollable
            vertical={true}
            trackVisibility="always"
            className={styles.scrollableContainer}
          >
            <div className={styles.panelSection}>{buttons}</div>
          </Scrollable>
      </>
    );
  }
};
