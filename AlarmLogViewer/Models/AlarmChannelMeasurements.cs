using System;
using System.Collections;
using System.Collections.Generic;
using AlarmLogViewer.Models;

namespace AlarmLogViewer
{
	/// <summary>
	/// Holds all of the alam measurement read from the trip data for a given channel and the information to "pretty print"
	/// the results.
	/// </summary>
	public class AlarmChannelMeasurements
	{
		#region Public Properties
		/// <summary>
		/// Device channel triggering the alarm
		/// </summary>
		public string Channel { get; set; } = "";

		/// <summary>
		/// Data type for a specific channel on the data logger
		/// </summary>
		public string DataType { get; set; } = "";

		/// <summary>
		/// Min alarm setting for the channel on this trip
		/// </summary>
		public double Min { get; set; } = 0.0;

		/// <summary>
		/// Max alarm setting for the channel on this trip
		/// </summary>
		public double Max { get; set; } = 0.0;

		/// <summary>
		/// Gets or sets the measurements that were in alarm for this channel.
		/// </summary>
		/// <value>The measurements.</value>
		public List<AlarmMeasurement> Measurements { get; set; } = new List<AlarmMeasurement>();

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
				foreach (AlarmMeasurement measurement in Measurements)
				{
					// Is this measurement from an event?
					if (measurement.ConditionSource == AlarmMeasurementSource.FromEvent)
					{
						// Yes, accumulate value
						total += measurement.ConditionDuration;
					}
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
				foreach (AlarmMeasurement measurement in Measurements)
				{
					// Is this measurement from an event?
					if (measurement.ConditionSource == AlarmMeasurementSource.OutsideOfEvent)
					{
						// Yes, accumulate value
						total += measurement.ConditionDuration;
					}
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
		/// Gets the total time alarm exceeded the maximum allowed value.
		/// </summary>
		/// <value>The total time alarm exceeded maximum.</value>
		public TimeSpan TotalTimeAlarmExceededMaximum
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmMeasurement measurement in Measurements)
				{
					// Is this measurement from an event?
					if (measurement.ConditionType == AlarmConditionType.ExceededMaximumValue)
					{
						// Yes, accumulate value
						total += measurement.ConditionDuration;
					}
				}

				// Return results
				return total;
			}
		}

