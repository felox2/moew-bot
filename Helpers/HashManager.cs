using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoewBot
{
	public class HashManager
	{
		private readonly string _directory;
		private readonly BinaryFormatter _binaryFormatter;
		private Dictionary<string, string> _hashes;

		public string Filename => Path.Combine(_directory, "Hashes");

		public HashManager(string directory)
		{
			_directory       = directory;
			_binaryFormatter = new BinaryFormatter();

			if (File.Exists(Filename))
			{
				Load();
			}
			else
			{
				_hashes = new Dictionary<string, string>();
				Save();
			}
		}

		public string FindHash(string file)
		{
			return _hashes.ContainsKey(file) ? _hashes[file] : null;
		}

		public void SetHash(string file, string hash)
		{
			_hashes[file] = hash;
		}

		public void Save()
		{
			var dir = Path.GetDirectoryName(Filename);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (Stream stream = File.Open(Filename, FileMode.OpenOrCreate))
			{
				_binaryFormatter.Serialize(stream, _hashes);
			}
		}

		public void Load()
		{
			using (Stream stream = File.Open(Filename, FileMode.Open))
			{
				_hashes = (Dictionary<string, string>) _binaryFormatter.Deserialize(stream);
			}
		}
	}
}
