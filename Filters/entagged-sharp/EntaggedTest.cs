using System;
using System.IO;
using Entagged;

public class EntaggedTest
{
    public static void Main(string [] args)
    {
        string testDir = args.Length > 0 ?
            args[0] :
            "../tests/samples";
    
        foreach(string file in Directory.GetFiles(testDir)) {
           // try {
                AudioFile af = new AudioFile(file);
		Console.WriteLine (af);
		/*
		Console.WriteLine(af.Title);
		Console.WriteLine(af.Album);
		foreach (string artist in af.Artists)
			System.Console.WriteLine ("Artist: " + artist);
		*/
          //  } catch(Exception) {}
        }
    }
}
