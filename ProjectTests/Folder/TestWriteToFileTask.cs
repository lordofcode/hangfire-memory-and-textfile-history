using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTests.Folder
{
    internal class TestWriteToFileTask
    {
        public static void WriteToOutput()
        {
            var saveLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "App_Data");
            //System.Diagnostics.Debug.WriteLine($"Called TestWriteToFileTask.WriteToOutput on {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            File.AppendAllText(@$"{saveLocation}\running-task.txt", $"Uitgevoerd op {DateTime.Now.ToString("HH:mm:ss")}\r\n");
        }
    }
}
