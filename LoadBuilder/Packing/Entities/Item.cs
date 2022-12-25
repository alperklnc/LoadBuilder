using System;

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

		public int FinalRotationType { get; set; }

		public RotationType RotationType { get; set; }

		public string LoadingType { get; set; }

		public string ItemId { get; set; }

		public Item(int id, string itemId, string type, decimal dim1, decimal dim2, decimal dim3, RotationType rotationType = RotationType.Full, int quantity = 0, string loadingType = "")
		{
			ID = id;
			ItemId = itemId;
			Type = type;
			Dim1 = dim1;
			Dim2 = dim2;
			Dim3 = dim3;
			LoadingType = loadingType; 
			Volume = dim1 * dim2 * dim3;
			this.RotationType = rotationType;
			Quantity = quantity;
		}

		public void SetRotationType(string rotationType)
		{
			switch (rotationType)
			{
				case Entities.LoadingType.FullLoading:
					RotationType = RotationType.Full;
					break;
				case Entities.LoadingType.WithoutHorizontal:
					RotationType = RotationType.OnlyVertical;
					break;
				case Entities.LoadingType.UnloadingWithClamp:
					RotationType = RotationType.OnlyDefault;
					break;
				default:
					Console.WriteLine("Missing Rotation Type");
					break;
			}
		}

		public static RotationType? GetRotationType(string loadingType)
		{
			switch (loadingType)
			{
				case Entities.LoadingType.FullLoading:
					return RotationType.Full;
				case Entities.LoadingType.WithoutHorizontal:
					return RotationType.OnlyVertical;
				case Entities.LoadingType.UnloadingWithClamp:
					return RotationType.OnlyDefault;
			}

			return null;
		}

		public bool IsBar()
		{
			return Type == "Bar";
		}
	}
}