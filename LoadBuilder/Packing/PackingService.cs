using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LoadBuilder.Packing.Algorithms;
using LoadBuilder.Packing.Entities;

namespace LoadBuilder.Packing
{
	public static class PackingService
	{
		public static List<ContainerPackingResult> Pack(string orderDocumentNumber, Container container,
			List<Item> itemsToPack, bool unloadingWithClamp,
			List<int> algorithmTypeIDs)
		{
			Object sync = new Object();
			List<ContainerPackingResult> result = new List<ContainerPackingResult>();

			var containerAmount = 0;

			int unpackedItemAmount = 0;
			do
			{
				unpackedItemAmount = 0;
				containerAmount++;
				
				ContainerPackingResult containerPackingResult = new ContainerPackingResult(containerAmount);
				containerPackingResult.ContainerId = container.ID;
				
				Parallel.ForEach(algorithmTypeIDs, algorithmTypeId =>
				{
					IPackingAlgorithm algorithm = GetPackingAlgorithmFromTypeId(algorithmTypeId);

					// Until I rewrite the algorithm with no side effects, we need to clone the item list
					// so the parallel updates don't interfere with each other.
					List<Item> items = new List<Item>();

					itemsToPack.ForEach(item =>
					{
						items.Add(new Item(item.ID, item.ItemId, item.Type, item.Dim1, item.Dim2, item.Dim3, item.RotationType, item.Quantity, item.LoadingType));
					});

					Stopwatch stopwatch = new Stopwatch();
					AlgorithmPackingResult algorithmResult = algorithm.Run(orderDocumentNumber, container, items, unloadingWithClamp, stopwatch);
					stopwatch.Stop();

					if (algorithmResult == null)
					{
						return;
					}

					algorithmResult.PackTimeInMilliseconds = stopwatch.ElapsedMilliseconds;

					decimal containerVolume = container.Length * container.Width * container.Height;
					decimal itemVolumePacked = algorithmResult.PackedItems.Sum(i => i.Volume);
					decimal itemVolumeUnpacked = algorithmResult.UnpackedItems.Sum(i => i.Volume);

					algorithmResult.PercentContainerVolumePacked = Math.Round(itemVolumePacked / containerVolume * 100, 2);
					algorithmResult.PercentItemVolumePacked = Math.Round(itemVolumePacked / (itemVolumePacked + itemVolumeUnpacked) * 100, 2);

					unpackedItemAmount = algorithmResult.UnpackedItems.Count;
					itemsToPack = algorithmResult.UnpackedItems;

					lock (sync)
					{
						containerPackingResult.AlgorithmPackingResults.Add(algorithmResult);
					}
				});

				containerPackingResult.AlgorithmPackingResults = containerPackingResult.AlgorithmPackingResults.OrderBy(r => r.AlgorithmName).ToList();

				lock (sync)
				{
					result.Add(containerPackingResult);
				}
			} while (unpackedItemAmount > 0);
			
			return result;
		}

		private static IPackingAlgorithm GetPackingAlgorithmFromTypeId(int algorithmTypeId)
		{
			switch (algorithmTypeId)
			{
				case (int)AlgorithmType.adaptiveheuristic:
					return new AdaptiveHeuristic();

				default:
					throw new Exception("Invalid algorithm type.");
			}
		}
	}
}