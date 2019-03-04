using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UnitTests
{
	[TestClass]
	public class UnitTestArgParser
	{
		[TestInitialize]
		public void Initialize()
		{
			//otherwise the unittests might collide with each other
			BizHawk.Client.EmuHawk.GlobalWin.socketServer = null;
			BizHawk.Client.EmuHawk.GlobalWin.httpCommunication = null;
			BizHawk.Client.EmuHawk.GlobalWin.memoryMappedFiles = null;
		}

		[TestMethod]
		public void ArgParserParseSimple()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--socket_ip=192.168.0.1", "--socket_port=9999"};

			Assert.AreEqual(null, argParser.socket_ip);
			Assert.AreEqual(0, argParser.socket_port);

			argParser.ParseArguments(args);

			Assert.AreEqual("192.168.0.1", argParser.socket_ip);
			Assert.AreEqual(9999, argParser.socket_port);
		}

		[TestMethod]
		public void ArgParserParseUpperCase()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--SOCKET_IP=192.168.0.1", "--SOCKET_PORT=9999" };

			Assert.AreEqual(null, argParser.socket_ip);
			Assert.AreEqual(0, argParser.socket_port);

			argParser.ParseArguments(args);

			Assert.AreEqual("192.168.0.1", argParser.socket_ip);
			Assert.AreEqual(9999, argParser.socket_port);
		}

		[TestMethod]
		public void ArgParserParseCaseMix()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--MOVIE=lower_case_movie" };

			Assert.AreEqual(null, argParser.cmdMovie);

			argParser.ParseArguments(args);

			Assert.AreEqual("lower_case_movie", argParser.cmdMovie);
		}

		[TestMethod]
		[ExpectedException(typeof(BizHawk.Client.EmuHawk.ArgParserException))]
		public void ArgParserParseThrows()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--SOCKET_IP=192.168.0.1" };

			argParser.ParseArguments(args);
		}

		[TestMethod]
		public void ArgParserGetCmdConfigFile()
		{
			string[] args = new string[] { "--config=foo", "--SOCKET_IP=192.168.0.1", "--SOCKET_PORT=9999" };

			BizHawk.Client.EmuHawk.ArgParser.GetCmdConfigFile(args);
			Assert.AreEqual("foo", BizHawk.Client.EmuHawk.ArgParser.GetCmdConfigFile(args));
		}

		[TestMethod]
		public void ArgParserGetCmdConfigFileNoException()
		{
			string[] args = new string[] { "--config=bar", "--SOCKET_PORT=9999" };

			BizHawk.Client.EmuHawk.ArgParser.GetCmdConfigFile(args);
			Assert.AreEqual("bar", BizHawk.Client.EmuHawk.ArgParser.GetCmdConfigFile(args));
		}

		[TestMethod]
		public void ArgParserParseDuplicateEntries()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--SOCKET_IP=192.168.0.1", "--SOCKET_PORT=1111", "--SOCKET_PORT=9999" };

			argParser.ParseArguments(args);
			Assert.AreEqual(9999, argParser.socket_port);
		}

		[TestMethod]
		public void ArgParserParseSocketServer()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--SOCKET_IP=192.168.0.1", "--SOCKET_PORT=9999" };

			Assert.AreEqual(null, BizHawk.Client.EmuHawk.GlobalWin.socketServer);

			argParser.ParseArguments(args);
			Assert.AreNotEqual(null, BizHawk.Client.EmuHawk.GlobalWin.socketServer);
		}

		[TestMethod]
		public void ArgParserParseMmf()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--mmf=foo" };

			Assert.AreEqual(null, BizHawk.Client.EmuHawk.GlobalWin.memoryMappedFiles);

			argParser.ParseArguments(args);
			Assert.AreNotEqual(null, BizHawk.Client.EmuHawk.GlobalWin.memoryMappedFiles);
			Assert.AreEqual("foo", BizHawk.Client.EmuHawk.GlobalWin.memoryMappedFiles.filename_main);
		}

		[TestMethod]
		public void ArgParserParseHttpGet()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--url_get=foo" };

			Assert.AreEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);

			argParser.ParseArguments(args);
			Assert.AreNotEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);
			Assert.AreEqual("foo", BizHawk.Client.EmuHawk.GlobalWin.httpCommunication.GetGetUrl());
		}

		[TestMethod]
		public void ArgParserParseHttpPost()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--url_post=foo" };

			Assert.AreEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);

			argParser.ParseArguments(args);
			Assert.AreNotEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);
			Assert.AreEqual("foo", BizHawk.Client.EmuHawk.GlobalWin.httpCommunication.GetPostUrl());
		}

		[TestMethod]
		public void ArgParserParseHttpGetPost()
		{
			BizHawk.Client.EmuHawk.ArgParser argParser = new BizHawk.Client.EmuHawk.ArgParser();
			string[] args = new string[] { "--url_get=foo", "--url_post=bar" };

			Assert.AreEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);

			argParser.ParseArguments(args);
			Assert.AreNotEqual(null, BizHawk.Client.EmuHawk.GlobalWin.httpCommunication);
			Assert.AreEqual("foo", BizHawk.Client.EmuHawk.GlobalWin.httpCommunication.GetGetUrl());
			Assert.AreEqual("bar", BizHawk.Client.EmuHawk.GlobalWin.httpCommunication.GetPostUrl());
		}




	}
}
