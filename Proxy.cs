using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using Fiddler;

namespace MoewBot
{
	class Proxy
	{
		public const string SCRIPTS_DIRECTORY = "Scripts";
		public bool ShouldStop = false;

		private readonly ushort _port;
		private readonly HashManager _hashManager;

		public Proxy(ushort port)
		{
			_port        = port;
			_hashManager = new HashManager(SCRIPTS_DIRECTORY);
		}

		public void Start()
		{
			FiddlerCoreStartupSettings fiddlerCoreStartupSettings = new FiddlerCoreStartupSettingsBuilder().ListenOnPort(_port).DecryptSSL().OptimizeThreadPool().RegisterAsSystemProxy().AllowRemoteClients().Build();
			FiddlerApplication.Startup(fiddlerCoreStartupSettings);

			Logger.Info("Loading available scripts");

			if (!Directory.Exists(SCRIPTS_DIRECTORY))
			{
				Directory.CreateDirectory(SCRIPTS_DIRECTORY);
			}
			else
			{
				CompileScripts();
				LoadScripts();
			}

			Logger.Info($"Created endpoint listening on port &s{fiddlerCoreStartupSettings.ListenPort}");
		}

		public void LoadScripts()
		{
			var filesToLoad = Util.FindDlls(SCRIPTS_DIRECTORY);
			if (filesToLoad.Count <= 0)
			{
				Logger.Debug("No scripts to load");
				return;
			}

			foreach (var file in filesToLoad)
			{
				var dll = Assembly.LoadFile(Path.GetFullPath(file));

				foreach (var exportedType in dll.GetExportedTypes())
				{
					dynamic c = Activator.CreateInstance(exportedType);
					c.Init();

					Logger.Debug($"Loaded {c}");
				}
			}

			Logger.Info($"Loaded {filesToLoad.Count} scripts");
		}

		public void CompileScripts()
		{
			var filesToCompile = Util.FindScripts(SCRIPTS_DIRECTORY);

			// compile .cs files to .dll
			if (filesToCompile.Count <= 0)
			{
				Logger.Debug("No scripts to compile");
				return;
			}

			var provider = CodeDomProvider.CreateProvider("CSharp");

			_hashManager.Load();
			foreach (var file in filesToCompile)
			{
				var hash     = Util.CalculateMd5(file);
				var fileHash = _hashManager.FindHash(file);

				if (fileHash != null && fileHash == hash)
				{
					Logger.Trace($"File already compiled &s{file}");
					continue;
				}

				var dll = file.Replace(".cs", ".dll");

				if (File.Exists(dll))
					File.Delete(dll);

				var currentAssembly = typeof(Program).Assembly.GetName().Name + ".exe";
				var parameters = new CompilerParameters
				{
					ReferencedAssemblies = {"System.dll", currentAssembly},
					GenerateExecutable   = false,
					OutputAssembly       = dll
				};
				var results = provider.CompileAssemblyFromFile(parameters, file);

				if (results.Errors.Count > 0)
				{
					foreach (CompilerError compilerError in results.Errors)
					{
						Logger.Error(compilerError);
					}
				}
				else
				{
					_hashManager.SetHash(file, hash);

					Logger.Debug($"Compiled {file}");
				}
			}

			_hashManager.Save();
		}

		public void Stop()
		{
			FiddlerApplication.Shutdown();
		}
	}
}