		/// <summary>
		/// Gets the total time alarm exceeded the minimum allowed value.
		/// </summary>
		/// <value>The total time alarm exceeded minimum.</value>
		public TimeSpan TotalTimeAlarmExceededMinimum
		{
			get
			{
				TimeSpan total = new TimeSpan();

				// Compute the total time time in events
				foreach (AlarmMeasurement measurement in Measurements)
				{
					// Is this measurement from an event?
					if (measurement.ConditionType == AlarmConditionType.ExceededMinimumValue)
					{
						// Yes, accumulate value
						total += measurement.ConditionDuration;
					}
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
				var desc = $"Total Duration: {TotalAlarmDuration}\n" + 
					$"Event Duration: {DurationOfAlarmFromEvents}\n" +
					$"Outside Event Duration: {DurationOfAlarmOutsideOfEvents}\n" +
					$"Total Exceeded Maximum: {TotalTimeAlarmExceededMaximum}\n" +
					$"Total Exceeded Minimum: {TotalTimeAlarmExceededMinimum}\n\n" +
					"DETAILS\n";

				// Include details
				foreach(AlarmMeasurement measurement in Measurements)
				{
					// Add description
					desc += measurement.ConditionDescription;
				}

				return desc;
			}
		}

		/// <summary>
		/// Gets the menu item that represents this channel.
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
					Text = $"{DataType} Alarms",
					Description = AlarmDescription
				};
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="T:AlarmLogViewer.AlarmChannelMeasurements"/> class.
		/// </summary>
		public AlarmChannelMeasurements()
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Accumulates a new alarm state measurement handling states where the new measurement overlaps an existing measurement in the collection.
		/// </summary>
		/// <param name="measurement">The new measurement to add to the collection.</param>
		public void AccumulateMeasurement(AlarmMeasurement measurement)
		{
			// If the measurement source is unknown, this is an incomplete measurement that does not need to be recorded.
			if (measurement.ConditionSource == AlarmMeasurementSource.Unknown) return;

			// Is there already an event that covers this measurement?
			foreach(AlarmMeasurement existingMeasurement in Measurements)
			{
				// Is the new event before or after this event?
				if (measurement.ConditionEnded < existingMeasurement.ConditionStarted || measurement.ConditionStarted > existingMeasurement.ConditionEnded)
				{
					// Yes, this could still be a good measurement so continue processing.
				} else if ( measurement.ConditionStarted >= existingMeasurement.ConditionStarted && measurement.ConditionStarted <= existingMeasurement.ConditionEnded
					&& measurement.ConditionEnded >= existingMeasurement.ConditionStarted && measurement.ConditionEnded <= existingMeasurement.ConditionEnded)
				{
					// The new measurement is wholy contained within an existing measurement so we don't need to record it.
					return;
				} else if (existingMeasurement.ConditionStarted >= measurement.ConditionStarted && existingMeasurement.ConditionStarted <= measurement.ConditionEnded
				  && existingMeasurement.ConditionEnded >= measurement.ConditionStarted && existingMeasurement.ConditionEnded <= measurement.ConditionEnded)
				{
					// The existing measurement is wholy contained within the new measurement.
					// TODO: I am making an assumption that we need to make new events that cover the section of time outside of the existing event
					// to trap the alarm state that might have been missed due to an alarm memory overrun state on the recording device.

					// Data before event
					Measurements.Add(new AlarmMeasurement() {
						ConditionSource = measurement.ConditionSource,
						ConditionType = measurement.ConditionType,
						ConditionStarted = measurement.ConditionStarted,
						ConditionEnded = existingMeasurement.ConditionStarted,
						ConditionStartedValue = measurement.ConditionStartedValue,
						ConditionEndedValue = existingMeasurement.ConditionStartedValue
					 });

					// Data after event
					Measurements.Add(new AlarmMeasurement()
					{
						ConditionSource = measurement.ConditionSource,
						ConditionType = measurement.ConditionType,
						ConditionStarted = existingMeasurement.ConditionEnded,
						ConditionEnded = measurement.ConditionEnded,
						ConditionStartedValue = existingMeasurement.ConditionEndedValue,
						ConditionEndedValue = measurement.ConditionEndedValue
					});

					// Finished processing
					return;
				} else if (measurement.ConditionStarted < existingMeasurement.ConditionStarted && measurement.ConditionEnded > existingMeasurement.ConditionStarted)
				{
					// The new measurement overlaps the start of an existing measurement.
					// TODO: I am making an assumption that we need to make new events that cover the section of time outside of the existing event
					// to trap the alarm state that might have been missed due to an alarm memory overrun state on the recording device.

					// Data before event
					Measurements.Add(new AlarmMeasurement()
					{
						ConditionSource = measurement.ConditionSource,
						ConditionType = measurement.ConditionType,
						ConditionStarted = measurement.ConditionStarted,
						ConditionEnded = existingMeasurement.ConditionStarted,
						ConditionStartedValue = measurement.ConditionStartedValue,
						ConditionEndedValue = existingMeasurement.ConditionStartedValue
					});

					// Finished processing
					return;
				} else if (measurement.ConditionStarted < existingMeasurement.ConditionEnded && measurement.ConditionEnded > existingMeasurement.ConditionEnded)
				{
					// The new measurement overlaps the end of an existing measurement.
					// TODO: I am making an assumption that we need to make new events that cover the section of time outside of the existing event
					// to trap the alarm state that might have been missed due to an alarm memory overrun state on the recording device.

					// Data before event
					Measurements.Add(new AlarmMeasurement()
					{
						ConditionSource = measurement.ConditionSource,
						ConditionType = measurement.ConditionType,
						ConditionStarted = measurement.ConditionStarted,
						ConditionEnded = existingMeasurement.ConditionStarted,
						ConditionStartedValue = existingMeasurement.ConditionEndedValue,
						ConditionEndedValue = measurement.ConditionEndedValue
					});

					// Finished processing
					return;
				}
			} 

			// The new measurement doesn't overlap any existing measurement so we can record it.
			Measurements.Add(measurement);
		}
		#endregion
	}
}
