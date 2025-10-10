using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode.Rendering.Light
{
    public class Shape
    {
        public static void Mask(Light2D light, LightCollider2D id, LayerSetting layerSetting)
        {
            if (!id.InLight(light))
            {
                return;
            }

            LightColliderShape shape = id.mainShape;

            List<MeshObject> meshObjects = shape.GetMeshes();

            if (meshObjects == null)
            {
                return;
            }
                        
            Vector2 position = shape.transform2D.position - light.transform2D.position;

            Vector2 pivotPosition = shape.GetPivotPoint() - light.transform2D.position;
            GLExtended.color = LayerSettingColor.Get(pivotPosition, layerSetting, id.maskLit, 1, id.maskLitCustom);

            GLExtended.DrawMeshPass(meshObjects, position, shape.transform.lossyScale, shape.transform2D.rotation);
        }
    }
}