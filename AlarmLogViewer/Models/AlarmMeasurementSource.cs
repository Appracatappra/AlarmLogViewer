using System;
namespace AlarmLogViewer
{
	/// <summary>
	/// Specifies the source of an alarm measurement. 
	/// </summary>
	public enum AlarmMeasurementSource
	{
		/// <summary>
		/// The source of the alarm measurement is unknown.
		/// </summary>
		Unknown,

		/// <summary>
		/// The alarm measurement was read from an event recorded in the trip data.
		/// </summary>
		FromEvent,

		/// <summary>
		/// The alarm measurement was read from the trip upload data that was outside of the 
		/// specified parameters not already covered by a recorded trip log event.
		/// </summary>
		OutsideOfEvent
	}
}
