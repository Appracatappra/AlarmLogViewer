using System;
namespace AlarmLogViewer
{
	/// <summary>
	/// Defines the type of condition that triggered the alarm.
	/// </summary>
	public enum AlarmConditionType
	{
		/// <summary>
		/// The condition that triggered the alarm is unknown.
		/// </summary>
		Unknown,

		/// <summary>
		/// The measured value exceeded the maximum value specified by the alarm settings.
		/// </summary>
		ExceededMaximumValue,

		/// <summary>
		/// The measured value exceeded the minimum value specified by the alarm settings.
		/// </summary>
		ExceededMinimumValue
	}
}
