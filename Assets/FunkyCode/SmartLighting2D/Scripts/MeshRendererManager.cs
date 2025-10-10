using UnityEngine;

namespace FunkyCode
{
	[ExecuteInEditMode]
	public class MeshRendererManager
	{
		// management
		static public LightingMeshRenderer AddBuffer(UnityEngine.Object source)
		{
			GameObject meshRenderer = new GameObject ("Mesh Renderer (Id :" + (LightingMeshRenderer.GetCount() + 1) + ")");
			meshRenderer.transform.parent = LightingManager2D.Get().transform;

			if (Lighting2D.ProjectSettings.managerInternal == LightingSettings.ManagerInternal.HideInHierarchy)
			{
				meshRenderer.hideFlags = HideFlags.HideInHierarchy;
			}
				else
			{
				meshRenderer.hideFlags = HideFlags.None;
			}

			LightingMeshRenderer LightBuffer2D = meshRenderer.AddComponent<LightingMeshRenderer>();
			
			LightBuffer2D.Initialize ();

			LightBuffer2D.owner = source;
			LightBuffer2D.free = false;

			return(LightBuffer2D);
		}

		public static LightingMeshRenderer Pull(UnityEngine.Object source)
		{
			foreach (LightingMeshRenderer id in LightingMeshRenderer.List)
			{
				if (id.owner == source)
				{
					id.gameObject.SetActive(true);

					return(id);
				}
			}

			foreach (LightingMeshRenderer id in LightingMeshRenderer.List)
			{
				if (id.free)
				{
					id.free = false;
					id.owner = source;
					id.gameObject.SetActive(true);

					return(id);
				}
			}
				
			return(AddBuffer(source));		
		}
	}
}