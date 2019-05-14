using System;
using System.Linq;
using Interview.Problem1.Domain;

namespace AlarmLogViewer
{
	/// <summary>
	/// Static class used to parse and calculate alam states and durations from a given `tripdata.json` log file read
	/// into a Domain model.
	/// </summary>
	public static class TripDataProcessor
	{
		#region Static Properties
		/// <summary>
		/// Gets or sets the trip data read from the `tripdata.json` file as a domain model.
		/// </summary>
		/// <value>The trip data.</value>
		public static TripVm TripData { get; set; }

		/// <summary>
		/// Gets or sets the trip measurements computed from the trip data.
		/// </summary>
		/// <value>The trip measurements.</value>
		public static AlarmMeasurements TripMeasurements { get; set; }
		#endregion

		#region Static Methods
		/// <summary>
		/// Parses the trip data accumulating any alarm measurements that exist in the data.
		/// </summary>
		public static void ParseTripData()
		{
			AlarmMeasurements measurements = new AlarmMeasurements();
			AlarmChannelMeasurements channelMeasurements = new AlarmChannelMeasurements();
			AlarmMeasurement measurement = new AlarmMeasurement();
			double dataValue = 0.0;

			// Sort the event data first by channel, then by time
			var events = TripData.TripUploadEvents.OrderBy(a => a.Channel).ThenBy(a => a.Timestamp).ToList();

			// Process all events
			foreach(DeviceAlarmVm alarm in events)
			{
				// Get the current data value
				dataValue = double.Parse(alarm.AlarmData);

				// Has this channel already been seen?
				if (channelMeasurements.Channel == "")
				{
					// No, this channel hasn't been seen before, add it to the collection.
					channelMeasurements.Channel = alarm.Channel;
					SetupChannelMeasurement(channelMeasurements);
					measurements.AlarmChannels.Add(channelMeasurements);

				} else if (channelMeasurements.Channel != alarm.Channel)
				{
					// Do we have an open event without a matching out state?
					FinalizeMeasurement(measurement, dataValue);

					// Processing a new channel.
					channelMeasurements = new AlarmChannelMeasurements() {
						Channel = alarm.Channel
					};
					SetupChannelMeasurement(channelMeasurements);
					measurements.AlarmChannels.Add(channelMeasurements);
				}

				// Take action based on the alarm type
				switch(alarm.EventType)
				{
					case 6:
						// MaxAlarmOut - A maximum alarm state has started.
						// Close any open measurements.
						FinalizeMeasurement(measurement, dataValue);

						// Start a new event for this measurement and add to collection.
						measurement = new AlarmMeasurement()
						{
							ConditionSource = AlarmMeasurementSource.FromEvent,
							ConditionType = AlarmConditionType.ExceededMaximumValue,
							ConditionStarted = alarm.Timestamp,
							ConditionStartedValue = dataValue
						};
						channelMeasurements.Measurements.Add(measurement);
						break;
					case 7:
						// MinAlarmOut - A minimum alarm state has started.
						// Close any open measurements.
						FinalizeMeasurement(measurement, dataValue);

						// Start a new event for this measurement and add to collection.
						measurement = new AlarmMeasurement()
						{
							ConditionSource = AlarmMeasurementSource.FromEvent,
							ConditionType = AlarmConditionType.ExceededMinimumValue,
							ConditionStarted = alarm.Timestamp,
							ConditionStartedValue = dataValue
						};
						channelMeasurements.Measurements.Add(measurement);
						break;
					case 8:
						// MaxAlarmIn - A maximum alarm state has ended.
						measurement.ConditionEnded = alarm.Timestamp;
						measurement.ConditionEndedValue = dataValue;
						measurement = new AlarmMeasurement();
						break;
					case 9:
						// MinAlarmIn - A minimum alarm state has ended.
						measurement.ConditionEnded = alarm.Timestamp;
						measurement.ConditionEndedValue = dataValue;
						measurement = new AlarmMeasurement();
						break;
				}
			}

			// Close any dangling open events
			FinalizeMeasurement(measurement, dataValue);

			// Sort the upload data first by channel, then by time
			var data = TripData.TripUploadData.OrderBy(a => a.Channel).ThenBy(a => a.Timestamp).ToList();

			// Check for any alarm events that might have occurred while the alarm memory was overrun.
			channelMeasurements = new AlarmChannelMeasurements();
			measurement = new AlarmMeasurement();
			dataValue = 0.0;
			foreach(DeviceDataVm dataPoint in data) {
				// Get current datapoint
				dataValue = dataPoint.Data;

				// Are we working on the same channel?
				if (dataPoint.Channel != channelMeasurements.Channel)
				{
					// No, finalize any existing measurements and record.
					FinalizeMeasurement(measurement, dataValue);
					channelMeasurements.AccumulateMeasurement(measurement);
					measurement = new AlarmMeasurement();

					// Find or create the channel
					channelMeasurements = measurements.FindChannel(dataPoint.Channel);

					// Have we seen this channel before?
					if (channelMeasurements.DataType == "")
					{
						// No, configure the channel
						SetupChannelMeasurement(channelMeasurements);
					}
				}

				// Is the data measurement outside of the given range?
				if (dataPoint.Data >= channelMeasurements.Max)
				{
					// Do we have an open event?
					switch(measurement.ConditionType)
					{
						case AlarmConditionType.Unknown:
							// No, start a new event
							measurement.ConditionSource = AlarmMeasurementSource.OutsideOfEvent;
							measurement.ConditionType = AlarmConditionType.ExceededMaximumValue;
							measurement.ConditionStarted = dataPoint.Timestamp;
							measurement.ConditionStartedValue = dataValue;
							break;
						case AlarmConditionType.ExceededMinimumValue:
							// We have an open minimum condition, we need to close and save it.
							measurement.ConditionEnded = dataPoint.Timestamp;
							measurement.ConditionEndedValue = dataValue;
							channelMeasurements.AccumulateMeasurement(measurement);

							// Start a measurement for this alarm state.
							measurement = new AlarmMeasurement() {
								ConditionSource = AlarmMeasurementSource.OutsideOfEvent,
								ConditionType = AlarmConditionType.ExceededMaximumValue,
								ConditionStarted = dataPoint.Timestamp,
								ConditionStartedValue = dataValue
							};
							break;
						default:
							// Still inside of the alarm state, continue accumulating.
							break;
					}

				} else if (dataPoint.Data <= channelMeasurements.Min) {
					// Do we have an open event?
					switch (measurement.ConditionType)
					{
						case AlarmConditionType.Unknown:
							// No, start a new event
							measurement.ConditionSource = AlarmMeasurementSource.OutsideOfEvent;
							measurement.ConditionType = AlarmConditionType.ExceededMinimumValue;
							measurement.ConditionStarted = dataPoint.Timestamp;
							measurement.ConditionStartedValue = dataValue;
							break;
						case AlarmConditionType.ExceededMaximumValue:
							// We have an open minimum condition, we need to close and save it.
							measurement.ConditionEnded = dataPoint.Timestamp;
							measurement.ConditionEndedValue = dataValue;
							channelMeasurements.AccumulateMeasurement(measurement);

							// Start a measurement for this alarm state.
							measurement = new AlarmMeasurement()
							{
								ConditionSource = AlarmMeasurementSource.OutsideOfEvent,
								ConditionType = AlarmConditionType.ExceededMinimumValue,
								ConditionStarted = dataPoint.Timestamp,
								ConditionStartedValue = dataValue
							};
							break;
						default:
							// Still inside of the alarm state, continue accumulating.
							break;
					}
				} else {
					// Value back inside of allowed range, close any open alarm events.
					if (measurement.ConditionSource != AlarmMeasurementSource.Unknown)
					{
						// Close, save and start a new measurement
						measurement.ConditionEnded = dataPoint.Timestamp;
						measurement.ConditionEndedValue = dataValue;
						channelMeasurements.AccumulateMeasurement(measurement);
						measurement = new AlarmMeasurement();
					}
				}
			}

			// Close any dangling open events
			if (measurement.ConditionSource != AlarmMeasurementSource.Unknown)
			{
				// Finalize any existing measurements and record.
				FinalizeMeasurement(measurement, dataValue);
				channelMeasurements.AccumulateMeasurement(measurement);
			}

			// Save the final results
			TripMeasurements = measurements;
		}

