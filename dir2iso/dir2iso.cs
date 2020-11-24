using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IStream = System.Runtime.InteropServices.ComTypes.IStream;

class IsoImage
{
	[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
	static extern int SHCreateStreamOnFile(string pszFile, uint grfMode, out IStream ppstm);

	static int Main(string[] args)
	{
		if (args.Length < 3 || args.Length > 4)
		{
			Console.WriteLine("dir2iso <MyOutIsoFile.iso> <MyVolumeName> <SourceDirectory> (optional)<FileSystemsToCreate>");
			Console.WriteLine("\n<FileSystemsToCreate> bitfield: Default is: 3 (Joliet | ISO9660) if left blank\n1: ISO9660\n2: Joliet\n4: UDF");
			return -1;
		}

		var isoFilePath = args[0];
		var volumeName = args[1];
		var path = args[2];

		int fileSystemOption = 3/*FsiFileSystemJoliet | FsiFileSystemISO9660*/;

		if (args.Length > 3)
        {
			try
			{
				fileSystemOption = Convert.ToInt32(args[3]);

				// Somehow there is no Math.Clamp() in C# !!! Math.Clamp(fileSystemOption, 1, 7)
				fileSystemOption = Math.Max(Math.Min(fileSystemOption, 7), 1);

				// if FsiFileSystemJoliet is enabled, ISO9660 must be enabled as well, otherwise the IMAPI crashes!
				fileSystemOption |= ((fileSystemOption >> 1) & 1);
			}
			catch (Exception e)
			{
				Console.WriteLine("FileSystemsToCreate should be a number between 1..7");
				Console.WriteLine("EXCEPTION: " + e.Message + e.StackTrace);
			}
		}

		Console.WriteLine("Creating Volume: {0} from path: {1} to {2} with filesystem option: {3}", volumeName, path, isoFilePath, fileSystemOption);
		Stopwatch stopWatch = Stopwatch.StartNew();

		dynamic image = Activator.CreateInstance(Type.GetTypeFromProgID("IMAPI2FS.MsftFileSystemImage"));
		image.ChooseImageDefaultsForMediaType(12/*IMAPI_MEDIA_TYPE_DISK*/);
		image.FileSystemsToCreate = fileSystemOption;
		image.VolumeName = volumeName;
		image.Root.AddTree(path, false);
		var resultImage = image.CreateResultImage();
		IStream inStream = resultImage.ImageStream;

		try
		{
			var hResult = SHCreateStreamOnFile(isoFilePath, 0x00001001, out IStream outStream);

			if (hResult == 0)
			{
				inStream.CopyTo(outStream, (long)resultImage.TotalBlocks * (long)resultImage.BlockSize, IntPtr.Zero, IntPtr.Zero);
				outStream.Commit(0);
			}
			else
			{
				throw new System.InvalidOperationException("Can not create output ISO file: " + isoFilePath);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine("EXCEPTION: " + e.Message + e.StackTrace);
			return -2;
		}

		stopWatch.Stop();
		Console.WriteLine("ISO file created successfully: {0} in {1} seconds.", isoFilePath, (float)stopWatch.ElapsedMilliseconds / 1000.0);
		return 0;
	}
}