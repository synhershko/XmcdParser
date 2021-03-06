﻿using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Raven.Client.Document;

namespace XmcdParser
{
	class Program
	{
		private const int BatchSize = 24;
		static void Main()
		{
			using (var store = new DocumentStore
			{
				Url = "http://localhost:8080", DefaultDatabase = "Music"
			}.Initialize())
			{
				var session = store.OpenSession();
				var count = 0;

				var sp = ParseDisks(diskToAdd =>
				{
					session.Store(diskToAdd);
					count += 1;

                    diskToAdd.TrackFramesOffsets.Add(diskToAdd.DiskLength * 75);
                    bool hasLengthInfo = diskToAdd.TrackFramesOffsets.Count == diskToAdd.Tracks.Count + 1;
				    for (int i = 0; i < diskToAdd.Tracks.Count; i++)
				    {
                        string artist = diskToAdd.Artist, title = diskToAdd.Tracks[i];
                        if (!string.IsNullOrWhiteSpace(diskToAdd.Tracks[i]) && diskToAdd.Tracks[i].Contains(" / "))
                        {
                            var tmp = diskToAdd.Tracks[i].Split(new[] {" / "}, StringSplitOptions.None);
                            artist = tmp[0];
                            title = tmp[1];
                        }

				        var track = new Track
				                        {
				                            AlbumId = diskToAdd.Id,
				                            TrackNo = i + 1,
                                            Artist = artist,
                                            Genre = diskToAdd.Genre,
                                            Title = title,
                                            Year = diskToAdd.Year,
                                            Length = hasLengthInfo ? (diskToAdd.TrackFramesOffsets[i + 1] - diskToAdd.TrackFramesOffsets[i]) / 75 : (int?)null,
				                        };
                        session.Store(track);
				    }

					if (count < BatchSize) 
						return;

					session.SaveChanges();
					session = store.OpenSession();
					count = 0;
				});

				session.SaveChanges();

				Console.WriteLine();
				Console.WriteLine("Done in {0}", sp.Elapsed);
			}
		}

		private static Stopwatch ParseDisks(Action<Disk> addToBatch)
		{
			int i = 0;
			var parser = new Parser();
			var buffer = new byte[1024*1024];// more than big enough for all files

			var sp = Stopwatch.StartNew();

			using (var bz2 = new BZip2InputStream(File.Open(@"z:\freedb-complete-20120901.tar.bz2", FileMode.Open)))
			using (var tar = new TarInputStream(bz2))
			{
				TarEntry entry;
				while((entry=tar.GetNextEntry()) != null)
				{
					if(entry.Size == 0 || entry.Name == "README" || entry.Name == "COPYING")
						continue;
					var readSoFar = 0;
					while(true)
					{
						var read = tar.Read(buffer, readSoFar, ((int) entry.Size) - readSoFar);
						if (read == 0)
							break;

						readSoFar += read;
					}
					// we do it in this fashion to have the stream reader detect the BOM / unicode / other stuff
					// so we can read the values properly
					var fileText = new StreamReader(new MemoryStream(buffer,0, readSoFar)).ReadToEnd();
					try
					{
						var disk = parser.Parse(fileText);
						addToBatch(disk);
						if (i++ % BatchSize == 0)
							Console.Write("\r{0} {1:#,#}  {2}         ", entry.Name, i, sp.Elapsed);
					}
					catch (Exception e)
					{
						Console.WriteLine();
						Console.WriteLine(entry.Name);
						Console.WriteLine(e);
						return sp;
					}
				}
			}
			return sp;
		}
	}
}