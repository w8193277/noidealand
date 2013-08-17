using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;


public static class FileUtilities{
	/// <summary>
    ///    Replaces a line of text with the specified text.
    /// </summary>
	public static void ReplaceLine(string filePath, string searchText, string replaceText )
	{
		var content = string.Empty;
		using (StreamReader reader = new StreamReader( filePath ))
		{
			content = reader.ReadToEnd();
			reader.Close();
		}
		
		File.WriteAllText(filePath, File.ReadAllText(filePath).Replace(searchText, replaceText));
		
		using (StreamWriter writer = new StreamWriter( filePath ))
		{
			writer.Write( content );
			writer.Close();
		}
	}
	/// <summary>
    ///    Creates a new file with the specified parameters.
    /// </summary>
	public static void CreateNewFile(string filePath, string fileName, string extension)
	{
		if(!File.Exists(filePath + fileName + extension))
		{
			File.Create(filePath + fileName + extension);
		}
		else
		{
			return;
		}
	}
}


