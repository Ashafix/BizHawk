using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Windows.Forms;
using BizHawk.Bizware.BizwareGL;
using System.Drawing;
using BizHawk.Emulation.Common;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common.IEmulatorExtensions;
using System.Net.Sockets;

namespace BizHawk.Client.EmuHawk
{

	public class Communication
	{
		
		public class HttpCommunication
		{
			private static HttpClient client = new HttpClient();
			private string PostUrl = "http://localhost:9876/post/";
			private string GetUrl = "http://localhost:9876/index";
			public bool initialized = false;
			private string errorMessage = "";
			private ScreenShot screenShot = new ScreenShot();
			
			public string TestGet()
			{
				string resp = Get(GetUrl).ToString();
				return resp;
			}

			public void SetTimeout(TimeSpan timeout)
			{
				client.Timeout = timeout;
			}

			public static async Task<string> Get(string url)
			{
				var response = await client.GetAsync(url);
				string result = await response.Content.ReadAsStringAsync();
				return result;
			}
			public void SetUrls(string url)
			{
				PostUrl = url;
				GetUrl = url;
			}
			public void SetPostUrl(string url)
			{
				PostUrl = url;
			}
			public void SetGetUrl(string url)
			{
				GetUrl = url;
			}

			public string GetGetUrl()
			{
				return GetUrl;
			}

			public string GetPostUrl()
			{
				return PostUrl;
			}

			public string Post(string url, FormUrlEncodedContent content)
			{
				var r = PostAsync(url, content);
				r.Wait();
				return r.Result;
			}

			public async Task<String> PostAsync(string url, FormUrlEncodedContent content)
			{
				HttpResponseMessage response = null;
				try
				{
					response = await client.PostAsync(url, content);
				}
				catch (Exception)
				{
					return errorMessage;
				}
				if (response.StatusCode != HttpStatusCode.OK)
				{
					return errorMessage;
				}

				string resp_string = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				
				return resp_string;
			}

			public string SendScreenshot()
			{
				var values = new Dictionary<string, string>
				{
					{ "screenshot", screenShot.GetScreenShotAsString()},
				};
				FormUrlEncodedContent content = new FormUrlEncodedContent(values);
				return Post(PostUrl, content);
			}
			

			public string SendScreenshot(string url)
			{
				var values = new Dictionary<string, string>
				{
					{ "screenshot", screenShot.GetScreenShotAsString()},
				};
			
				var content = new FormUrlEncodedContent(values);
				return Post(url, content).ToString();
			}

			public string SendScreenshot(string url, string parameter)
			{
				var values = new Dictionary<string, string>
				{
					{ parameter, screenShot.GetScreenShotAsString() }
				};
				var content = new FormUrlEncodedContent(values);
				return Post(url, content).ToString();
			}

			public void SetErrorMessage(string message)
			{
				errorMessage = message;
			}

		}
		public class SocketServer
		{
			public string ip = "192.168.178.21";
			public int port = 9999;
			public Decoder decoder = Encoding.UTF8.GetDecoder();
			public Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			public IPAddress ipAdd;
			public IPEndPoint remoteEP;
			public IVideoProvider currentVideoProvider = null;
			public bool connected = false;
			public bool initialized = false;
			public int retries = 10; //number of retries until giving up
			public bool success = false; //indicates whether the last command was executed succesfully
			public int timeOut = 1000; //timeout in milliseconds

