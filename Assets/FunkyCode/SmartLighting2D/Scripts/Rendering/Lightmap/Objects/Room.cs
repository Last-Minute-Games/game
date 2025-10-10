using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode.Rendering.Lightmap
{
    public static class Room
    {

        public static void Draw(LightRoom2D id, Camera camera)
        {

            switch(id.shape.type)
            {
                case LightRoom2D.RoomType.Collider:
                    DrawCollider(id, camera);
                break;

                case LightRoom2D.RoomType.Sprite:
                    DrawSprite(id, camera);
                break;
            }

            Material material = Lighting2D.materials.mask.GetMask();
            material.mainTexture = null;
        }

        public static bool drawColliderPass = false;

        static public void DrawColliderPass(LightRoom2D id, Camera camera)
        {
            List<MeshObject> meshObjects = id.shape.GetMeshes();

            if (meshObjects == null)
            {
                return;
            }
            
            if (!drawColliderPass)
            {
                drawColliderPass = true;

                Material material = Lighting2D.materials.room.GetRoomMask();
                material.mainTexture = null;
                material.SetPass(0);

                GLExtended.color = id.color;
                
                GL.Begin(GL.TRIANGLES);
            }

            Vector2 position = id.transform.position - camera.transform.position;

            GLExtended.DrawMeshPass(meshObjects, position, id.transform.lossyScale, id.transform.rotation.eulerAngles.z);
        }

        

        static public void DrawCollider(LightRoom2D id, Camera camera)
        {
            List<MeshObject> meshObjects = id.shape.GetMeshes();

            if (meshObjects == null)
            {
                return;
            }

            Material material = Lighting2D.materials.room.GetRoomMask();
            material.mainTexture = null;
            material.SetPass(0);

            GLExtended.color = id.color;

            Vector2 position = id.transform.position - camera.transform.position;

            GLExtended.DrawMesh(meshObjects, position, id.transform.lossyScale, id.transform.rotation.eulerAngles.z);
        }

        static public void DrawSprite(LightRoom2D id, Camera camera)
        {
            UnityEngine.SpriteRenderer spriteRenderer = id.shape.spriteShape.GetSpriteRenderer();
                    
            if (spriteRenderer == null)
            {
                return;
            }

            Sprite sprite = spriteRenderer.sprite;

            if (sprite == null)
            {
                return;
            }

            Material material = Lighting2D.materials.room.GetRoomMask();
            material.mainTexture = sprite.texture;
            material.SetPass(0);

            GLExtended.color = id.color;

            Vector2 position = id.transform.position - camera.transform.position;

            Rendering.Universal.Sprite.FullRect.Draw(id.spriteMeshObject, spriteRenderer, position, id.transform.lossyScale, id.transform.eulerAngles.z);	
        }
    }
}