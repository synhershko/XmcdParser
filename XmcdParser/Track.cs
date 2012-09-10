using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmcdParser
{
    public class Track
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public int? Year { get; set; }
        public int? Length { get; set; }
        
        public string AlbumId { get; set; }
        public int TrackNo { get; set; }
    }
}
