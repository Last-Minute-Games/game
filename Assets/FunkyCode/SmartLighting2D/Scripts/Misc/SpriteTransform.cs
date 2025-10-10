using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
    public struct SpriteTransform {
        public Vector2 position;
        public Vector2 scale;
        public float rotation;

        public Rect uv;

        public SpriteTransform(VirtualSpriteRenderer spriteRenderer, Vector2 position, Vector2 scale, float rotation) {
            UnityEngine.Sprite sprite = spriteRenderer.sprite;

            /*
            if (spriteRenderer == null || sprite == null) {
                this.rotation = 0;
                this.scale = Vector2.zero;
                this.uv = new Rect();
                this.position = Vector2.zero;

                return;
            }*/

            if (sprite == null) {
                this.position = Vector2.zero;
                this.scale = Vector2.zero;
                this.rotation = 0;
                this.uv = new Rect(0, 0, 0, 0);
                return;
            }

            Texture2D spriteTexture = sprite.texture;

            float textureWidth = spriteTexture.width;
            float textureHeight = spriteTexture.height;

            Rect spriteRect = sprite.textureRect;

            float spriteWidth = spriteRect.width;
            float spriteHeight = spriteRect.height;

            // Scale
            Vector2 textureScale = new Vector2(
                textureWidth / spriteWidth, 
                textureHeight / spriteHeight
            );
    
            float pixelsPerUnit = sprite.pixelsPerUnit * 2;

            scale.x = (scale.x / textureScale.x) * (textureWidth / pixelsPerUnit);
            scale.y = (scale.y / textureScale.y) * (textureHeight / pixelsPerUnit);

            if (spriteRenderer.flipX) {
                scale.x = -scale.x;
            }

            if (spriteRenderer.flipY) {
                scale.y = -scale.y;
            }

            // Pivot
            Vector2 pivot = sprite.pivot;
            
            pivot.x = ((pivot.x / spriteWidth) - 0.5f) * (scale.x * 2);
            pivot.y = ((pivot.y / spriteHeight) - 0.5f) * (scale.y * 2);

            // Matrix Projection
            float angle = rotation * Mathf.Deg2Rad + Mathf.PI;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            this.position.x = position.x + pivot.x * cos - pivot.y * sin;
            this.position.y = position.y + pivot.x * sin + pivot.y * cos;

            // UV coordinates
            Rect uvRect = new Rect();
            uvRect.x = spriteRect.x / textureWidth;
            uvRect.y = spriteRect.y / textureHeight;
            uvRect.width = spriteWidth / textureWidth + uvRect.x;
            uvRect.height = spriteHeight / textureHeight + uvRect.y;

            this.uv = uvRect;

            this.scale = scale;

            this.rotation = rotation;
        }
    }
}