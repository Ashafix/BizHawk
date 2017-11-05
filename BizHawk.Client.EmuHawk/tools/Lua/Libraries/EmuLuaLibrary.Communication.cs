using System;
using System.ComponentModel;
using NLua;
using BizHawk.Emulation.Common;
using BizHawk.Client.Common;
using System.Windows.Forms;
using System.Text;

namespace BizHawk.Client.EmuHawk
{
	[Description("A library for communicating with other programs")]
	public sealed class CommunicationLuaLibrary : LuaLibraryBase
	{
		//[RequiredService]
		//private IEmulator Emulator { get; set; }

		//[RequiredService]
		//private IVideoProvider VideoProvider { get; set; }

		public CommunicationLuaLibrary(Lua lua)
			: base(lua) { }

		public CommunicationLuaLibrary(Lua lua, Action<string> logOutputCallback)
			: base(lua, logOutputCallback) { }

		public override string Name => "comm";

		//TO DO: not working yet!
		[LuaMethod("getluafunctionslist", "returns a list of implemented functions")]
		public static string GetLuaFunctionsList()
		{
			var list = new StringBuilder();
			foreach (var function in GlobalWin.Tools.LuaConsole.LuaImp.Docs)
			{
				list.AppendLine(function.Name);
			}

			return list.ToString();
		}

		[LuaMethod("SocketServerScreenShot", "send a screen shot to the Socket server")]
		public bool SocketServerScreenShot()
		{
			return GlobalWin.socketServer.SendScreenshot();
		}

		[LuaMethod("SocketServerResponse", "receives a message from the Socket server")]
		public string SocketServerResponse()
		{
			return GlobalWin.socketServer.ReceiveMessage();
		}

		[LuaMethod("SocketServerSuccessful", "returns the status of the last Socket server action")]
		public bool SocketServerSuccessful()
		{
			return GlobalWin.socketServer.Successful();
		}

		[LuaMethod("mmf_screenshot", "returns the status of the last Socket server action")]
		public string Mmf_screenshot()
		{
			GlobalWin.memoryMappedFiles.ScreenShotToFile();
			return "screenshot saved to memory mapped file";
		}
		[LuaMethod("httptest", "HTTP get")]
		public string Httptest()
		{
			GlobalWin.httpCommunication.TestGet();
			GlobalWin.httpCommunication.SendScreenshot();
			return "done testing";

		}
		[LuaMethod("httppostscreenshot", "HTTP POST screenshot")]
		public string Httppostscreenshot()
		{
			return GlobalWin.httpCommunication.SendScreenshot();
		}
		[LuaMethod("sethttptimeout", "Sets HTTP timeout in seconds")]
		public void Sethttptimeout(double timeout)
		{
			GlobalWin.httpCommunication.SetTimeout(System.TimeSpan.FromSeconds(timeout));
		}

	}

}
