using System;
using System.IO;
using UnityEngine;

using Object = UnityEngine.Object;

using MaterialUtility = UnityEditor.Experimental.MaterialUtility;

namespace UnityEditor.Rendering
{
    class MaterialVariantMenuItem
    {
        private const string MATERIAL_VARIANT_MENU_PATH = "Assets/Create/Material Variant";
        private const int MATERIAL_VARIANT_MENU_PRIORITY = 302; // right after material

        class DoCreateNewMaterialVariant : ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object parentAsset = AssetDatabase.LoadAssetAtPath<Object>(resourceFile);

                if (!(parentAsset is Material || parentAsset is Shader))
                    throw new ArgumentNullException("Invalid parent asset");

                Object o = MaterialUtility.CreateVariant(parentAsset, pathName);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

        [MenuItem(MATERIAL_VARIANT_MENU_PATH, true)]
        static bool ValidateMaterialVariantMenu()
        {
            Object root = Selection.activeObject;
            // We don't allow to create a MaterialVariant without parent
            return (EditorUtility.IsPersistent(root) && ((root is Material) || (root is Shader)));
        }

        [MenuItem(MATERIAL_VARIANT_MENU_PATH, false, MATERIAL_VARIANT_MENU_PRIORITY)]
        static void CreateMaterialVariantMenu()
        {
            var target = Selection.activeObject;

            string sourcePath = AssetDatabase.GetAssetPath(target);
            string variantPath = Path.Combine(Path.GetDirectoryName(sourcePath),
                Path.GetFileNameWithoutExtension(sourcePath) + " Variant.mat");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateNewMaterialVariant>(),
                variantPath,
                EditorGUIUtility.FindTexture("PrefabVariant Icon"),
                sourcePath);
        }
    }
}