		/// <summary>
		/// Finalizes the measurement by closing any open "in" measurement event without a matching "out" event by using
		/// the endtime of the trip data.
		/// </summary>
		/// <param name="measurement">The current measurement to finalize.</param>
		/// <param name="dataValue">The default value to close an open measurement on.</param>
		private static void FinalizeMeasurement(AlarmMeasurement measurement, double dataValue = 0.0)
		{
			// Do we have an open event without a matching out state?
			if (measurement.ConditionType != AlarmConditionType.Unknown)
			{
				// Yes, assume data collection ended before the alarm was closed and use
				// the endtime of the trip data.
				measurement.ConditionEnded = TripData.EndTime;

				// Save closing measurement
				if (dataValue == 0.0)
				{
					// The value wasn't known, assume it's still the same as the starting value.
					measurement.ConditionEndedValue = measurement.ConditionStartedValue;
				} else
				{
					// Save ending condition
					measurement.ConditionEndedValue = dataValue;
				}
			}
		}

		/// <summary>
		/// Setup the channel measurement object by copying over the data from the matching setting.
		/// </summary>
		/// <param name="channelMeasurements">The channel measurement to setup.</param>
		/// <remarks>This information will be used to find alarm events that might have been missed if the alarm memory 
		/// was overrun on the device and to "pretty print" the results.</remarks>
		private static void SetupChannelMeasurement(AlarmChannelMeasurements channelMeasurements)
		{
			// Find setting that match this channel
			foreach(TripSettingVm setting in TripData.TripSettings)
			{
				// Found matching channel?
				if (setting.ChannelName == channelMeasurements.Channel)
				{
					// Yes, copy over the required data
					channelMeasurements.DataType = setting.DataType;
					channelMeasurements.Max = setting.Max;
					channelMeasurements.Min = setting.Min;
					return;
				}
			}
		}
		#endregion

	}
}
