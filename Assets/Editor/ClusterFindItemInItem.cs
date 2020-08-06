//  Copyright (c) 2020 R01hee
//
// Released under the MIT license
// https://github.com/r01hee/ClusterFindItemInItem/blob/master/LICENSE
//
// For more information: https://github.com/r01hee/ClusterFindItemInItem

using UnityEditor;
using UnityEngine;
using System.Linq;
using ClusterVR.CreatorKit.Item.Implements;

public class ClusterFindItemInItem : EditorWindow
{
    private class ItemRelation
    {
        public Item Item { get; }

        public ItemRelation Parent { get; set; }

        public ItemRelation[] Children { get; set; }

        public ItemRelation(Item item)
        {
            Item = item;
        }
    }

    private static float ItemIndentX = 24f;

    private ItemRelation[] ItemRelations = null;

    private Vector2 ScrollPos;

    [MenuItem("Cluster/Find Item in Item", priority = 10101)]
    private static void MenuItemGenerator()
    {
        GetWindow<ClusterFindItemInItem>();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        if (GUILayout.Button("Find\n探す"))
        {
            ItemRelations = GetItemRelations();
        }

        if (ItemRelations == null)
        {
            return;
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        if (!ItemRelations.Any(x => x.Parent != null))
        {
            var s = GUI.skin.box;
            s.alignment = TextAnchor.UpperLeft;
            EditorGUILayout.LabelField("There is not any nested Item, No problem!!\n入れ子になっているItemはありません、問題なし!!", s);
            return;
        }

        EditorGUILayout.LabelField("When you click below listed item, Gameobject in Hierarchy is selected!\n下記リストの項目をクリックするとHierarchy上のGameobjectが選択されます!", GUI.skin.box);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

        foreach (var i in ItemRelations.Where(x => x.Parent == null))
        {
            ShowItem(i);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        }

        EditorGUILayout.EndScrollView();
    }

    private static void ShowItem(ItemRelation itemRelation, int indent = 0)
    {
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 1.5f);

        var lastRect = GUILayoutUtility.GetLastRect();
        
        var buttonRect = new Rect(lastRect.x + (ItemIndentX * indent), lastRect.y , 8 * (itemRelation.Item.name.Length + 2), EditorGUIUtility.singleLineHeight);

        if (GUI.Button(buttonRect, itemRelation.Item.name))
        {
            Selection.activeGameObject = itemRelation.Item.gameObject;
        }

        var id = itemRelation.Item.GetInstanceID();

        foreach (var c in itemRelation.Children.Where(x => x.Parent.Item.GetInstanceID() == id))
        {
            ShowItem(c, indent + 1);
        }
    }

    private static ItemRelation[] GetItemRelations()
    {
        var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        var ItemRelations = Resources.FindObjectsOfTypeAll<Item>()
                       .Where(x => x.gameObject.scene.path == scenePath)
                       .Select(x => new ItemRelation(x))
                       .ToArray();

        foreach (var i in ItemRelations)
        {
            var id = i.Item.GetInstanceID();
            var children = i.Item.GetComponentsInChildren<Item>().Where(x => x.GetInstanceID() != id).ToArray();
            i.Children = children
                .Select(x =>
                {
                    var id2 = x.GetInstanceID();
                    return ItemRelations.FirstOrDefault(x2 => x2.Item.GetInstanceID() == id2);
                })
                .Where(x => x != null)
                .ToArray();
        }

        foreach (var i in ItemRelations)
        {
            foreach (var c in i.Children)
            {
                if (c.Parent == null)
                {
                    c.Parent = i;
                }
                else
                {
                    var id = i.Item.GetInstanceID();
                    if (c.Parent.Children.Any(x => x.Item.GetInstanceID() == id))
                    {
                        c.Parent = i;
                    }
                }
            }
        }

        return ItemRelations;
    }
}
