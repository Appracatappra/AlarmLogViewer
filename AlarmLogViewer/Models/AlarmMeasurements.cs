using System;
using System.Collections;
using System.Collections.Generic;
using AlarmLogViewer.Models;

namespace AlarmLogViewer
{
	/// <summary>
	/// Holds the collection of all measurements read from the trip data and calculates the duration of time that an 
	/// alarm state existed.
	/// </summary>
	public class AlarmMeasurements
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets the measurements for the given alarm channels.
		/// </summary>
		/// <value>The alarm channels.</value>
		public List<AlarmChannelMeasurements> AlarmChannels { get; set; } = new List<AlarmChannelMeasurements>();

		/// <summary>
		/// Gets the duration of alarm from events.
		/// </summary>
		/// <value>The duration of alarm from events.</value>
		public TimeSpan DurationOfAlarmFromEvents
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmChannelMeasurements channel in AlarmChannels)
				{
					// Is this measurement from an event?
					total += channel.DurationOfAlarmFromEvents;
				}

				// Return results
				return total;
			}
		}

		/// <summary>
		/// Gets the duration of alarm outside of events.
		/// </summary>
		/// <value>The duration of alarm outside of events.</value>
		public TimeSpan DurationOfAlarmOutsideOfEvents
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmChannelMeasurements channel in AlarmChannels)
				{
					// Is this measurement from an event?
					total += channel.DurationOfAlarmOutsideOfEvents;
				}

				// Return results
				return total;
			}
		}

		/// <summary>
		/// Gets the total duration of the alarm.
		/// </summary>
		/// <value>The total duration of the alarm.</value>
		public TimeSpan TotalAlarmDuration
		{
			get
			{
				// Return the total time in alarm
				return DurationOfAlarmFromEvents + DurationOfAlarmOutsideOfEvents;
			}
		}

		/// <summary>
		/// Gets the total time alarm exceeded maximum.
		/// </summary>
		/// <value>The total time alarm exceeded maximum.</value>
		public TimeSpan TotalTimeAlarmExceededMaximum
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmChannelMeasurements channel in AlarmChannels)
				{
					// Is this measurement from an event?
					total += channel.TotalTimeAlarmExceededMaximum;
				}

				// Return results
				return total;
			}
		}

		/// <summary>
		/// Gets the total time the alarm exceeded minimum.
		/// </summary>
		/// <value>The total time alarm exceeded minimum.</value>
		public TimeSpan TotalTimeAlarmExceededMinimum
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmChannelMeasurements channel in AlarmChannels)
				{
					// Is this measurement from an event?
					total += channel.TotalTimeAlarmExceededMinimum;
				}

				// Return results
				return total;
			}
		}

		/// <summary>
		/// Gets the human readable alarm description.
		/// </summary>
		/// <value>The alarm description.</value>
		public string AlarmDescription
		{
			get
			{
				// Build overview
				var desc = $"Overall Alarm Duration: {TotalAlarmDuration}\n" +
					$"Overall Event Duration: {DurationOfAlarmFromEvents}\n" +
					$"Overall Outside Event Duration: {DurationOfAlarmOutsideOfEvents}\n" +
					$"Overall Exceeded Maximum: {TotalTimeAlarmExceededMaximum}\n" +
					$"Overall Exceeded Minimum: {TotalTimeAlarmExceededMinimum}\n";

				return desc;
			}
		}

		/// <summary>
		/// Gets the menu item representing the overall results.
		/// </summary>
		/// <value>The menu item.</value>
		public Item menuItem
		{
			get
			{
				// Build and return an item
				return new Item()
				{
					Id = Guid.NewGuid().ToString(),
					Text = "All Alarms",
					Description = AlarmDescription
				};
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="T:AlarmLogViewer.AlarmMeasurements"/> class.
		/// </summary>
		public AlarmMeasurements()
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Finds an existing alarm channel measurement in our collection of channels or creates a new channel if
		/// one cannot be found.
		/// </summary>
		/// <returns>The matching channel or a new channel if one cannot be found.</returns>
		/// <param name="channel">The name of the channel to find.</param>
		public AlarmChannelMeasurements FindChannel(string channel)
		{
			// Check for an existing channel
			foreach(AlarmChannelMeasurements channelMeasurements in AlarmChannels)
			{
				if (channelMeasurements.Channel == channel)
				{
					// Found, return
					return channelMeasurements;
				}
			}

			// Not found, create a new measurement, add to our collection and return.
			AlarmChannelMeasurements newChannel = new AlarmChannelMeasurements() { 
				Channel = channel
			};
			AlarmChannels.Add(newChannel);
			return newChannel;
		}
		#endregion
	}
}
