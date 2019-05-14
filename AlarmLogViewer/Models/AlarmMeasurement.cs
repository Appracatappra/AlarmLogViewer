using System;
namespace AlarmLogViewer
{
	/// <summary>
	/// Holds an alarm state measurement read from either the trip data event log or from the uploaded trip data that
	/// was not already covered by an event recorded in the trip data events.
	/// </summary>
	public class AlarmMeasurement
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets the source of the condition.
		/// </summary>
		/// <value>The condition source.</value>
		public AlarmMeasurementSource ConditionSource { get; set; } = AlarmMeasurementSource.Unknown;

		/// <summary>
		/// Gets or sets the type of the condition that triggered the alarm.
		/// </summary>
		/// <value>The type of the condition.</value>
		public AlarmConditionType ConditionType { get; set; } = AlarmConditionType.Unknown;

		/// <summary>
		/// Gets or sets the date and time the condition started.
		/// </summary>
		/// <value>The condition started.</value>
		public DateTime ConditionStarted { get; set; } = DateTime.Now;

		/// <summary>
		/// Gets or sets the condition ended.
		/// </summary>
		/// <value>The condition ended.</value>
		public DateTime ConditionEnded { get; set; } = DateTime.Now;

		/// <summary>
		/// Gets or sets the value that started the condition.
		/// </summary>
		/// <value>The condition started value.</value>
		public double ConditionStartedValue { get; set; } = 0.0;

		/// <summary>
		/// Gets or sets the value that ended the condition.
		/// </summary>
		/// <value>The condition ended value.</value>
		public double ConditionEndedValue { get; set; } = 0.0;

		/// <summary>
		/// Gets the duration of the condition.
		/// </summary>
		/// <value>The duration of the condition.</value>
		public TimeSpan ConditionDuration
		{
			get
			{
				// Calculate the duration of the condition.
				return ConditionEnded - ConditionStarted;
			}
		}

		/// <summary>
		/// Gets the human readable description of the condition.
		/// </summary>
		/// <value>The condition description.</value>
		public string ConditionDescription
		{
			get
			{
				var source = "";
				var type = "";

				// Get source
				switch (ConditionSource)
				{
					case AlarmMeasurementSource.FromEvent:
						source = "From Event";
						break;
					case AlarmMeasurementSource.OutsideOfEvent:
						source = "Outside Event";
						break;
				}

				// Get type
				switch (ConditionType)
				{
					case AlarmConditionType.ExceededMaximumValue:
						type = "Exceeded Maximum";
						break;
					case AlarmConditionType.ExceededMinimumValue:
						type = "Exceeded Minimum";
						break;
				}

				return $"* {type} For {ConditionDuration} Value {ConditionStartedValue} To {ConditionEndedValue} {source}.\n";
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="T:AlarmLogViewer.AlarmMeasurement"/> class.
		/// </summary>
		public AlarmMeasurement()
		{
		}
		#endregion
	}
}
