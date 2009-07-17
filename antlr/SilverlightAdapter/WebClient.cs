using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace System.Net
{
    static public class WebClientStub
    {
        static public string DownloadString(this WebClient client, string address)
        {
            return DownloadString(client, new Uri(address));
        }

        static public string DownloadString(this WebClient client, Uri uri)
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            string str = null;

            DownloadStringCompletedEventHandler dsc_eh = null;
            dsc_eh = new DownloadStringCompletedEventHandler(
                 delegate(object sender, DownloadStringCompletedEventArgs e)
                 {
                     client.DownloadStringCompleted -= dsc_eh;
                     str = e.Result;
                     evt.Set();
                 }
             );

            client.DownloadStringCompleted += dsc_eh;

            client.DownloadStringAsync(uri);
            evt.WaitOne();

            return str;            
        }

        static public byte[] DownloadData(this WebClient client, string address)
        {
            return DownloadData(client, address);
        }

        static public byte[] DownloadData(this WebClient client, Uri uri)
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            byte[] data = null;

            OpenReadCompletedEventHandler orc_eh = null;
            orc_eh = new OpenReadCompletedEventHandler(
                 delegate(object sender, OpenReadCompletedEventArgs e)
                 {
                     client.OpenReadCompleted -= orc_eh;
                     if (e.Error == null && !e.Cancelled && e.Result != null)
                     {
                         data = new byte[e.Result.Length];
                         try
                         {
                             e.Result.Read(data, 0, (int)e.Result.Length);
                         }
                         catch
                         {
                             data = null;
                         }
                         finally
                         {
                             e.Result.Close();
                         }
                     }
                     evt.Set();
                 }
             );

            client.OpenReadCompleted += orc_eh;

            client.OpenReadAsync(uri);
            evt.WaitOne();

            return data;   
        }
    }
}
