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
		if (args.Length != 3)
		{
			Console.WriteLine("dir2iso <myOutIsoFile.iso> <myVolumeName> <sourceDirectory>");
			return -1;
		}

		var isoFilePath = args[0];
		var volumeName = args[1];
		var path = args[2];

		Console.WriteLine("Creating Volume: {0} from path: {1} to {2}", volumeName, path, isoFilePath);
		Stopwatch stopWatch = Stopwatch.StartNew();

		dynamic image = Activator.CreateInstance(Type.GetTypeFromProgID("IMAPI2FS.MsftFileSystemImage"));
		image.ChooseImageDefaultsForMediaType(12/*IMAPI_MEDIA_TYPE_DISK*/);
		image.FileSystemsToCreate = 3/*FsiFileSystemJoliet | FsiFileSystemISO9660*/;
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