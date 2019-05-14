using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlarmLogViewer.Models;

namespace AlarmLogViewer.Services
{
	public class MockDataStore : IDataStore<Item>
	{
		List<Item> items;

		public MockDataStore()
		{
			items = new List<Item>();

			// Build overall results view
			items.Add(TripDataProcessor.TripMeasurements.menuItem);

			// Add items for each channel of results we accumulated
			foreach(AlarmChannelMeasurements channelMeasurements in TripDataProcessor.TripMeasurements.AlarmChannels)
			{
				// Add channel details
				items.Add(channelMeasurements.menuItem);
			}

		}

		public async Task<bool> AddItemAsync(Item item)
		{
			items.Add(item);

			return await Task.FromResult(true);
		}

		public async Task<bool> UpdateItemAsync(Item item)
		{
			var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
			items.Remove(oldItem);
			items.Add(item);

			return await Task.FromResult(true);
		}

		public async Task<bool> DeleteItemAsync(string id)
		{
			var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
			items.Remove(oldItem);

			return await Task.FromResult(true);
		}

		public async Task<Item> GetItemAsync(string id)
		{
			return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
		}

		public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
		{
			return await Task.FromResult(items);
		}
	}
}