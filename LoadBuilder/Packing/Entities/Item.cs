namespace LoadBuilder.Packing.Entities
{
	public class Item
	{
		public int ID { get; set; }

		public string Type { get; set; }
		
		public bool IsPacked { get; set; }
		
		public decimal Dim1 { get; set; }
		
		public decimal Dim2 { get; set; }
		
		public decimal Dim3 { get; set; }
		
		public decimal CoordX { get; set; }
		
		public decimal CoordY { get; set; }
		
		public decimal CoordZ { get; set; }
		
		public int Quantity { get; set; }
		
		public decimal PackDimX { get; set; }
		
		public decimal PackDimY { get; set; }
		
		public decimal PackDimZ { get; set; }
		
		public decimal Volume { get; }

		public bool IsFullRotationAllowed { get; set; }

		public int RotationType { get; set; }

		public Item(int id, string type, decimal dim1, decimal dim2, decimal dim3, bool isFullRotationAllowed = true, int quantity = 0)
		{
			ID = id;
			Type = type;
			Dim1 = dim1;
			Dim2 = dim2;
			Dim3 = dim3;
			Volume = dim1 * dim2 * dim3;
			IsFullRotationAllowed = isFullRotationAllowed;
			Quantity = quantity;
		}
	}
}