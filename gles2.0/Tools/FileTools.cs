using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Skweez.Filetools
{
    class FileTools
    {
        //static public string getContentByFilePath(string filePath)
        //{
        //    try
        //    {
        //        StreamReader streamReader = new StreamReader(filePath);
        //        string text = streamReader.ReadToEnd();
        //        streamReader.Close();
        //        return text;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        static public string getContentByStream(Stream stream)
        {
            try
            {
                StreamReader streamReader = new StreamReader(stream);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                return text;
            }
            catch
            {
                throw;
            }
        }
    }
}