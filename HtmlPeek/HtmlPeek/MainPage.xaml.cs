using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.IO;
using Microsoft.Phone.Shell;
using System.Device.Location;
using HtmlAgilityPack;
using System.Collections.Generic;
using RestSharp;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
//using Google.AdMob.Ads.WindowsPhone7;


namespace HtmlPeek
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(Current_Deactivated);
            PhoneApplicationService.Current.Closing += new EventHandler<ClosingEventArgs>(Current_Closing);

            webBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(webBrowser_Navigated);
            webBrowser.ScriptNotify += new EventHandler<NotifyEventArgs>(webBrowser_ScriptNotify);

            var textBox = App.IsolatedStorageCacheManager<string>.Retrieve("textBoxUrlValue");

            if (textBox != null) textBoxUrl.Text = textBox;
        }

        private void textBoxUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // TODO: Add event handler implementation here.
                //MessageBox.Show(e.Key.ToString());
                if (e.Key.ToString() == "Enter" && (textBoxUrl.Text != "") && (textBoxUrl.Text != "http://"))
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(textBoxUrl.Text);
                    var result = (IAsyncResult)request.BeginGetResponse(ResponseCallback, request);

                    progressUrl.Visibility = System.Windows.Visibility.Visible;
                    imgCloud.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(ex.Message);
                    progressUrl.Visibility = System.Windows.Visibility.Collapsed;
                });
            }
        }

        private void ResponseCallback(IAsyncResult result)
        {
            try
            {
                var request = (HttpWebRequest)result.AsyncState;
                var response = request.EndGetResponse(result);

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(contents);
                    
                    //textToBeDisplayed = contents;
              
                    foreach(var node in doc.DocumentNode.Descendants("body"))
                    {
                        textToBeDisplayed = node.InnerText;
                    }
                    //textToBeDisplayed = doc.DocumentNode.Descendants("body").ToString();
                    //textToBeDisplayed = HttpUtility.HtmlEncode(textToBeDisplayed).Replace("&amp;nbsp;", " ").Replace("&lt;!--", " ").Replace("--&gt;", " ").Replace("&lt;", " ").Replace("&gt;", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
                    textToBeDisplayed = HttpUtility.HtmlDecode(textToBeDisplayed);
                    if (textToBeDisplayed != "")
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            NavigateWebBrowser();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(ex.Message);
                    progressUrl.Visibility = System.Windows.Visibility.Collapsed;
                });
            }
        }

        public string textToBeDisplayed = "";
        //private Uri pageWithInvokeScript = new Uri("http://chrispogi.com/htmlpeek.htm");

        //private Uri pageWithInvokeScript = new Uri("http://<yourdomain>.com/htmlpeek.htm");

        public void NavigateWebBrowser()
        {
            //webBrowser.Navigate(pageWithInvokeScript);
            webBrowser.NavigateToString(
            @"<html>
              <head><title>HtmlPeek</title>
                <script type='text/javascript'>
                    function DataReceivedFromPhoneApp(input) {
                        textReceived.innerHTML = input;
                        return true;
                    }

                    function SendDataToPhoneApp() {
                        window.external.Notify('Go');
                    }
                </script>
            </head>
            <body bgcolor='#000000' onload='SendDataToPhoneApp()'>
            <div style='color:white' id='textReceived' />
            </body>
            </html>");
        }

        //a14d806b5fed5f1
        //public Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd adControl =  null;
        //public adMob7.adMobRenderer adControl;
        //private bool adLoaded = false;

        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //imgCloud.Visibility = System.Windows.Visibility.Collapsed;
            webBrowser.Visibility = System.Windows.Visibility.Visible;
            #region comment
            //if (!adLoaded)
            //{
            //    if (adControl == null) adControl = new Google.AdMob.Ads.WindowsPhone7.WPF.BannerAd
            //    {
            //        AdUnitID = "a14d806b5fed5f1", 
            //        //GpsLocation = GpsLocationProvider.CurrentLocation
            //    };

            //    adControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //    adControl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //    System.Windows.Thickness thickness = new Thickness(0, 550, 0, 0);
            //    adControl.Margin = thickness;
            //    if (ContentPanel.Children.Contains(adControl)) ContentPanel.Children.Remove(adControl);
            //    ContentPanel.Children.Add(adControl);
            //    adLoaded = true; //if commented out, will reload ad in a dirty way
            //    //MessageBox.Show("hmm");
            //}
            #endregion
        }

        void webBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            #region Call Javascript function..
            //webBrowser.InvokeScript("DataReceivedFromPhoneApp", HttpUtility.HtmlEncode(textToBeDisplayed).Replace("\n", "<br/>").Replace("\r", "<br/>").Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp"));
            //webBrowser.InvokeScript("DataReceivedFromPhoneApp", HttpUtility.HtmlEncode(textToBeDisplayed));
            webBrowser.InvokeScript("DataReceivedFromPhoneApp", textToBeDisplayed);
            #endregion
            //webBrowser.InvokeScript("DataReceivedFromPhoneApp", HttpUtility.HtmlEncode(textToBeDisplayed));

            //Call Mashape
            MashapeWorldCloudCall();

            #region comment
            progressUrl.Visibility = System.Windows.Visibility.Collapsed;
            #endregion
        }

        void MashapeWorldCloudCall()
        {
            var client = new RestClient();
            client.BaseUrl = "https://gatheringpoint-word-cloud-maker.p.mashape.com";

            var request = new RestRequest("index.php", Method.POST);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddHeader("X-Mashape-Authorization", "MWM4bW04OHZ2bGR5ZjNzdnEza2R0enBxYnVmZmw0OmUzYzAzNmYxN2JiOTI4MDc0MGQ2ZmZjMzkwZWY1YTJiMWVmMzNiODc=");
            request.AddParameter("height", "550");
            request.AddParameter("textblock", textToBeDisplayed);
            request.AddParameter("width", "480");
 
            client.ExecuteAsync<WordCloud>(request, (response) => 
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    MessageBox.Show(response.ErrorMessage);
                }
                else
                {
                    GetResponse(response.Data); 
                } 
            });
        }

        private void GetResponse(WordCloud wordCloud)
        {             
            imgCloud.Source = new BitmapImage(new Uri(wordCloud.url));
            webBrowser.Visibility = System.Windows.Visibility.Collapsed;
            imgCloud.Visibility = System.Windows.Visibility.Visible;

            MashapeBitlyCall(wordCloud.url);
        }

        void MashapeBitlyCall(string url)
        {
            var client = new RestClient();
            client.BaseUrl = "https://ismaelc-bitly.p.mashape.com";

            var request = new RestRequest("/v3/shorten?longUrl=" + url + "&login=ismaelc&apikey=R_70085ff324b8f0d7b3ad65f241f73e30", Method.GET);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddHeader("X-Mashape-Authorization", "MWM4bW04OHZ2bGR5ZjNzdnEza2R0enBxYnVmZmw0OmUzYzAzNmYxN2JiOTI4MDc0MGQ2ZmZjMzkwZWY1YTJiMWVmMzNiODc=");

            client.ExecuteAsync<Bitly>(request, (response) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    MessageBox.Show(response.ErrorMessage);
                }
                else
                {
                    GetResponseBitly(response.Data);
                }
            });
        }

        private void GetResponseBitly(Bitly bitly)
        {
            //imgCloud.Source = new BitmapImage(new Uri(wordCloud.url));
            webBrowser.Visibility = System.Windows.Visibility.Collapsed;
            txtBitly.Text = bitly.data.url;

        }

        private void textBoxUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxUrl.SelectionStart = 0;
            textBoxUrl.SelectionLength = textBoxUrl.Text.Length;
        }

        private void textBoxUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxUrl.Text == "") textBoxUrl.Text = "http://";
        }

        void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            App.IsolatedStorageCacheManager<string>.Store("textBoxUrlValue", textBoxUrl.Text);
        }

        void Current_Closing(object sender, ClosingEventArgs e)
        {
            App.IsolatedStorageCacheManager<string>.Store("textBoxUrlValue", textBoxUrl.Text);
        }


        public class GpsLocationProvider 
        { 
            //static GpsLocationProvider() 
            //{ 
            //    try 
            //    { 
            //        CurrentLocation = null; 
            //        GeoCoordinateWatcher Provider = new GeoCoordinateWatcher(); 
            //        Provider.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(GpsLocationProvider_PositionChanged); 
            //        Provider.Start(true); 
            //        System.Diagnostics.Debug.WriteLine("GpsLocationProvider Position: {0}", Provider.Position.Location); 
            //    } 
            //    catch (Exception exception) 
            //    { 
            //        System.Diagnostics.Debug.WriteLine("GpsLocationProvider Exception"); 
            //        System.Diagnostics.Debug.WriteLine(exception); 
            //    } 
            //} 
            
            //private static void GpsLocationProvider_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e) 
            //{ 
            //    if (e.Position.Location.IsUnknown)         
            //        CurrentLocation = null; 
            //    else         
            //        CurrentLocation = new GpsLocation { 
            //            Latitude = e.Position.Location.Longitude, 
            //            Longitude = e.Position.Location.Longitude, 
            //            Accuracy = e.Position.Location.HorizontalAccuracy 
            //        }; 
            //} 
            
            //public static GpsLocation? CurrentLocation { get; private set; } 
       }
    }

    public class WordCloud
    {
        public string url { get; set; }
        public string filename { get; set; }
        public string timestamp { get; set; }
        public bool error { get; set; }
        public string errormsg { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string config { get; set; }
    }

    public class Data
    {
        public string global_hash { get; set; }
        public string hash { get; set; }
        public string long_url { get; set; }
        public int new_hash { get; set; }
        public string url { get; set; }
    }

    public class Bitly
    {
        public Data data { get; set; }
        public int status_code { get; set; }
        public string status_txt { get; set; }
    }
}
