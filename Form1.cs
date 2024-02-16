using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace UMC_Hydroelectricity
{
    public partial class Form1 : Form
    {
        public Form1()
        {
           // webBrowser1.ScriptErrorsSuppressed = true;
            InitializeComponent();
            // Initialize WebView2 (Call this only once in the application)
            webView21.EnsureCoreWebView2Async();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {


        }
    }
}
