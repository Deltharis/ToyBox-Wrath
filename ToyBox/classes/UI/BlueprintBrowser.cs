﻿using UnityEngine;
using UnityModManagerNet;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace ToyBox {
    public class BlueprintBrowser {
        public static BlueprintScriptableObject[] blueprints = null;
        public static BlueprintScriptableObject[] filteredBPs = null;
        public static String[] filteredBPNames = null;
        public static int matchCount = 0;
        public static String parameter = "";
        static int selectedBlueprintIndex = -1;
        static BlueprintScriptableObject selectedBlueprint = null;
        static BackgroundWorker searchWorker = new BackgroundWorker();

        static readonly NamedTypeFilter[] blueprintTypeFilters = new NamedTypeFilter[] {
            new NamedTypeFilter("All", typeof(BlueprintScriptableObject)),
            new NamedTypeFilter("Facts",typeof(BlueprintFact)),
            new NamedTypeFilter("Features", typeof(BlueprintFeature)),
            new NamedTypeFilter("Races", typeof(BlueprintRace)),
            new NamedTypeFilter("Buffs", typeof(BlueprintBuff)),
            new NamedTypeFilter("Weapons", typeof(BlueprintItemWeapon)),
            new NamedTypeFilter("Armor", typeof(BlueprintItemArmor)),
            new NamedTypeFilter("Shields", typeof(BlueprintItemShield)),
            new NamedTypeFilter("Equipment", typeof(BlueprintItemEquipment)),
            new NamedTypeFilter("Usable", typeof(BlueprintItemEquipmentUsable)),
            new NamedTypeFilter("Units", typeof(BlueprintUnit)),
            new NamedTypeFilter("Quests", typeof(BlueprintQuest)),
        };
        public static BlueprintScriptableObject[] GetBlueprints() {
            var bundle = (AssetBundle)AccessTools.Field(typeof(ResourcesLibrary), "s_BlueprintsBundle")
                .GetValue(null);
            return bundle.LoadAllAssets<BlueprintScriptableObject>();
        }

        public static void ResetSearch() {
            filteredBPs = null;
            filteredBPNames = null;
        }
        public static async void UpdateSearchResults() {
            if (blueprints == null) {
                blueprints = GetBlueprints(); //.Where(bp => !BlueprintAction.ignoredBluePrintTypes.Contains(bp.GetType())).ToArray();
            }
            selectedBlueprint = null;
            selectedBlueprintIndex = -1;
            if (Main.settings.searchText.Trim().Length == 0) {
                ResetSearch();
            }
            var terms = Main.settings.searchText.Split(' ').Select(s => s.ToLower()).ToArray();
            var filtered = new List<BlueprintScriptableObject>();
            var selectedType = blueprintTypeFilters[Main.settings.selectedBPTypeFilter].type;
            foreach (BlueprintScriptableObject blueprint in blueprints) {
                var name = blueprint.name.ToLower();
                var type = blueprint.GetType();
                if (terms.All(term => name.Contains(term)) && type.IsKindOf(selectedType)) {
                    filtered.Add(blueprint);
                }
            }
            matchCount = filtered.Count();
            filteredBPs = filtered
                    .OrderBy(bp => bp.name)
                    .Take(Main.settings.searchLimit).OrderBy(bp => bp.name).ToArray();
            filteredBPNames = filteredBPs.Select(b => b.name).ToArray();
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry) {
            UI.Section("Search 'n Pick", () => {
                UI.Label("(please note the first search may take a few seconds)".green(), UI.AutoWidth());
                UI.Space(25);
                UI.ActionSelectionGrid(ref Main.settings.selectedBPTypeFilter,
                    blueprintTypeFilters.Select(tf => tf.name).ToArray(),
                    5,
                    (selected) => { UpdateSearchResults(); },
                    UI.MinWidth(200));
                UI.Space(10);

                UI.BeginHorizontal();
                UI.ActionTextField(
                    ref Main.settings.searchText, (text) => { },
                    "searhText", () => { UpdateSearchResults(); },
                    UI.Width(400));
                UI.Space(50);
                UI.Label("Limit", UI.ExpandWidth(false));
                UI.ActionIntTextField(
                    ref Main.settings.searchLimit, (limit) => { },
                    "searchLimit", () => { UpdateSearchResults(); },
                    UI.Width(200));
                if (Main.settings.searchLimit > 1000) { Main.settings.searchLimit = 1000; }
                UI.Space(50);
                UI.Label((matchCount > 0
                            ? "Matches: ".green().bold() + $"{matchCount}".orange().bold()
                                + (matchCount > Main.settings.searchLimit
                                    ? " => ".cyan() + $"{Main.settings.searchLimit}".cyan().bold()
                                    : "")
                            : ""), UI.ExpandWidth(false));
                UI.EndHorizontal();

                UI.ActionButton("Search", () => {
                    UpdateSearchResults();
                }, UI.AutoWidth());
                UI.Space(10);

                if (filteredBPs != null) {
                    UnitReference selected = CharacterPicker.selectedCharacter;
                    int index = 0;
                    int maxActions = 0;
                    foreach (BlueprintScriptableObject blueprint in filteredBPs) {
                        var actions = blueprint.ActionsForUnit(selected);
                        maxActions = Math.Max(actions.Count, maxActions);
                    }

                    foreach (BlueprintScriptableObject blueprint in filteredBPs) {
                        UI.BeginHorizontal();
                        UI.Label(blueprint.name.orange().bold(), UI.Width(650));
                        var actions = blueprint.ActionsForUnit(selected);
                        int actionCount = actions != null ? actions.Count() : 0;
                        for (int ii = 0; ii < maxActions; ii++) {
                            if (ii < actionCount) {
                                BlueprintAction action = actions[ii];
                                UI.ActionButton(action.name, () => { action.action(selected, blueprint); }, UI.Width(140));
                                UI.Space(10);
                            }
                            else {
                                UI.Space(154);
                            }
                        }
                        UI.Space(30);
                        UI.Label($"{blueprint.GetType().Name.cyan()}", UI.Width(400));
                        UI.EndHorizontal();
                        String description = blueprint.GetDescription();
                        if (description.Length > 0) {
                            UI.BeginHorizontal();
                            UI.Space(684 + maxActions * 154);
                            UI.Label($"{description.green()}");
                            UI.EndHorizontal();
                        }
                        index++;
                    }
                }

            });
        }

    }
}