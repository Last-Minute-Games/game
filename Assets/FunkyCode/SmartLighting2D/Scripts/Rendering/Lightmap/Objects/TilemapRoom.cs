using UnityEngine;
using FunkyCode.LightTilemapCollider;
using FunkyCode.Utilities;

namespace FunkyCode.Rendering.Lightmap
{
    public class TilemapRoom
    {
        static public void Draw(LightTilemapRoom2D id, Camera camera)
        {
            Material materialColormask = Lighting2D.materials.room.GetRoomMask();
            Material materialMultiply = Lighting2D.materials.room.GetRoomMultiply();

            Material material = null;

            switch(id.shaderType)
            {
                case LightTilemapRoom2D.ShaderType.ColorMask:

                    material = materialColormask;

                    break;

                case LightTilemapRoom2D.ShaderType.MultiplyTexture:

                    material = materialMultiply;

                    break;
            }

            switch(id.maskType)
            {
                case LightTilemapRoom2D.MaskType.Sprite:
                    
                    switch(id.mapType)
                    {
                        case MapType.UnityRectangle:

                            GLExtended.color = id.color;
                            
                            Sprite.Draw(camera, id, material);

                        break;	

                        case MapType.SuperTilemapEditor:

                            SuperTilemapEditorSupport.Lightmap.Room.DrawTiles(camera, id, material);

                        break;
                    }
                    
                break;
            }
        }

        public class Sprite
        {
            public static VirtualSpriteRenderer spriteRenderer = new VirtualSpriteRenderer();

            public static void Draw(Camera camera, LightTilemapRoom2D id, Material material)
            {
                Vector2 cameraPosition = -camera.transform.position;

                float cameraRadius = CameraTransform.GetRadius(camera);

                LightTilemapCollider.Base tilemapCollider = id.GetCurrentTilemap();

                material.mainTexture = null; 

                Texture2D currentTexture = null;
        
                GL.Begin (GL.QUADS);

                int count = tilemapCollider.chunkManager.GetTiles(CameraTransform.GetWorldRect(camera));

                for(int i = 0; i < count; i++)
                {
                    LightTile tile = tilemapCollider.chunkManager.display[i];

                    if (tile.GetSprite() == null)
                    {
                       continue;
                    }

                    Vector2 tilePosition = tile.GetWorldPosition(tilemapCollider);

                    tilePosition += cameraPosition;

                    if (tile.NotInRange(tilePosition, cameraRadius))
                    {
                       continue;
                    }

                    spriteRenderer.sprite = tile.GetSprite();

                    if (spriteRenderer.sprite.texture == null)
                    {
                        continue;
                    }
                    
                    if (currentTexture != spriteRenderer.sprite.texture)
                    {
                        currentTexture = spriteRenderer.sprite.texture;
                        material.mainTexture = currentTexture;

                        material.SetPass(0);
                    }
        
                    Universal.Sprite.Pass.Draw(tile.spriteMeshObject, spriteRenderer, tilePosition, tile.worldScale, tile.worldRotation);
                }

                GL.End();

                material.mainTexture = null;
            }
        }
    }
}
