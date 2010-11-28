﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Phone.Tasks;

namespace Hacker_News
{
    public class News : INotifyPropertyChanged
    {
        private string nextIdValue = String.Empty;
        private string versionValue = String.Empty;
        private ObservableCollection<Article> itemsValue = new ObservableCollection<Article>();

        #region I don't really understand what's going on here ...
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        public string nextId
        {
            get { return this.nextIdValue; }
            set { this.nextIdValue = value; NotifyPropertyChanged("nextId"); }
        }
        public string version
        {
            get { return this.versionValue; }
            set { this.versionValue = value; NotifyPropertyChanged("version"); }
        }

        public ObservableCollection<Article> items
        {
            get { return this.itemsValue; }
            set { this.itemsValue = value; NotifyPropertyChanged("items"); }
        }
    }
    public class Article
    {
        Uri urlValue = null;
        public string title { get; set; }
        public string urlDomainOnly
        {
            get
            {
                if (this.urlValue == null) { return ""; }
                string[] hostParts = this.urlValue.Host.Split('.');
                int len = hostParts.Length;
                if (len > 1)
                {
                    return hostParts[len - 2] + "." + hostParts[len - 1];
                }
                else
                {
                    return this.urlValue.Host;
                }
            }
        }
        public string url { get; set; }
        public int id { get; set; }
        public int commentCount { get; set; }
        public int points { get; set; }
        public string postedAgo { get; set; }
        public string postedBy { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        public News news_data = new News();

        public void setProgressBar(Boolean state)
        {
            progressBar.IsIndeterminate = state;
            progressBar.Visibility = (state == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandleNewsResult(IAsyncResult result)
        {
            Common common = new Common();
            var binding = (result.AsyncState as AsyncState).binding as News;
            StreamReader txt = common.makeStreamReaderFromResult(result);

            News rv = common.deserializeStreamReader<News>(txt);
            this.Dispatcher.BeginInvoke(
            () =>
            {
                binding.nextId = rv.nextId;
                binding.version = rv.version;
                binding.items = rv.items;
                // FIXME: Shouldn't I be able to do this instead?
                //binding = processJsonString(txt);
                setProgressBar(false);
            }
            );
        }

        public void populateBinding(News binding, string Url)
        {
            AsyncState state = new AsyncState();
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            request.Accept = "application/json"; //atom+xml";
            state.request = request;
            state.binding = binding;
            request.BeginGetResponse(HandleNewsResult, state);
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            setProgressBar(true);

            news.DataContext = news_data;

            populateBinding(news_data, "http://api.ihackernews.com/page");
            // populatePageWithUrl(newest, "http://api.ihackernews.com/new");
            // populatePageWithUrl(comments, "http://api.ihackernews.com/newcomments");
            // populatePageWithUrl(ask, "http://api.ihackernews.com/ask");

        }

        private void title_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected = ((Hacker_News.Article)(((System.Windows.FrameworkElement)(sender)).DataContext));
            Post.id = selected.id;
            NavigationService.Navigate(new Uri("/Post.xaml", UriKind.Relative));

            // WebBrowserTask webBrowserTask = new WebBrowserTask();
            // webBrowserTask.URL = selected_url;
            // webBrowserTask.Show();
            Browser.url = selected.url;
            NavigationService.Navigate(new Uri("/Browser.xaml", UriKind.Relative));

        }
    }
}
