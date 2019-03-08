using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

class Program
{
	private const int MAX_BUFFER_SIZE = 67108864;

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random _random;

        /// <summary>
        /// Represents a thread safe pseudo-random number generator, a device that produces a sequence
        /// of numbers that meet certain statistical requirements for randomness.
        /// </summary>
        public static Random @Random
        {
            get { return _random ?? (_random = new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId)); }
        }

        /// <summary>
        /// Randomize list element order using Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to shuffle.</param>
        public static void Shuffle<T>(IList<T> list)
        {
            for (int pass = list.Count - 1; pass > 1; pass--)
            {
                int index = ThreadSafeRandom.Random.Next(pass + 1);
                T value = list[index];
                list[index] = list[pass];
                list[pass] = value;
            }
        }
    }

		#region OverwriteFile_Gutmann

        /// <summary>
        /// Overwrite the file based on the Peter Gutmann's algorithm.
        /// </summary>
        /// <param name="file">The file.</param>
        internal static void OverwriteFile_Gutmann(FileInfo file)
        {
            byte[][] pattern = new byte[][] { 
                new byte[] {0x55, 0x55, 0x55}, new byte[] {0xAA, 0xAA, 0xAA}, new byte[] {0x92, 0x49, 0x24}, new byte[] {0x49, 0x24, 0x92}, new byte[] {0x24, 0x92, 0x49}, 
                new byte[] {0x00, 0x00, 0x00}, new byte[] {0x11, 0x11, 0x11}, new byte[] {0x22, 0x22, 0x22}, new byte[] {0x33, 0x33, 0x33}, new byte[] {0x44, 0x44, 0x44}, 
                new byte[] {0x55, 0x55, 0x55}, new byte[] {0x66, 0x66, 0x66}, new byte[] {0x77, 0x77, 0x77}, new byte[] {0x88, 0x88, 0x88}, new byte[] {0x99, 0x99, 0x99}, 
                new byte[] {0xAA, 0xAA, 0xAA}, new byte[] {0xBB, 0xBB, 0xBB}, new byte[] {0xCC, 0xCC, 0xCC}, new byte[] {0xDD, 0xDD, 0xDD}, new byte[] {0xEE, 0xEE, 0xEE}, 
                new byte[] {0xFF, 0xFF, 0xFF}, new byte[] {0x92, 0x49, 0x24}, new byte[] {0x49, 0x24, 0x92}, new byte[] {0x24, 0x92, 0x49}, new byte[] {0x6D, 0xB6, 0xDB}, 
                new byte[] {0xB6, 0xDB, 0x6D}, new byte[] {0xDB, 0x6D, 0xB6} };

            ThreadSafeRandom.Shuffle<byte[]>(pattern);

            Random random = ThreadSafeRandom.Random;

            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None);

			Console.WriteLine("Overwrite this file: ");
            for (int pass = 1; pass <= 35; ++pass)
            {
				Console.Write( ( (pass<10)?"0":"") + pass + ( (pass%7==0)?" \n":" " ) );
                for (int index = 0; index < 3; index++)
                {
                    fs.Position = 0;

                    for (long size = fs.Length; size > 0; size -= MAX_BUFFER_SIZE)
                    {
                        long bufferSize = (size < MAX_BUFFER_SIZE) ? size : MAX_BUFFER_SIZE;

                        byte[] buffer = new byte[bufferSize];

                        if (pass > 4 && pass < 32)
                        {
                            for (int bufferIndex = 0; bufferIndex < bufferSize; ++bufferIndex)
                            {
                                buffer[bufferIndex] = pattern[pass - 5][index];
                            }
                        }
                        else
                        {
                            for (int bufferIndex = 0; bufferIndex < bufferSize; ++bufferIndex)
                            {
                                buffer[bufferIndex] = (byte)(random.Next() % 256);
                            }
                        }

                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush(true);
                    }
                }
            }
			Console.WriteLine();
            fs.Close(); fs.Dispose(); fs = null;
        }

        #endregion
		
		private static bool PromptConfirmation(string confirmText)
		{
			Console.Write(confirmText + " [y/n] : ");
			ConsoleKey response = Console.ReadKey(false).Key;
			Console.WriteLine();
			return (response == ConsoleKey.Y);
		}

		public static void Main(string [] args){
			Console.WriteLine("Command line arguments: >Gutmann.exe [\"pathway\\filename.extension\"]");
			string path = "";
			if(args.Length!=0){
				path = args[0];

				FileInfo fileInf = new FileInfo(path);
				if (fileInf.Exists)
				{
					Console.WriteLine("Filename: {0}", fileInf.Name);
					Console.WriteLine("Creation date: {0}", fileInf.CreationTime);
					Console.WriteLine("filesize: {0}", fileInf.Length);
					Console.WriteLine("fullPath: {0}\n", fileInf.FullName);
				
				
					string confirm_text = "After overwrite, the file: \n\""+path+"\"\nshould be deleted without recovery!\nOverwrite this?";
					if( PromptConfirmation(confirm_text) )
					{
						try{
							OverwriteFile_Gutmann(fileInf);		//Try to overwrite the file, using "The Gutmann Method".
							Console.WriteLine("File "+path+" successfully overwritted, using \"The Gutmann method\"!");
						}catch(Exception e){
							Console.WriteLine("Cann't overwrite the file: "+e);
						}
					}
				
					confirm_text = "Delete this file?";
					if(PromptConfirmation(confirm_text)){	//by default
						try{
							File.Delete(fileInf.FullName);	//Try remove the file
							Console.WriteLine("File successfully deleted.");
						}
						catch(Exception e){
							Console.WriteLine("Cann't delete file: "+e);
						}
					}
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
}