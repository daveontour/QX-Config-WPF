using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WXE.Internal.Tools.ConfigEditor.Common;
using System.Xml;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Windows.Markup;

using Path = System.Windows.Shapes.Path;

namespace WXE.Internal.Tools.ConfigEditor.XMLEditorModule.Views {
    /// <summary>
    /// Interaction logic for CommandsBarView.xaml
    /// </summary>
    public partial class CommandsBarView : UserControl {

        public static MenuItem executeMenuItem;
        public static MenuItem exportMenuItem;
        public static MenuItem saveMenuItem;
        public static MenuItem saveAsMenuItem;

        public CommandsBarView() {
            InitializeComponent();
            this.MenuBar.Items.Clear();
            MenuItem fileMenuItem = new MenuItem { Header = "_File" };
            MenuItem newMenuItem = new MenuItem { Header = "_New" };
            newMenuItem.Click += new RoutedEventHandler(newMenuItem_Click);
            Path newI = GetResourceCopy<Path>("new");
            newMenuItem.Icon = newI;

            MenuItem openMenuItem = new MenuItem { Header = "_Open" };
            openMenuItem.Click += new RoutedEventHandler(openMenuItem_Click);
            Path open = GetResourceCopy<Path>("open");
            openMenuItem.Icon = open;

            saveMenuItem = new MenuItem { Header = "_Save" };
            saveMenuItem.Click += new RoutedEventHandler(saveMenuItem_Click);
            saveMenuItem.IsEnabled = false;
            Path save = GetResourceCopy<Path>("save");
            saveMenuItem.Icon = save;

            saveAsMenuItem = new MenuItem { Header = "Save _As" };
            saveAsMenuItem.Click += new RoutedEventHandler(saveAsMenuItem_Click);
            saveAsMenuItem.IsEnabled = false;
            Path saveas = GetResourceCopy<Path>("save");
            saveAsMenuItem.Icon = saveas;

            exportMenuItem = new MenuItem { Header = "Package and Export" };
            exportMenuItem.Click += new RoutedEventHandler(exportMenuItem_Click);
            exportMenuItem.IsEnabled = false;
            Path export = GetResourceCopy<Path>("export");
            exportMenuItem.Icon = export;


            fileMenuItem.Items.Add(newMenuItem);
            fileMenuItem.Items.Add(openMenuItem);
            fileMenuItem.Items.Add(new Separator());
            fileMenuItem.Items.Add(saveMenuItem);
            fileMenuItem.Items.Add(saveAsMenuItem);
            fileMenuItem.Items.Add(new Separator());
            fileMenuItem.Items.Add(exportMenuItem);
            this.MenuBar.Items.Add(fileMenuItem);

            executeMenuItem = new MenuItem { Header = "_Execute" };
            executeMenuItem.IsEnabled = false;

            Path go = GetResourceCopy<Path>("go");
            executeMenuItem.Icon = go;
            executeMenuItem.Click += new RoutedEventHandler(executeMenuItem_Click);

            this.MenuBar.Items.Add(executeMenuItem);
        }

        public event EventHandler<DocumentLoadedEventArgs> DocumentLoaded;
        public event EventHandler SaveRequested;
        public event EventHandler SaveAsAndExecuteRequested;
        public event EventHandler PackageRequested;
        public event EventHandler<SaveAsEventArgs> SaveAsRequested;

        private void OnDocumentLoaded(object sender, DocumentLoadedEventArgs e) {
            if (DocumentLoaded != null) {
                DocumentLoaded(sender, e);
            }
        }

        #region Menu Click Handlers

        void openMenuItem_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "XML Files (*.xml)|*.xml";
            if (open.ShowDialog() == true) {
                XmlDocument document = new XmlDocument();
                try {
                    document.Load(open.FileName);
                    DocumentLoadedEventArgs args = new DocumentLoadedEventArgs() { Path = open.FileName, Document = document, FileName = open.SafeFileName };
                    OnDocumentLoaded(this, args);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                }

            }
        }

        void newMenuItem_Click(object sender, RoutedEventArgs e) {

            XmlDocument document = new XmlDocument();
            document.LoadXml("<?xml version=\"1.0\" encoding=\"utf - 8\"?><config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <settings> </settings><pipes></pipes></config>"); ;

            try {

                DocumentLoadedEventArgs args = new DocumentLoadedEventArgs() { Path = null, Document = document, FileName = "new.xml*" };
                OnDocumentLoaded(this, args);

            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }

        }

        void saveAsMenuItem_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML Files (*.xml)|*.xml";
            if (dialog.ShowDialog() == true) {
                SaveAsEventArgs args = new SaveAsEventArgs { FileName = dialog.SafeFileName, Path = dialog.FileName };
                if (SaveAsRequested != null) {
                    SaveAsRequested(this, args);
                }
            }
        }

        void saveMenuItem_Click(object sender, RoutedEventArgs e) {

            SaveRequested?.Invoke(this, e);
        }

        void exportMenuItem_Click(object sender, RoutedEventArgs e) {
            if (PackageRequested != null) {
                PackageRequested(this, e);
            }
        }

        void executeMenuItem_Click(object sender, RoutedEventArgs e) {
            if (SaveAsAndExecuteRequested != null) {
                SaveAsAndExecuteRequested(this, e);
            }
        }
        #endregion


        private T GetResourceCopy<T>(string key) {
            T model = (T)FindResource(key);
            return ElementClone<T>(model);
        }
        /// <summary>
        /// Clones an element.
        /// </summary>
        public static T ElementClone<T>(T element) {
            T clone = default(T);
            MemoryStream memStream = ElementToStream(element);
            clone = ElementFromStream<T>(memStream);
            return clone;
        }

        /// <summary>
        /// Saves an element as MemoryStream.
        /// </summary>
        public static MemoryStream ElementToStream(object element) {
            MemoryStream memStream = new MemoryStream();
            XamlWriter.Save(element, memStream);
            return memStream;
        }

        /// <summary>
        /// Rebuilds an element from a MemoryStream.
        /// </summary>
        public static T ElementFromStream<T>(MemoryStream elementAsStream) {
            object reconstructedElement = null;

            if (elementAsStream.CanRead) {
                elementAsStream.Seek(0, SeekOrigin.Begin);
                reconstructedElement = XamlReader.Load(elementAsStream);
                elementAsStream.Close();
            }

            return (T)reconstructedElement;
        }

    }
}
