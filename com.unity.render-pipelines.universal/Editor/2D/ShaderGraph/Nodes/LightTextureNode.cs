using System;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace _2D.ShaderGraph
{
    enum BlendStyle
    {
        LightTex0,
        LightTex1,
        LightTex2,
        LightTex3,
    }

    [Title("Input", "2D", "Light Texture")]
    class LightTextureNode :  AbstractMaterialNode
    {
        private const int OutputSlotId = 0;
        private const string kOutputSlotName = "Out";

        [SerializeField] private BlendStyle m_BlendStyle = BlendStyle.LightTex0;

        [EnumControl("")]
        public BlendStyle blendStyle
        {
            get { return m_BlendStyle; }
            set
            {
                if (m_BlendStyle == value)
                    return;

                m_BlendStyle = value;
                Dirty(ModificationScope.Graph);
            }
        }

        public LightTextureNode()
        {
            name = "2D Light Texture";
            UpdateNodeAfterDeserialization();
        }
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId });
        }

        string GetVariableName()
        {
            return $"_ShapeLightTexture{(int)m_BlendStyle}";
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            return $"UnityBuildTexture2DStructNoScale({GetVariableName()})";
        }

        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            properties.AddShaderProperty(new Texture2DShaderProperty()
            {
                overrideReferenceName = GetVariableName(),
                generatePropertyBlock = false,
                // value = m_Texture,
                modifiable = false
            });
        }
    }
}
