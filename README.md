![Find It](https://imgur.com/V2ktQrz.png)
# Find It for Cities: Skylines II
Quickly browse and search through all of the assets inside of the game.
Use **Ctrl+F** to quickly open the Find It panel, or click on the magnifier icon in your toolbar.
Use **Ctrl+P** to enable the Picker tool, or click on the picker icon in your toolbar.

Huge thanks to **YenYang** for their help on the UI.

Special thanks to **Algernon** for their contribution and allowing me to take on this project

Thank you to **Chameleon** for providing the Icons.

And thank you to **Baka-gourd** (NullPinter) for their help with focus.

## Features
* Quick and intelligent search methods.
* Extensive and comprehensive asset categories.
* Panel stays open while placing assets, no jumping between different tabs.
* Tool options like Tree Controller and Line Tool are fully supported and can be used while the panel is open.
* Favorite your most used assets.
* Integrated Picker
* Extensive sorting and filtering options.

## Notes
Find It should automatically support any mods that add or alter the vanilla assets.

If you're having any issues, please report them on my discord as it's easier to help you that way.

---

## For Mod Developers

Find It exposes two APIs that other mods can integrate with. No hard dependency on Find It is required — integration is done through reflection-based hooks that Find It calls automatically on startup.

### Prefab Picker API

Lets your mod open Find It's picker window and get back the prefab the user selected.

**How to opt in:** Expose the following public static method on your `IMod` class:

```csharp
public static void SetFindItPrefabSelector(Func<Type, string, Task<PrefabBase>> selector)
{
    // Store the delegate — Find It calls this once on load to hand you the function.
    _findItSelector = selector;
}
```

Find It will discover this method automatically and call it with the picker delegate. Once you have it, call it whenever you need the user to pick a prefab:

```csharp
// Show a picker filtered to TreePrefab (title auto-generates as "Tree Prefab")
PrefabBase picked = await _findItSelector(typeof(TreePrefab), null);

// With a custom title
PrefabBase picked = await _findItSelector(typeof(StaticObjectPrefab), "Select a Prop");

// Returns null if the user closes the picker without selecting
if (picked != null) { /* use the prefab */ }
```

**Parameters:**
- `Type prefabType` — Any type that derives from `PrefabBase`. Find It will show only prefabs of that type (including subtypes).
- `string title` — Window title shown in the picker header. Pass `null` to use the auto-generated name (e.g. `"TreePrefab"` → `"Tree Prefab"`).

**Return value:** `Task<PrefabBase>` that completes with the selected prefab, or `null` if the user dismissed the picker.

---

### Custom Search API

Lets your mod override Find It's search logic with your own matching function.

**How to opt in:** Expose the following public static method on your `IMod` class:

```csharp
public static Func<string, bool> GetFindItSearchMethod(string searchText)
{
    // Return a predicate that returns true for prefab names that should match.
    return name => name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
}
```

Find It calls this method on every search and uses your predicate to filter results by both display name and internal prefab name.

---

### Prefab Multi-Selection API

Lets the active tool receive several prefabs at once. When a tool implements this contract, **Ctrl+click** toggles a prefab in/out of the selection and **Shift+click** selects a range; Find It highlights the whole set and keeps it in sync with the tool.

**How to opt in:** Expose all three public methods on your tool (a `ToolBaseSystem`). Find It discovers them by reflection — no dependency on Find It and no shared assembly:

```csharp
public void SelectPrefab(PrefabBase prefab);      // add one to the selection
public void DeselectPrefab(PrefabBase prefab);    // remove one from the selection
public List<PrefabBase> GetSelectedPrefabs();     // current selection
```

All three are required — a lone select method is ignored. After every change Find It re-reads `GetSelectedPrefabs()`, so your tool stays the single source of truth and may silently reject prefabs it can't handle. No separate "clear" method is needed: Find It clears by removing each selected prefab.

A tool that already has `SelectTreePrefab` / `UnselectTreePrefab` / `GetSelectedPrefabs` just adds two aliases:

```csharp
public void SelectPrefab(PrefabBase prefab) => SelectTreePrefab(prefab);
public void DeselectPrefab(PrefabBase prefab) => UnselectTreePrefab(prefab);
```

---

## For Asset Creators

### Find It Properties

Open your asset in the editor and click the **Find It Properties** button (the tag icon) in the editor's bottom bar, next to the advisor button. It edits the asset you have selected in the inspector, and you can choose where the data is saved:

- **Standard:** saved in the `FindItProperties` component. Every option is available, but if the asset is edited and saved again without Find It enabled, these properties will be lost.
- **More Compatibility:** saved as `#` entries in the `Editor Asset Category Override` exclude list. Only tags and the category are available, but they're kept even if the asset is edited without Find It. The category is saved as a `FindIt/<category>/<sub-category>` entry in the include list.

Switching between the two moves your tags and category over. Switching to More Compatibility removes the `FindItProperties` component, along with its hide setting.

You can also edit the `FindItProperties` component (under the `FindIt/` menu) or the exclude list directly in the inspector.

### Tags

Tags let players find your assets with a `#tag` search. Players can't remove them, and Find It doesn't save them, they always come from your asset. Spaces in tags are replaced with dashes, so `white label` becomes `white-label`.

### Category

Places the asset in a specific Find It sub-category instead of the one Find It picks automatically. In the component, `Sub Category` is the sub-category's name, for example `Props_Fences` or `Buildings_Specialized`. Leave it empty to keep the automatic category.

### Hide from Find It

Keeps the asset out of Find It completely.
