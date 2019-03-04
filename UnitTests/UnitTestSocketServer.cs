using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class UnitTestSocketServer
	{
		[TestMethod]
		public void TestInit()
		{
			BizHawk.Client.EmuHawk.Communication.SocketServer socketServer = new BizHawk.Client.EmuHawk.Communication.SocketServer();
			Assert.IsFalse(socketServer.connected);

			socketServer.SetIp("192.168.0.1", 9876);
			socketServer.Connect();
			
		}
	}
}
