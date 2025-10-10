namespace FunkyCode.Rendering.Light.Sorting
{
	public struct SortObject : System.Collections.Generic.IComparer<SortObject>
	{
		public enum Type {Collider, Tile, TilemapMap};

		public Type type;
		public float value; // value

		public object lightObject;

		public LightTilemapCollider2D tilemap;

		public SortObject(int a)
		{
			type = Type.Collider;

			value = 0;

			lightObject = null;

			tilemap = null;
		}

		public int Compare(SortObject a, SortObject b)
		{
			if (a.value > b.value)
			{
				return 1;
			}

			return a.value < b.value ? -1 : 0;
		}

		private static System.Collections.Generic.IComparer<SortObject> comparer = (System.Collections.Generic.IComparer<SortObject>) new SortObject();

		public static System.Collections.Generic.IComparer<SortObject> Sort()
		{ 
			return (comparer);
		}
	}
}