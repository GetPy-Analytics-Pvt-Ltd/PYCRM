﻿using System;
using System.Configuration;
using System.IO;


namespace PYCRM
{
    class fileNameCreator
    {
        public static string csvFileNameCreatetor(string table)
        {
            var NotSentDir = ConfigurationManager.AppSettings["NotSentDir"];
            if (!Directory.Exists(NotSentDir))
                Directory.CreateDirectory(NotSentDir);            
            var _posidstr = "1";
            DateTime currentDate = DateTime.Now;
            string dateText = currentDate.ToString("yyyyMMdd_HHmmss");
            var _finalFilename = dateText + "_KEY1_" + table + "_"  + "_KEY5_" + _posidstr + "_DUMMY" + ".csv";
            var _finalFullFileName = Path.Combine(NotSentDir + "\\" + _finalFilename);
            return _finalFullFileName;
        }
        public static string logFileNameCreatetor(string logFileName)
        {
            var NotSentDir = ConfigurationManager.AppSettings["NotSentDir"];
            if (!Directory.Exists(NotSentDir))
                Directory.CreateDirectory(NotSentDir);

            var _merchantidstr = ConfigurationManager.AppSettings["merchantidStr"];
            var _posidstr = ConfigurationManager.AppSettings["posIdStr"];
            DateTime currentDate = DateTime.Now;
            string dateText = currentDate.ToString("yyyyMMdd_HHmmss");
            var _finalFilename = dateText + "_KEY1_" + "LogFile" + "_" + "_KEY4_" + _merchantidstr + "_KEY5_" + _posidstr + "_DUMMY" + ".txt";
            var _finalFullFileName = Path.Combine(NotSentDir + "\\" + _finalFilename);
            return _finalFullFileName;
        }
    }
}