			public void Initialize(IVideoProvider _currentVideoProvider)
			{
				currentVideoProvider = _currentVideoProvider;
				SetIp(ip, port);
				initialized = true;
			}
			public Boolean Connect()
			{
				if (!initialized)
				{
					Initialize(currentVideoProvider);
				}
				remoteEP = new IPEndPoint(ipAdd, port);
				soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IAsyncResult result = soc.BeginConnect(ip, port, null, null);
				bool success = result.AsyncWaitHandle.WaitOne(timeOut, true);
				if (success && soc.Connected)
				{
					connected = true;
					return true;
				} else
				{
					connected = false;
					return false;
				}
				
			}
			public void SetIp(string ip_)
			{
				ip = ip_;
				ipAdd = System.Net.IPAddress.Parse(ip);
				remoteEP = new IPEndPoint(ipAdd, port);
			}
			public void SetIp(string ip_, int port_)
			{
				ip = ip_;
				port = port_;
				ipAdd = System.Net.IPAddress.Parse(ip);
				remoteEP = new IPEndPoint(ipAdd, port);
			}
			public void SocketConnected()
			{
				bool part1 = soc.Poll(1000, SelectMode.SelectRead);
				bool part2 = (soc.Available == 0);
				if (part1 && part2)
					connected = false;
				else
					connected = true;
			}
			public Boolean SendScreenshot()
			{
				if (!connected)
				{
					Connect();
				}
				ScreenShot screenShot = new ScreenShot();
				using (var bb = screenShot.MakeScreenShotImage())
				{
					using (var img = bb.ToSysdrawingBitmap())
					{
						byte[] bmpBytes = screenShot.ImageToByte(img);
						int sentBytes = 0;
						int tries = 0;
						while (sentBytes == 0 && tries < retries)
						{
							try
							{
								tries++;
								sentBytes = soc.Send(bmpBytes);
							}
							catch (SocketException)
							{
								Connect();
								sentBytes = 0;
							}
						}
						success = (tries < retries);
					}
				}
				return success;
			}
			public string ReceiveMessage()
			{
				Connect();
				byte[] receivedBytes = new byte[1024];
				int receivedLength = soc.Receive(receivedBytes);
				char[] chars = new char[receivedLength];
				int charLen = decoder.GetChars(receivedBytes, 0, receivedLength, chars, 0);
				return chars.ToString();
			}
			public bool Successful()
			{
				return success;
			}

			public void SetTimeOut(int timeout)
			{
				timeOut = timeout;
			}
		}

		public class MemoryMappedFiles
		{
			public string filename_main = "BizhawkTemp_main";
			public string filename_index = "BizhawkTemp_index";
			public string filename_response = "BizhawkTemp_response";
			public MemoryMappedFile mmf_main = null;
			public MemoryMappedFile mmf_index = null;
			public MemoryMappedFile mmf_response = null;
			public int index = 0;
			private byte[] bytesWaiting = Encoding.ASCII.GetBytes("waiting");
			public bool initialized = false;
			public int main_size = 10^5;
			ScreenShot screenShot = new ScreenShot();

			public void SetFilenames(string _filename_main)
			{
				filename_main = _filename_main;
			}
			public void SetFilenames(string _filename_main, string _filename_index)
			{
				filename_main = _filename_main;
				filename_index = _filename_index;
			}
			public void SetFilenames(string _filename_main, string _filename_index, string _filename_response)
			{
				filename_main = _filename_main;
				filename_index = _filename_index;
				filename_response = _filename_response; ;
			}
			public void ScreenShotToFile()
			{
				ScreenShot screenShot = new ScreenShot();
				var bb = screenShot.MakeScreenShotImage();
				var img = bb.ToSysdrawingBitmap();
				byte[] bmpBytes = screenShot.ImageToByte(img);
				if (mmf_main == null)
				{
					mmf_main = MemoryMappedFile.CreateOrOpen(@filename_main, bmpBytes.Length);
				}
				try
				{
					using (MemoryMappedViewAccessor accessor = mmf_main.CreateViewAccessor(0, bmpBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, bmpBytes, 0, bmpBytes.Length);
					}
				}
				catch (UnauthorizedAccessException)
				{
					try
					{
						mmf_main.Dispose();
					}
					catch (Exception)
					{

					}
					mmf_main = MemoryMappedFile.CreateOrOpen(@filename_main, bmpBytes.Length);
					using (MemoryMappedViewAccessor accessor = mmf_main.CreateViewAccessor(0, bmpBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, bmpBytes, 0, bmpBytes.Length);
					}
				}
			}

