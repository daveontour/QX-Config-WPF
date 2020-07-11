using Microsoft.Win32;
using QXEditorModule.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using Path = System.Windows.Shapes.Path;

namespace QXEditorModule.Views
{
    /// <summary>
    /// Interaction logic for CommandsBarView.xaml
    /// </summary>
    public partial class CommandsBarView : UserControl
    {

        public static MenuItem executeMenuItem;
        public static MenuItem exportMenuItem;
        public static MenuItem saveMenuItem;
        public static MenuItem saveAsMenuItem;
        public static MenuItem aboutMenuItem;

        public CommandsBarView()
        {
            InitializeComponent();
            this.MenuBar.Items.Clear();
            MenuItem fileMenuItem = new MenuItem { Header = "_File" };
            MenuItem newMenuItem = new MenuItem { Header = "_New" };
            newMenuItem.Click += new RoutedEventHandler(NewMenuItem_Click);
            Path newI = GetResourceCopy<Path>("new");
            newMenuItem.Icon = newI;

            MenuItem openMenuItem = new MenuItem { Header = "_Open" };
            openMenuItem.Click += new RoutedEventHandler(OpenMenuItem_Click);
            Path open = GetResourceCopy<Path>("open");
            openMenuItem.Icon = open;

            saveMenuItem = new MenuItem { Header = "_Save" };
            saveMenuItem.Click += new RoutedEventHandler(SaveMenuItem_Click);
            saveMenuItem.IsEnabled = false;
            Path save = GetResourceCopy<Path>("save");
            saveMenuItem.Icon = save;

            saveAsMenuItem = new MenuItem { Header = "Save _As" };
            saveAsMenuItem.Click += new RoutedEventHandler(SaveAsMenuItem_Click);
            saveAsMenuItem.IsEnabled = false;
            Path saveas = GetResourceCopy<Path>("save");
            saveAsMenuItem.Icon = saveas;

            exportMenuItem = new MenuItem { Header = "Package and Export" };
            exportMenuItem.Click += new RoutedEventHandler(ExportMenuItem_Click);
            exportMenuItem.IsEnabled = false;
            Path export = GetResourceCopy<Path>("export");
            exportMenuItem.Icon = export;

            aboutMenuItem = new MenuItem { Header = "About" };
            aboutMenuItem.Click += new RoutedEventHandler(AboutMenuItem_Click);
            aboutMenuItem.IsEnabled = true;


            fileMenuItem.Items.Add(newMenuItem);
            fileMenuItem.Items.Add(openMenuItem);
            fileMenuItem.Items.Add(new Separator());
            fileMenuItem.Items.Add(saveMenuItem);
            fileMenuItem.Items.Add(saveAsMenuItem);
            fileMenuItem.Items.Add(new Separator());
            fileMenuItem.Items.Add(exportMenuItem);
            fileMenuItem.Items.Add(new Separator());
            fileMenuItem.Items.Add(aboutMenuItem);

            this.MenuBar.Items.Add(fileMenuItem);

            executeMenuItem = new MenuItem { Header = "_Execute" };
            executeMenuItem.IsEnabled = false;

            Path go = GetResourceCopy<Path>("go");
            executeMenuItem.Icon = go;
            executeMenuItem.Click += new RoutedEventHandler(ExecuteMenuItem_Click);

            this.MenuBar.Items.Add(executeMenuItem);
        }

        public event EventHandler<DocumentLoadedEventArgs> DocumentLoaded;
        public event EventHandler SaveRequested;
        public event EventHandler SaveAsAndExecuteRequested;
        public event EventHandler PackageRequested;
        public event EventHandler AboutRequested;
        //public event EventHandler<SaveAsEventArgs> SaveAs;
        public event EventHandler<SaveAsEventArgs> SaveAsRequested;

        private void OnDocumentLoaded(object sender, DocumentLoadedEventArgs e)
        {
            DocumentLoaded?.Invoke(sender, e);
        }

        #region Menu Click Handlers

        void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml"
            };
            if (open.ShowDialog() == true)
            {
                XmlDocument document = new XmlDocument();
                try
                {
                    document.Load(open.FileName);
                    DocumentLoadedEventArgs args = new DocumentLoadedEventArgs() { Path = open.FileName, Document = document, FileName = open.SafeFileName };
                    OnDocumentLoaded(this, args);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }

        void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {

            XmlDocument document = new XmlDocument();
            document.LoadXml($"<?xml version=\"1.0\" encoding=\"utf - 8\"?><config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <settings> </settings><pipes><pipe name=\"Name of Pipe\" id=\"{Guid.NewGuid()}\" numInstances=\"1\" ><input type=\"MSMQ\" id=\"{Guid.NewGuid()}\" name=\"Description of the Node\" /><output type=\"MSMQ\" id=\"{Guid.NewGuid()}\" name=\"Description of the Node\" /></pipe></pipes></config>"); ;

            try
            {

                DocumentLoadedEventArgs args = new DocumentLoadedEventArgs() { Path = null, Document = document, FileName = "new.xml*" };
                OnDocumentLoaded(this, args);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml"
            };
            if (dialog.ShowDialog() == true)
            {
                SaveAsEventArgs args = new SaveAsEventArgs { FileName = dialog.SafeFileName, Path = dialog.FileName };
                SaveAsRequested?.Invoke(this, args);
            }
        }

        void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveRequested?.Invoke(this, e);
        }

        void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PackageRequested?.Invoke(this, e);
        }

        void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutRequested(this, e);
        }

        void ExecuteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveAsAndExecuteRequested?.Invoke(this, e);
        }
        #endregion


        private T GetResourceCopy<T>(string key)
        {
            T model = (T)FindResource(key);
            return ElementClone<T>(model);
        }
        /// <summary>
        /// Clones an element.
        /// </summary>
        public static T ElementClone<T>(T element)
        {
            T clone = default(T);
            MemoryStream memStream = ElementToStream(element);
            clone = ElementFromStream<T>(memStream);
            return clone;
        }

        /// <summary>
        /// Saves an element as MemoryStream.
        /// </summary>
        public static MemoryStream ElementToStream(object element)
        {
            MemoryStream memStream = new MemoryStream();
            XamlWriter.Save(element, memStream);
            return memStream;
        }

        /// <summary>
        /// Rebuilds an element from a MemoryStream.
        /// </summary>
        public static T ElementFromStream<T>(MemoryStream elementAsStream)
        {
            object reconstructedElement = null;

            if (elementAsStream.CanRead)
            {
                elementAsStream.Seek(0, SeekOrigin.Begin);
                reconstructedElement = XamlReader.Load(elementAsStream);
                elementAsStream.Close();
            }

            return (T)reconstructedElement;
        }

    }
}
