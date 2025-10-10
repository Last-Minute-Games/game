using UnityEngine;

namespace FunkyCode.Rendering.Day
{
    public class Sorted
    {
        static public void Draw(Pass pass)
        {
            for(int id = 0; id < pass.sortList.count; id++)
            {
                Sorting.SortObject sortObject = pass.sortList.list[id];

                switch(sortObject.type)
                {
                    case Sorting.SortObject.Type.Collider:

                        DayLightCollider2D collider = (DayLightCollider2D)sortObject.lightObject;

                        if (collider != null)
                        {
                            if (!collider.InAnyCamera()) 
                            {
                                continue;
                            }

                            if (pass.drawShadows)
                            {
                                if (collider.mainShape.shadowType == DayLightCollider2D.ShadowType.Collider2D || collider.mainShape.shadowType == DayLightCollider2D.ShadowType.SpritePhysicsShape)
                                {
                                    Lighting2D.materials.shadow.GetDayCPUShadow().SetPass (0);

                                    GL.Begin(GL.TRIANGLES);

                                    Shadow.Draw(collider, pass.offset);  

                                    GL.End(); 
                                }

                                SpriteRendererShadow.Begin(pass.offset);

                                SpriteRendererShadow.DrawOffset(collider);

                                SpriteRendererShadow.End();
                            }

                            if (pass.drawMask)
                            {
                                SpriteRenderer2D.Draw(collider, pass.offset);
                            }
                        }

                    break;

                     case Sorting.SortObject.Type.TilemapCollider:

                        DayLightTilemapCollider2D tilemap = (DayLightTilemapCollider2D)sortObject.lightObject;

                        if (tilemap != null)
                        {
                            if (pass.drawShadows)
                            {
                                if (!tilemap.ShadowsDisabled())
                                {
                                    Lighting2D.materials.shadow.GetDayCPUShadow().SetPass(0);

                                    GL.Begin(GL.TRIANGLES);
                                        Shadow.DrawTilemap(tilemap, pass.offset, pass.camera);            
                                    GL.End(); 
                                }
                            }
                           
                            if (pass.drawMask)
                            {
                                if (!tilemap.MasksDisabled())
                                {
                                    SpriteRenderer2D.DrawTilemap(tilemap, pass.offset);
                                }
                            }
                        }

                    break;
                }
            }
        }
    }
}