			public void ScreenShotToFile(int _index)
			{
				var bb = screenShot.MakeScreenShotImage();
				var img = bb.ToSysdrawingBitmap();
				byte[] bmpBytes = screenShot.ImageToByte(img);
				int[] indexInts = new int[2] { bmpBytes.Length, _index };
				byte[] indexBytes = new byte[indexInts.Length * sizeof(int)];
				Buffer.BlockCopy(indexInts, 0, indexBytes, 0, indexBytes.Length);
				if (mmf_main == null)
				{
					mmf_main = MemoryMappedFile.CreateOrOpen(@filename_main, bmpBytes.Length);
					mmf_index = MemoryMappedFile.CreateOrOpen(@filename_index, indexBytes.Length * 32);
				}
				string resp = ReadResponse();
				int trials = 0;
				while (resp == "waiting" && trials < 100)
				{
					resp = ReadResponse();
					Thread.Sleep(50);
					trials += 1;
				}
				if (trials >= 100)
				{
					MessageBox.Show("timed out");
				}
				try
				{
					using (MemoryMappedViewAccessor accessor = mmf_main.CreateViewAccessor(0, bmpBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, bmpBytes, 0, bmpBytes.Length);
					}
				}
				catch (UnauthorizedAccessException)
				{
					try
					{
						mmf_main.Dispose();
					}
					catch (Exception)
					{

					}
					mmf_main = MemoryMappedFile.CreateOrOpen(@filename_main, bmpBytes.Length);
					using (MemoryMappedViewAccessor accessor = mmf_main.CreateViewAccessor(0, bmpBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, bmpBytes, 0, bmpBytes.Length);
					}
				}

				try
				{
					using (MemoryMappedViewAccessor accessor = mmf_index.CreateViewAccessor(0, indexBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, indexBytes, 0, indexBytes.Length);
					}
				}
				catch (UnauthorizedAccessException)
				{
					try
					{
						mmf_index.Dispose();
					}
					catch (Exception)
					{
					}

					mmf_index = MemoryMappedFile.CreateOrOpen(@filename_index, indexBytes.Length * 32);
					using (MemoryMappedViewAccessor accessor = mmf_index.CreateViewAccessor(0, indexBytes.Length, MemoryMappedFileAccess.Write))
					{
						accessor.WriteArray<byte>(0, indexBytes, 0, indexBytes.Length);
					}
				}
				WriteResponse();
				index += _index + 1;
			}

			public void SetIndex(int _index)
			{
				index = _index;
			}
			
			public void WriteResponse()
			{

				if (mmf_response == null)
				{
					mmf_response = MemoryMappedFile.CreateOrOpen(filename_response, 32);
				}
				using (MemoryMappedViewAccessor accessor = mmf_response.CreateViewAccessor(0, 32, MemoryMappedFileAccess.Write))
				{
					accessor.WriteArray<byte>(0, bytesWaiting, 0, bytesWaiting.Length);
				}
			}

			public string ReadResponse()
			{

				if (mmf_response == null)
				{
					try
					{
						mmf_response = MemoryMappedFile.OpenExisting(filename_response);
					}
					catch (FileNotFoundException)
					{
						return "";
					}
				}
				using (MemoryMappedViewAccessor accessor = mmf_response.CreateViewAccessor(0, 32, MemoryMappedFileAccess.Read))
				{
					byte[] outStructure = new byte[32];
					accessor.ReadArray<byte>(0, outStructure, 0, 32);
					return Encoding.UTF8.GetString(outStructure).Trim('\0');
				}
			}

			public int GetIndex()
			{
				return index;
			}
		}
		class ScreenShot
		//makes all functionalities for providing screenshots available
		{
			private IVideoProvider currentVideoProvider = null;
			private ImageConverter converter = new ImageConverter();
			public BitmapBuffer MakeScreenShotImage()
			{
				if (currentVideoProvider == null)
				{
					currentVideoProvider = Global.Emulator.AsVideoProviderOrDefault();
				}
				return GlobalWin.DisplayManager.RenderVideoProvider(currentVideoProvider);
			}
			public byte[] ImageToByte(Image img)
			{
				
				return (byte[])converter.ConvertTo(img, typeof(byte[]));
			}
			public string ImageToString(Image img)
			{
				return Convert.ToBase64String(ImageToByte(img));	
			}
			public string GetScreenShotAsString()
			{
				BitmapBuffer bb = MakeScreenShotImage();
				byte[] imgBytes = ImageToByte(bb.ToSysdrawingBitmap());
				return Convert.ToBase64String(imgBytes);

			}
		}
	}
}
