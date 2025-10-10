using UnityEngine;

namespace FunkyCode.Rendering.Lightmap
{
	public static class Particle
    {
        static public void DrawPass(Vector2 pos, Vector2 size, float angle) {
            angle = angle * Mathf.Deg2Rad + Mathf.PI;

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            float cosx = size.x * cos;
            float sinx = size.x * sin;

            float cosy = size.y * cos;
            float siny = size.y * sin;

            GL.TexCoord3 (1, 1, 0);
            GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 1, 0);
            GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 0, 0);
            GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

            GL.TexCoord3 (1, 0, 0);
            GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
		}
	}
}