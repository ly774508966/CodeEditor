using System;
using System.IO;
using CodeEditor.IO;
using CodeEditor.Logging;
using CodeEditor.Reactive;
using CodeEditor.Testing;
using NUnit.Framework;

namespace CodeEditor.Languages.Common.Tests
{
	[TestFixture]
	public class ObservableServiceClientProviderTest : MockBasedTest
	{
		[Test]
		public void GetsServerAddressFromUriFile()
		{
			const string serverExecutable = "server.exe";
			const string serverUrilFile = "server.uri";
			const string serverAddress = "tcp://localhost:4242/IServiceProvider";

			var projectPathProvider = MockFor<IServerExecutableProvider>();
			projectPathProvider
				.SetupGet(_ => _.ServerExecutable)
				.Returns(serverExecutable);

			// provider tries to delete pid file to decide if it needs
			// to start the server
			var fileSystem = MockFor<IFileSystem>();
			var uriFile = MockFor<IFile>();
			fileSystem
				.Setup(_ => _.FileFor(serverUrilFile))
				.Returns(uriFile.Object);

			uriFile
				.Setup(_ => _.Delete())
				.Throws(new IOException());

			uriFile
				.Setup(_ => _.ReadAllText())
				.Returns(serverAddress + "\n");
			
			var subject = new ObservableServiceClientProvider
			{
				ServerExecutableProvider = projectPathProvider.Object,
				FileSystem = fileSystem.Object,
				Logger = new StandardLogger()
			};
			Assert.IsNotNull(subject.Client.FirstOrTimeout(TimeSpan.FromSeconds(1)));

			VerifyAllMocks();
		}
	}
}