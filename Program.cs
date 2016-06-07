﻿using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Security.Principal;

// logs to gather: Application, Security, Setup, System

namespace Coloma
{
    class Program
    {
        static void Main(string[] args)
        {
            EventLog[] eventLogs;
            eventLogs = EventLog.GetEventLogs();
            foreach (EventLog evt in eventLogs)
            {
                Console.WriteLine("evt.Log.ToString(): " + evt.Log.ToString() + "\tevt.LogDisplayName: " + evt.LogDisplayName);
            }

            // create the file
            string filename = @"\\iefs\users\mattgr\Coloma\" + "Coloma" + "_" + System.Environment.MachineName + "_" + System.Environment.UserName + "_" + System.Environment.TickCount.ToString() + ".csv";
            StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.UTF8);

            // just get logs for 3/1/2016 and after
            DateTime dt = new DateTime(2016, 3, 1, 0, 0, 0, 0, DateTimeKind.Local);

            Console.WriteLine();
            Console.WriteLine("Coloma is gathering your system, security, setup, and application log entries");
            Console.WriteLine("after" + dt.ToShortDateString());
            Console.WriteLine("And saving them to " + filename);

            // one log at a time
            EventLog log = new EventLog("System", ".");
            WriteLogToStream(log, sw, dt);
            log.Close();

            log = new EventLog("HardwareEvents", ".");
            WriteLogToStream(log, sw, dt);
            log.Close();

            log = new EventLog("Application", ".");
            WriteLogToStream(log, sw, dt);
            log.Close();

            log = new EventLog("Security", ".");
            WriteLogToStream(log, sw, dt);
            log.Close();

            WriteSetupLogToStream(sw, dt);

            sw.Close();
            Console.WriteLine("Done, thank you. Hit any key to exit");
            Console.ReadLine();
        }

        static void WriteLogToStream(EventLog log, StreamWriter sw, DateTime dt)
        {
            string build = WindowsVersion.GetWindowsBuildandRevision();
            
            foreach (EventLogEntry entry in log.Entries)
            {
                if (entry.TimeGenerated > dt)
                {
                    if ((entry.EntryType == EventLogEntryType.Error) ||
                        (entry.EntryType == EventLogEntryType.Warning))
                    {
                        string msg = entry.Message.Replace("\t", " ");
                        msg = msg.Replace("\r\n", "<br>");
                        msg = msg.Replace("\n", "<br>");
                        msg = msg.Replace("<br><br>", "<br>");
                        sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", build, System.Environment.MachineName, System.Environment.UserName, log.LogDisplayName, entry.EntryType.ToString(), entry.TimeGenerated.ToString(), entry.Source, msg);
                    }
                }
            }


        }

        static void WriteSetupLogToStream(StreamWriter sw, DateTime dt)
        {
            string build = WindowsVersion.GetWindowsBuildandRevision();

            EventLogQuery query = new EventLogQuery("Setup", PathType.LogName);
            query.ReverseDirection = false; // this tells it to start with newest first
            EventLogReader reader = new EventLogReader(query);

            EventRecord entry;
            while ((entry = reader.ReadEvent()) != null)
            {
                if (entry.TimeCreated > dt)
                {
                    if ((entry.Level == (byte)StandardEventLevel.Critical) ||
                        (entry.Level == (byte)StandardEventLevel.Error) ||
                        (entry.Level == (byte)StandardEventLevel.Warning))
                    {
                        string msg = entry.FormatDescription();
                        msg = msg.Replace("\t", " ");
                        msg = msg.Replace("\r\n", "<br>");
                        msg = msg.Replace("\n", "<br>");
                        msg = msg.Replace("<br><br>", "<br>");
                        sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", build, System.Environment.MachineName, System.Environment.UserName, "Setup", entry.Level.ToString(), entry.TimeCreated.ToString(), entry.ProviderName, msg);

                    }
                }
                // each eventRecord is an item from the event log
            }
        }
    }
}