#region Using directives

using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;
#endregion

namespace Mvp.Xml.Common.Serialization
{

	/// <summary>
	/// A PerfCounterManager manages the life cycle of performance
	/// counter instances and exposes methods to set the counter values.
	/// </summary>
	public class PerfCounterManager : IDisposable
	{
		/// <summary>
		/// Default Constructor. Instantiate a PerfCounterManager for the 
		/// life time of the Performance Counters.
		/// </summary>
		public PerfCounterManager()
		{
			CheckAndCreateCategory();
			CreateInstances();
			HitCounterInstanceCount = GlobalHitCounterInstanceCount++;
			InstanceCounterInstanceCount = GlobalInstanceCounterInstanceCount++;
		}

		internal const string CATEGORY = "Mvp.Xml.XmlSerializerCache";
		internal const string CATEGORY_DESCRIPTION = "Performance counters to instrument the classes from the Mvp.Xml project.";
		internal const string SERIALIZER_HITS_NAME = "Cache Hits";
		internal const string SERIALIZER_HITS_DESCRIPTION = "Number of times instances were retrieved from the serializer cache.";
		internal const string CACHED_INSTANCES_NAME = "Cached Instances";
		internal const string CACHED_INSTANCES_DESCRIPTION = "Number of XmlSerializer instances in the cache.";
 
		private void CheckAndCreateCategory()
		{
			if (!System.Diagnostics.PerformanceCounterCategory.Exists(CATEGORY))
			{
				CounterCreationDataCollection counters =
					new CounterCreationDataCollection();
				counters.Add(GetCacheHitCounterData());
				counters.Add(GetCachedInstancesCounterData());
				PerformanceCounterCategory.Create(
				   CATEGORY, CATEGORY_DESCRIPTION, PerformanceCounterCategoryType.MultiInstance, 
                   counters);
			}
		}

		private CounterCreationData GetCacheHitCounterData()
		{
			return new CounterCreationData(SERIALIZER_HITS_NAME
				, SERIALIZER_HITS_DESCRIPTION
				, PerformanceCounterType.NumberOfItems64);
		}

		private CounterCreationData GetCachedInstancesCounterData()
		{
			return new CounterCreationData(CACHED_INSTANCES_NAME
				, CACHED_INSTANCES_DESCRIPTION
				, PerformanceCounterType.NumberOfItems32);
		}

		private PerformanceCounter CacheHitCounter;
		private PerformanceCounter InstanceCounter;

		readonly int HitCounterInstanceCount;
		readonly int InstanceCounterInstanceCount;
		static int GlobalHitCounterInstanceCount;
		static int GlobalInstanceCounterInstanceCount;

		private void CreateInstances()
		{
			CacheHitCounter = new PerformanceCounter(CATEGORY
				, SERIALIZER_HITS_NAME
				, GetCounterInstanceName( HitCounterInstanceCount)
				, false );

			InstanceCounter = new PerformanceCounter(CATEGORY
				, CACHED_INSTANCES_NAME
				, GetCounterInstanceName( InstanceCounterInstanceCount)
				, false );
		}

		private string GetCounterInstanceName( int index )
		{
			string fileName = Environment.CommandLine.Split(' ')[0];
			foreach( char c in System.IO.Path.GetInvalidPathChars() )
			{
				fileName = fileName.Replace( c, '%' );
			}
			fileName = fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
			System.Diagnostics.Debug.WriteLine(String.Format("File name is: {0}", fileName));
			string name = String.Format("{0}-{1}#{2}"
					, fileName
					, AppDomain.CurrentDomain.FriendlyName
					, index );
			System.Diagnostics.Debug.WriteLine(string.Format("Created counter name: {0}"
				, name));
			return name;
		}

		/// <summary>
		/// Increments the counter for the cache hits.
		/// </summary>
		public void IncrementHitCount()
		{
			CacheHitCounter.Increment();
		}

		/// <summary>
		/// Increments the counter for the cached serializer instances
		/// </summary>
		public void IncrementInstanceCount()
		{
			InstanceCounter.Increment();
		}
	
		/// <summary>
		/// Finalizer to make sure counters are cleaned up 
		/// properly even if Dispose wasn't called.
		/// </summary>
		~PerfCounterManager()
		{
			Dispose(false);
		}

		private void Dispose(bool isDisposing)
		{
			try
			{
				CacheHitCounter.RemoveInstance();
			}
			catch{}
			try
			{
				InstanceCounter.RemoveInstance();
			}
			catch{}
		}

#region IDisposable Members

		/// <summary>
		/// Dispose method to clean up counter instances
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

#endregion
}
}
