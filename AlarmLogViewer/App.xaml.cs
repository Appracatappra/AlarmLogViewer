using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AlarmLogViewer.Services;
using AlarmLogViewer.Views;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Interview.Problem1.Domain;

namespace AlarmLogViewer
{
	public partial class App : Application
	{

		public App()
		{
			InitializeComponent();

			DependencyService.Register<MockDataStore>();
			MainPage = new MainPage();
		}

		protected override void OnStart()
		{
			// There are two parts to this problem, the first is to compute the duration 
			// in alarm from the events, and the second is to compute the duration in alarm based on 
			// measurements in the data that are outside the time intervals covered by the events. 

			// Load trip data log from an embedded resource
			var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
			Stream stream = assembly.GetManifestResourceStream("AlarmLogViewer.Data.tripdata.json");
			string json = "";
			using (var reader = new System.IO.StreamReader(stream))
			{
				json = reader.ReadToEnd();
			}

			// Deserialize data into domain model
			TripDataProcessor.TripData = JsonConvert.DeserializeObject<TripVm>(json);
			Console.WriteLine($"Loaded Trip Data: {TripDataProcessor.TripData.Name}");

			// Parse domain model to generate alarm data
			TripDataProcessor.ParseTripData();
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
