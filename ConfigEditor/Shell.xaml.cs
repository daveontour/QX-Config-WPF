using QXEditorModule.Common;
using QXEditorModule.ViewModels;
using QXEditorModule.Views;
using System;
using System.Windows;
using System.Xml;

namespace QueueExchange.ConfigEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly TreeEditorsViewModel editorsVM;
        public MainWindow()
        {
            InitializeComponent();
            this.commandBarView.DocumentLoaded += new EventHandler<DocumentLoadedEventArgs>(CommandBarView_DocumentLoaded);

            this.commandBarView.SaveRequested += new EventHandler(CommandBarView_SaveRequested);
            this.commandBarView.SaveAsAndExecuteRequested += new EventHandler(CommandBarView_SaveAsAndExecuteRequested);
            this.commandBarView.PackageRequested += new EventHandler(CommandBarView_PackageRequested);
            this.commandBarView.AboutRequested += new EventHandler(CommandBarView_AboutRequested);
            this.commandBarView.SaveAsRequested += new EventHandler<SaveAsEventArgs>(CommandBarView_SaveAsRequested);
            editorsVM = new TreeEditorsViewModel();

            this.editorsView.ViewModel = editorsVM;

            this.StartUp();


        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            QXSplash splash = new QXSplash
            {
                Owner = this
            };
            splash.ShowDialog();
        }

        void StartUp()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml($"<?xml version=\"1.0\" encoding=\"utf - 8\"?><config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <settings> </settings><pipes><pipe name=\"Name of Pipe\" id=\"{Guid.NewGuid()}\" numInstances=\"1\" ><input type=\"MSMQ\" id=\"{Guid.NewGuid()}\" name=\"Description of the Node\" /><output type=\"MSMQ\" id=\"{Guid.NewGuid()}\" name=\"Description of the Node\" /> </pipe></pipes></config>"); ;
            var xmlTreeViewModel = new TreeEditorViewModel(document, null, "new.xml*");
            editorsVM.Add(xmlTreeViewModel);

        }

        //void commandBarView_SearchRequested(object sender, SearchRequestedEventArgs e)  {          
        //   this.editorsVM.ActiveEditor.FindElementCommand.Execute(e.XPath);
        //}


        void CommandBarView_DocumentLoaded(object sender, DocumentLoadedEventArgs e)
        {
            var xmlTreeViewModel = new TreeEditorViewModel(e.Document, e.Path, e.FileName);
            editorsVM.Add(xmlTreeViewModel);
        }

        void CommandBarView_SaveAsRequested(object sender, SaveAsEventArgs e)
        {
            editorsVM.ActiveEditor.SaveAsDocumentCommand.Execute(e.Path);
        }

        void CommandBarView_SaveRequested(object sender, EventArgs e)
        {
            editorsVM.ActiveEditor.SaveDocumentCommand.Execute(null);
        }

        void CommandBarView_SaveAsAndExecuteRequested(object sender, EventArgs e)
        {
            editorsVM.ActiveEditor.SaveAsAndExecuteCommand.Execute(null);
        }

        void CommandBarView_PackageRequested(object sender, EventArgs e)
        {
            editorsVM.ActiveEditor.PackageCommand.Execute(null);
        }

        void CommandBarView_AboutRequested(object sender, EventArgs e)
        {
            editorsVM.ActiveEditor.AboutCommand.Execute(null);
        }

        private void EditorsView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}