﻿/* 
            //if (GL.Button("Add Feature", GL.Width(300f))) {
//    BlueprintActions.addFact(selectedBlueprint);
//}
//if (GL.Button("Remove Feature", GL.Width(300f)))
//{
//    BlueprintActions.removeFact(selectedBlueprint);
//}
//            if (GL.Button("Give Item", GL.Width(300f))) {
//                BlueprintActions.addItem(selectedBlueprint);
////                CheatsUnlock.CreateItem("- " + parameter);
//            }

    //selectedBlueprintIndex = GL.SelectionGrid(selectedBlueprintIndex, filteredBPNames, 4);

    if (selectedBlueprintIndex  >= 0)
    {
        parameter = filteredBPNames[selectedBlueprintIndex];
        selectedBlueprint = filteredBPs[selectedBlueprintIndex];
    }                     blueprints

.Where(bp => bp.name.ToLower().Contains(searchText.ToLower()))
            .OrderBy(bp => bp.name)
            .Take(Settings.searchLimit).ToArray();


GL.Space(10);
            GL.Label("MyFloatOption", GL.ExpandWidth(false));
            GL.Space(10);
            Settings.MyFloatOption = GL.HorizontalSlider(Settings.MyFloatOption, 1f, 10f, GL.Width(300f));
            GL.Label($" {Settings.MyFloatOption:p0}", GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            GL.Label("MyBoolOption", GL.ExpandWidth(false));
            GL.Space(10);
            Settings.MyBoolOption = GL.Toggle(Settings.MyBoolOption, $" {Settings.MyBoolOption}", GL.ExpandWidth(false));
            GL.EndHorizontal();

            GL.BeginHorizontal();
            GL.Label("MyTextOption", GL.ExpandWidth(false));
            GL.Space(10);
            Settings.MyTextOption = GL.TextField(Settings.MyTextOption, GL.Width(300f));
            GL.EndHorizontal();
            */
