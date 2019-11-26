using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Mvp.Xml.Common.Serialization
{
	/// <summary>
	/// Summary description for Installer1.
	/// </summary>
	[RunInstaller(true)]
	public class PerfCounterInstaller : System.Configuration.Install.Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Instantiates the custom installer class
		/// </summary>
		public PerfCounterInstaller()
		{
			System.Diagnostics.Debug.WriteLine( "PerfCounterInstaller: done" );
			// This call is required by the Designer.
			InitializeComponent();

			try
			{
				// Create an instance of 'PerformanceCounterInstaller'.
				PerformanceCounterInstaller performanceCounterInstaller =
					new PerformanceCounterInstaller();
				
				// Set the 'CategoryName' for performance counter.
				performanceCounterInstaller.CategoryName = PerfCounterManager.CATEGORY;

				CounterCreationData cacheHitCounterCreation = new CounterCreationData();
				cacheHitCounterCreation.CounterName = PerfCounterManager.SERIALIZER_HITS_NAME;
				cacheHitCounterCreation.CounterHelp = PerfCounterManager.SERIALIZER_HITS_DESCRIPTION;
				cacheHitCounterCreation.CounterType = PerformanceCounterType.NumberOfItems64;

				CounterCreationData cachedInstancesCounterCreation = new CounterCreationData();
				cachedInstancesCounterCreation.CounterName = PerfCounterManager.CACHED_INSTANCES_NAME;
				cachedInstancesCounterCreation.CounterHelp = PerfCounterManager.CACHED_INSTANCES_DESCRIPTION;
				cachedInstancesCounterCreation.CounterType = PerformanceCounterType.NumberOfItems64;

				// Add a counter to collection of  performanceCounterInstaller.
				performanceCounterInstaller.Counters.Add(cacheHitCounterCreation);
				performanceCounterInstaller.Counters.Add(cachedInstancesCounterCreation);
				Installers.Add(performanceCounterInstaller);
				System.Diagnostics.Debug.WriteLine( "PerfCounterInstaller: Added counters and category" );
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("PerfCounterInstaller Error occured :"+e.Message);
			}
			System.Diagnostics.Debug.WriteLine( "PerfCounterInstaller done" );

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
