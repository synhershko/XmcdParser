using System.Collections.Generic;
using Raven.Imports.Newtonsoft.Json;

namespace XmcdParser
{
	public class Disk
	{
	    public string Id { get; set; }
		public string Title { get; set; }
		public string Artist { get; set; }
		public int DiskLength { get; set; }
		public string Genre { get; set; }
		public int? Year { get; set; }
		public List<string> FreeDBDiskIds { get; set; }

        [JsonIgnore]
		public List<int> TrackFramesOffsets { get; set; }
		public List<string> Tracks { get; set; }
		public Dictionary<string, string> Attributes { get; set; }
		
        public Disk()
		{
			TrackFramesOffsets = new List<int>();
			Tracks = new List<string>();
            FreeDBDiskIds = new List<string>();
			Attributes = new Dictionary<string, string>();
		}
	}
}