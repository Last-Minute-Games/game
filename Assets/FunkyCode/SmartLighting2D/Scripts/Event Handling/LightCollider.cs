using System.Collections.Generic;
using UnityEngine;
using FunkyCode.LightingSettings;
using FunkyCode.Utilities;

namespace FunkyCode.EventHandling
{
    public class LightCollider : Base
    {
        static public void GetCollisions(List<LightCollision2D> collisions, Light2D lightingSource)
        {
            List<LightCollider2D> colliderList = LightCollider2D.ListEventReceivers;

            // why all and not selected? + Specific layer

            for(int i = 0; i < colliderList.Count; i++)
            {
                LightCollider2D id = colliderList[i];

                if (id.mainShape.shadowType == LightCollider2D.ShadowType.None)
                {
                    continue;
                }

                if (!id.InLight(lightingSource))
                {
                    continue;
                }

                List<Polygon2> polygons = id.mainShape.GetPolygonsWorld();

                if (polygons.Count < 1)
                {
                    continue;
                }

                Polygon2 polygon = polygons[0];
       
                LightCollision2D collision = new LightCollision2D();
                collision.light = lightingSource;
                collision.collider = id;
                collision.points = new List<Vector2>();
      
                foreach(Vector2 point in polygon.points)
                {
                    Vector2 p = point;

                    p.x -= lightingSource.transform.position.x;
                    p.y -= lightingSource.transform.position.y;

                    if (p.magnitude < lightingSource.size)
                    {
                        float direction = p.Atan2(Vector2.zero) * Mathf.Rad2Deg;

                        if (lightingSource.applyRotation != Light2D.Rotation.Disabled)
                        {
                            direction -= lightingSource.transform2D.rotation;
                        }

                        direction = (direction + 1080 - 90) % 360;

                        // not finished

                        if (direction <= lightingSource.spotAngleInner / 2 || direction >= 360 - lightingSource.spotAngleInner / 2)
                        {
                            collision.points.Add(p);
                        }
                    }
                }

                if (collision.points.Count > 0)
                {
                    collisions.Add(collision);
                }
            }
        }

        public static Vector2[] removePointsColliding = new Vector2[100];
        public static int removePointsCollidingCount = 0;

        public static LightCollision2D[] removeCollisions = new LightCollision2D[100];
        public static int removeCollisionsCount = 0;
  
        public static List<LightCollision2D> RemoveHiddenPoints(List<LightCollision2D> collisions, Light2D light, EventPreset eventPreset)
        {
            float lightSize = Mathf.Sqrt(light.size * light.size + light.size * light.size);
            float rotLeft, rotRight;	
            
            Polygon2 testPolygon = GetPolygon();

            Vector2 lightPosition = - light.transform.position;
            int next;

            for(int iid = 0; iid < eventPreset.layerSetting.list.Length; iid++)
            {
                int layerId = eventPreset.layerSetting.list[iid].layerID;

                List<LightCollider2D> colliderList = LightCollider2D.GetShadowList(layerId);

                int colliderCount = colliderList.Count;

                for(int ci = 0; ci < colliderCount; ci++)
                {
                    LightCollider2D id = colliderList[ci];

                    if (id.InLight(light) == false) {
                        continue;
                    }

                    if (id.mainShape.shadowType == LightCollider2D.ShadowType.None)
                    {
                        continue;
                    }

                    List<Polygon2> polygons = id.mainShape.GetPolygonsWorld();

                    if (polygons.Count < 1) {
                        continue;
                    }
                    
                    removePointsCollidingCount = 0;
                    removeCollisionsCount = 0;
                    
                    for(int i = 0; i < polygons.Count; i++)
                    {
                        Vector2[] pointsList = polygons[i].points;
                        int pointsCount = pointsList.Length;

                        for(int x = 0; x < pointsCount; x++)
                        {
                            next = (x + 1) % pointsCount;

                            Vector2 left = pointsList[x];
                            Vector2 right = pointsList[next];

                            edgeLeft.x = left.x + lightPosition.x;
                            edgeLeft.y = left.y + lightPosition.y;

                            edgeRight.x = right.x + lightPosition.x;
                            edgeRight.y = right.y + lightPosition.y;

                            rotLeft = (float)System.Math.Atan2 (edgeLeft.y, edgeLeft.x);
                            rotRight = (float)System.Math.Atan2 (edgeRight.y, edgeRight.x);
                        
                            projectionLeft.x = edgeLeft.x + (float)System.Math.Cos(rotLeft) * lightSize;
                            projectionLeft.y = edgeLeft.y + (float)System.Math.Sin(rotLeft) * lightSize;

                            projectionRight.x = edgeRight.x + (float)System.Math.Cos(rotRight) * lightSize;
                            projectionRight.y = edgeRight.y + (float)System.Math.Sin(rotRight) * lightSize;

                            testPolygon.points[0] = projectionLeft;
                            testPolygon.points[1] = projectionRight;
                            testPolygon.points[2] = edgeRight;
                            testPolygon.points[3] = edgeLeft;

                            float collisionCount = collisions.Count;

                            for(int c = 0; c < collisionCount; c++)
                            {
                                LightCollision2D col = collisions[c];

                                if (col.collider == id) {
                                    continue;
                                }

                                // Check if event handling objects are inside shadow
                                // Add it to remove list
                                int pCount = col.points.Count;
                                for(int p = 0; p < pCount; p++)
                                {
                                    Vector2 point = col.points[p];

                                    if (Math2D.PointInPoly(point, testPolygon))
                                    {
                                        removePointsColliding[removePointsCollidingCount] = point;
                                        removePointsCollidingCount++;
                                    }
                                }

                                // Remove Event Handling points with remove list
                                for(int p = 0; p < removePointsCollidingCount; p++)
                                {
                                    col.points.Remove(removePointsColliding[p]);
                                }

                                removePointsCollidingCount = 0;

                                // If there is no points left
                                // collision object should be removed
                                if (col.points.Count < 1)
                                {
                                    removeCollisions[removeCollisionsCount] = col;
                                    removeCollisionsCount ++;
                                }
                            }

                            for(int p = 0; p < removeCollisionsCount; p++)
                            {
                                collisions.Remove(removeCollisions[p]);
                            }
                            
                            removeCollisionsCount = 0;
                        }
                    }
                }
            }

            return(collisions);
        }
    }
}