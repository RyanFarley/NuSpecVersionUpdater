using System;
using System.IO;
using System.Xml;
using System.Reflection;

namespace NuSpecVersionUpdater
{
	enum ExitCode : int
	{
		Success = 0,
		IncorrectFunction = 1,
		InvalidFilename = 2,
		UnknownError = 10
	}

	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("USAGE: NuSpecVersionUpdater NUSPECFILE ASSEMBLYFILE");
				return (int)ExitCode.IncorrectFunction;
			}

			var specFile = args[0];
			var binFile = args[1];

			if (specFile == null || !File.Exists(specFile) || binFile == null || !File.Exists(binFile))
			{
				Console.WriteLine("Invalid file parameters passed. USAGE: NuSpecVersionUpdater NUSPECFILE ASSEMBLYFILE");
				return (int)ExitCode.InvalidFilename;
			}

			try
			{
				if (!UpdateSpecFile(specFile, GetFileVersion(binFile)))
				{
					Console.WriteLine("Could not locate the version node to update in the nuspec file. XML could be malformed or you are using a namespace for the spec.");
					return (int)ExitCode.UnknownError;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
				return (int)ExitCode.UnknownError;
			}

			return (int)ExitCode.Success;
		}

		static bool UpdateSpecFile(string specFile, string fileVersion)
		{
			var spec = new XmlDocument();
			spec.Load(specFile);

			var verNode = spec.SelectSingleNode("/package/metadata/version");
			if (verNode != null)
			{
				verNode.InnerText = fileVersion;
				spec.Save(specFile);
				return true;
			}
			return false;
		}

		static string GetFileVersion(string file)
		{
			var assembly = Assembly.ReflectionOnlyLoadFrom(file);
			var version = assembly.GetName().Version;

			return version.ToString();
		}
	}
}
