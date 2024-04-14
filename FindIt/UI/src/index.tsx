import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { TopBarComponent } from "mods/TopBar/TopBar";

import mod from "../mod.json";
import { ToolbarIconComponent } from "mods/ToolbarIcon/ToolbarIcon";
import { RemoveVanillaAssetMenuComponent } from "mods/RemoveVanillaAssetMenu/RemoveVanillaAssetMenu";
import { FindItMainContainerComponent } from "mods/MainContainer/MainContainer";

const register: ModRegistrar = (moduleRegistry) => {
  // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
  VanillaComponentResolver.setRegistry(moduleRegistry);

  // This repalaces the asset grid.
  moduleRegistry.extend(
    "game-ui/game/components/asset-menu/asset-menu.tsx",
    "AssetMenu",
    RemoveVanillaAssetMenuComponent
  );

  // This adds the fint it and picker icons to the toolbar
  moduleRegistry.extend(
    "game-ui/game/components/toolbar/top/toggles.tsx",
    "PhotoModeToggle",
    ToolbarIconComponent
  );

  // This wraps prefab selection and top bar components.
  moduleRegistry.append("Game", FindItMainContainerComponent);

  // This is just to verify using UI console that all the component registriations was completed.
  console.log(mod.id + " UI module registrations completed.");
};

export default register;
