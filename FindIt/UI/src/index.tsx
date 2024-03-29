import { ModRegistrar } from "cs2/modding";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { PrefabSelectionComponent } from "mods/PrefabSelection/PrefabSelection";
import { TopBarComponent } from "mods/TopBar/TopBar";

import mod from "../mod.json";

const register: ModRegistrar = (moduleRegistry) => {
  // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
  VanillaComponentResolver.setRegistry(moduleRegistry);

  // This repalaces the asset grid.
  moduleRegistry.override(
    "game-ui/game/components/asset-menu/asset-grid/asset-grid.tsx",
    "AssetGrid",
    PrefabSelectionComponent
  );

  // This repalaces the asset category top bar
  moduleRegistry.override(
    "game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.tsx",
    "AssetCategoryTabBar",
    TopBarComponent
  );

  // This is just to verify using UI console that all the component registriations was completed.
  console.log(mod.id + " UI module registrations completed.");
};

export default register;
